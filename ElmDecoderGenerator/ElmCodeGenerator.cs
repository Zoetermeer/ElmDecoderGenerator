using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Reflection;
using System.Text;
using ElmDecoderGenerator.Ast;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json;

namespace ElmDecoderGenerator {
    public class ElmCodeGenerator {
        private Assembly assembly;
        private string elmModuleName;

        private const BindingFlags PROP_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;

        public ElmCodeGenerator(Assembly assembly, string elmModuleName) {
            this.assembly = assembly;
            this.elmModuleName = elmModuleName;
        }

        public Dictionary<string, ElmAstNode> TypeDefs { get; } = new Dictionary<string, ElmAstNode>();
        public Dictionary<string, ElmAstNode> FunDefs { get; } = new Dictionary<string, ElmAstNode>();

        public void Generate() {
          var jsonTypes = from t in assembly.GetExportedTypes()
              where IsJsonType(t)
              select t;

          foreach (var t in jsonTypes) {
              AddTypeDef(t);
              AddDecodeFun(t);
          }
        }

        public void AddTypeDef(Type type) {
            string name = type.Name;
            if (TypeDefs.ContainsKey(name)) return;
            
            ElmAstNode def;
            if (type.IsEnum) {
                var t = new AdtDef(name);
                t.Alternatives.AddRange(Array.ConvertAll(type.GetEnumNames(), altName => AdtAlternativeName(type, altName)));
                def = t;
            }
            else {
                //Otherwise, a class
                var t = new RecordDef(name);
                def = t;
                foreach (var prop in type.GetProperties(PROP_FLAGS)) {
                    var jsonAttrs = prop.GetCustomAttributes(typeof(JsonPropertyAttribute));
                    if (jsonAttrs.Any()) {
                        t.Fields.Add(CamelCase(prop.Name), ElmType(prop.PropertyType));
                        
                        //If the property type is an enum, generate an ADT for it
                        if (prop.PropertyType.IsEnum) {
                            AddTypeDef(prop.PropertyType);
                            AddDecodeFun(prop.PropertyType);
                            AddToStringFun(prop.PropertyType);
                        }
                    } 
                }
            }
            
            TypeDefs.Add(name, def);
        }

        private void AddToStringFun(Type type) {
            if (!type.IsEnum) return;
            
            var funName = ToStringFunName(type);
            if (FunDefs.ContainsKey(funName)) return;
            
            var caseExp = new CaseExp(new Id("v"));
            foreach (var alt in type.GetEnumNames()) {
                caseExp.Cases.Add(new Tuple<ElmAstNode, ElmAstNode>(
                    new Id(AdtAlternativeName(type, alt)),
                    new StringLiteral(alt)
                    ));
            }

            var fun = new FunDef(funName, $"{funName} : {type.Name} -> {Elm.Types.STRING}");
            fun.Params.Add("v");
            fun.Body = caseExp;
            
            FunDefs.Add(funName, fun);
        }

        public void AddDecodeFun(Type type) {
            var funName = DecodeFunName(type);
            if (FunDefs.ContainsKey(funName))
                return;
            
            var f = new FunDef(funName, $"{funName} : {Elm.Types.DECODER} {type.Name}");
            if (type.IsEnum) {
                var innerFunName = $"{funName}Value";
                if (FunDefs.ContainsKey(innerFunName))
                    return;
                
                
                var innerF = new FunDef(innerFunName, $"{innerFunName} : {Elm.Types.STRING} -> {Elm.Types.DECODER} {type.Name}");
                innerF.Params.Add("s");
                
                var caseExp = new CaseExp(new Id("s"));
                innerF.Body = caseExp;
                foreach (var alt in type.GetEnumNames()) {
                    caseExp.Cases.Add(new Tuple<ElmAstNode, ElmAstNode>(
                        new StringLiteral(alt),
                        new FunApp(new Id("Decode.succeed")) { Rands = { new Id(AdtAlternativeName(type, alt)) }}));
                }

                caseExp.Cases.Add(new Tuple<ElmAstNode, ElmAstNode>(
                    new Id("_"),
                    new FunApp(new Id("Decode.fail")) {Rands = {new Id("(\"Unknown value: \" ++ s)")}}));
                
                FunDefs.Add(innerFunName, innerF);
                
                //Outer fun
                f.Body = new PipelineExpr() {
                    Exprs = {
                      new Id(Elm.Decoders.STRING),
                      new FunApp(new Id(Elm.Decoders.ANDTHEN)) {Rands = {new Id(innerFunName)}}
                    }
                };
            }
            else {
                var pipeline = new PipelineExpr();
                pipeline.Exprs.Add(new FunApp(new Id("Decode.succeed")) {Rands = {new Id(type.Name)}});
                foreach (var prop in type.GetProperties(PROP_FLAGS)) {
                    var jsonAttrs = prop.GetCustomAttributes(typeof(JsonPropertyAttribute));
                    var jsonAttr = jsonAttrs.FirstOrDefault() as JsonPropertyAttribute;
                    if (null != jsonAttr) {
                        var jsonFieldName = jsonAttr.PropertyName ?? SnakeCase(prop.Name);
                        pipeline.Exprs.Add(new FunApp(new Id("required")) { Rands = { new StringLiteral(jsonFieldName), new Id(DecodeFunName(prop.PropertyType)) } });
                    } 
                }

                f.Body = pipeline;
            }
            
            FunDefs.Add(funName, f);
        }

        public string GetCode() {
            var buf = new CodeBuffer();
            foreach (var entry in TypeDefs) {
                buf.AppendLine($"-- Type: {entry.Key}");
                entry.Value.Write(buf);
                buf.AppendLine();
            }

            foreach (var entry in FunDefs) {
                buf.AppendLine($"-- Function: {entry.Key}");
                entry.Value.Write(buf);
                buf.AppendLine();
            }

            var sb = new StringBuilder();
            sb.AppendLine($"module {elmModuleName} exposing (..)");
            sb.AppendLine(
                "import Json.Decode as Decode exposing (Decoder, andThen, bool, field, float, int, list, nullable, string)");
            sb.AppendLine("import Json.Decode.Pipeline exposing (..)");
            sb.AppendLine("import Time exposing (..)");
            sb.AppendLine();
            sb.AppendLine(
                @"
decodePosixTime : Decoder Time.Posix
decodePosixTime =
  int |> andThen (millisToPosix >> Decode.succeed)
");
            
            sb.Append(buf);

            return sb.ToString();
        }

        private static bool IsJsonType(Type type) {
            return type.GetCustomAttributes().OfType<JsonObjectAttribute>().Any();
        }

        private static string AdtAlternativeName(Type enumType, string altName) {
            return $"{enumType.Name}{altName}";
        }

        private static string ElmType(Type type) {
            if (type == typeof(int)) {
                return Elm.Types.INT;
            }
            
            if (type == typeof(double)) {
                return Elm.Types.FLOAT;
            }

            if (type == typeof(Single)) {
                return Elm.Types.FLOAT;
            }
            
            if (type == typeof(string)) {
                return Elm.Types.STRING;
            }
            
            if (type == typeof(bool)) {
                return Elm.Types.BOOL;
            }

            if (type == typeof(DateTime)) {
                return Elm.Types.TIME_POSIX;
            }
            
            if (type.IsPrimitive)
                throw new ArgumentException($".NET type {type.Name} has no Elm type mapping.");


            if (type.IsGenericType) {
                Type genType = type.GetGenericTypeDefinition();
                if (genType == typeof(Nullable<>)) {
                    return $"{Elm.Types.MAYBE} {ElmType(type.GetGenericArguments()[0])}";
                }

            }
            Type elementType = null;
            if (GetCollectionElementType(type, out elementType)) {
                return $"({Elm.Types.LIST} {ElmType(elementType)})";
            }

            return type.Name;
        }

        private static bool GetCollectionElementType(Type type, out Type elementType) {
            if (type.IsArray) {
                elementType = type.GetElementType();
                return true;
            }
            
            foreach (var ifaceTy in type.GetInterfaces()) {
                if (ifaceTy.IsGenericType && ifaceTy.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
                    elementType = ifaceTy.GetGenericArguments()[0];
                    return true;
                }
            }

            elementType = null;
            return false;
        }

        private static string DecodeFunName(Type type) {
            if (type == typeof(int)) {
                return Elm.Decoders.INT;
            }
            
            if (type == typeof(double)) {
                return Elm.Decoders.FLOAT;
            }
            
            if (type == typeof(Single)) {
                return Elm.Decoders.FLOAT;
            }
            
            if (type == typeof(string)) {
                return Elm.Decoders.STRING;
            }
            
            if (type == typeof(bool)) {
                return Elm.Decoders.BOOL;
            }

            if (type == typeof(DateTime)) {
                return "decodePosixTime";
            }
            
            if (type.IsPrimitive)
                throw new ArgumentException($".NET type {type.Name} has no Elm type mapping.");

            if (type.IsGenericType) {
                Type genType = type.GetGenericTypeDefinition();
                if (genType == typeof(Nullable<>)) {
                    return $"(nullable {DecodeFunName(type.GetGenericArguments()[0])})";
                }

            }
            
            Type elementType = null;
            if (GetCollectionElementType(type, out elementType)) {
                return $"({Elm.Decoders.LIST} {DecodeFunName(elementType)})";
            }

            return $"decode{type.Name}";
        }

        private static string ToStringFunName(Type type) {
            return $"{CamelCase(type.Name)}ToString";
        }

        private static string CamelCase(string name) {
            if (string.IsNullOrEmpty(name)) return name;
            var fst = name[0];

            return name.Substring(1).Insert(0, char.ToLower(fst).ToString());
        }

        private static string SnakeCase(string name) {
            return string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
 
        }
    }
}