using System;
using System.Collections.Generic;
using System.Text;

namespace ElmDecoderGenerator.Ast {
    public class CaseExp : ElmAstNode {
        public CaseExp(ElmAstNode exp) {
            Exp = exp;
        }
        
        public ElmAstNode Exp { get; }
        public List<Tuple<ElmAstNode, ElmAstNode>> Cases { get; } = new List<Tuple<ElmAstNode,ElmAstNode>>();
        
        public override void Write(CodeBuffer buf) {
            buf.Append("case ").Append(Exp).AppendLine(" of");
            buf.Indent();
            foreach (var (pred, body) in Cases) {
                buf.Append(pred).AppendLine(" ->");
                buf.Indented(() => buf.AppendLine(body));
            }
            
            buf.DeIndent();
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendLine($"case {Exp} of");
            foreach (var (pred, body) in Cases) {
                sb.AppendLine($"  {pred} ->");
                sb.AppendLine($"    {body}");
            }

            return sb.ToString();
        }
    }
}