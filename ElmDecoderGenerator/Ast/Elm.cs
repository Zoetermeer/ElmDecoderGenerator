using System.Net.Http.Headers;

namespace ElmDecoderGenerator.Ast {
    public static class Elm {
        public static class Types {
          public const string BOOL = "Bool";
          public const string FLOAT = "Float";
          public const string INT = "Int";
          public const string STRING = "String";
          public const string DECODER = "Decoder";
          public const string LIST = "List";
          public const string TIME_POSIX = "Time.Posix";
          public const string MAYBE = "Maybe";
        }

        public static class Decoders {
            public const string BOOL = "bool";
            public const string FLOAT = "float";
            public const string INT = "int";
            public const string STRING = "string";
            public const string LIST = "list";
            public const string ANDTHEN = "andThen";
        }
    }
}