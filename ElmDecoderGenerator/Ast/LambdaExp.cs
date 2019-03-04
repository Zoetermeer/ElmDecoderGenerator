using System.Collections.Generic;
using System.Text;

namespace ElmDecoderGenerator.Ast {
    public class LambdaExp : ElmAstNode {
        public LambdaExp(string param1) {
            Params.Add(param1);
        }

        public List<string> Params { get; } = new List<string>();
        public ElmAstNode Body { get; set; }
        
        public override void Write(CodeBuffer buf) {
            buf.AppendLine($"(\\{string.Join(' ', Params)} ->");
            buf.Indented(() => buf.AppendLine(Body));
            buf.AppendLine(")");
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendLine($"(\\{string.Join(' ', Params)} ->");
            sb.AppendLine($"    {Body}");
            sb.AppendLine(")");

            return sb.ToString();
        }
    }
}