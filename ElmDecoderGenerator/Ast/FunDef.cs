using System.Collections.Generic;
using System.Text;

namespace ElmDecoderGenerator.Ast {
    public class FunDef : ElmAstNode {
        public FunDef(string name, string type) {
            Name = name;
            Type = type;
        }
        
        public string Name { get; }
        public string Type { get; }
        public List<string> Params { get; } = new List<string>();
        public ElmAstNode Body { get; set; }
        
        public override void Write(CodeBuffer buf) {
            buf.AppendLine(Type);

            var headerItems = new List<string>();
            headerItems.Add(Name);
            headerItems.AddRange(Params);
            headerItems.Add("=");
            buf.AppendLine(string.Join(' ', headerItems));
            buf.Indented(() => buf.AppendLine(Body));
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendLine(Type);
            sb.AppendLine($"{Name} {string.Join(' ', Params)} =");
            sb.AppendLine(Body.ToString());

            return sb.ToString();
        }
    }
}