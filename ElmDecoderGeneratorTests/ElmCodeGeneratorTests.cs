using System;
using System.Collections.Generic;
using System.Reflection;
using ElmDecoderGenerator;
using Newtonsoft.Json;
using Xunit;

namespace ElmDecoderGeneratorTests {
    public class ElmCodeGeneratorTests {
        [JsonObject]
        public class SampleObject {
            [JsonProperty("id")]
            public int Id { get; }

            [JsonProperty("stuff")]
            public List<string> Stuff { get; } = new List<string>();
        }

        public enum SampleEnum {
            Option1,
            Option2,
            Option3
        }
        
        [Fact]
        public void ItGeneratesRecordDefs() {
            var gen = new ElmCodeGenerator(Assembly.GetExecutingAssembly());
            gen.AddTypeDef(typeof(SampleObject));

            var code = gen.GetCode();
            var expected = 
@"type alias SampleObject =
  { id : Int
  , stuff : List String
  }

";
            
            Assert.Equal(expected, code);
        }

        [Fact]
        public void ItGeneratesEnumDecoders() {
            var gen = new ElmCodeGenerator(Assembly.GetExecutingAssembly());
            gen.AddDecodeFun(typeof(SampleEnum));

            var code = gen.GetCode();
            var expected =
@"decodeSampleEnumValue : String -> Decoder SampleEnum
decodeSampleEnumValue s =
  case s of
    ""Option1"" ->
      Decode.succeed SampleEnumOption1
    ""Option2"" ->
      Decode.succeed SampleEnumOption2
    ""Option3"" ->
      Decode.succeed SampleEnumOption3
    _ ->
      Decode.fail (""Unknown value: "" ++ s)


decodeSampleEnum : Decoder SampleEnum
decodeSampleEnum =
  string
  |> andThen decodeSampleEnumValue


";
            
            Assert.Equal(expected, code);
        }
    }
}