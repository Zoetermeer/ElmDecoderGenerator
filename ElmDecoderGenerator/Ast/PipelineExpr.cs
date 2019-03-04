using System.Collections.Generic;
using System.Text;

namespace ElmDecoderGenerator.Ast {
    public class PipelineExpr : ElmAstNode {
        public PipelineExpr() {
            
        }

        public List<ElmAstNode> Exprs { get; } = new List<ElmAstNode>();

        public override void Write(CodeBuffer buf) {
            for (var i = 0; i < Exprs.Count; i++) {
                var expr = Exprs[i];
                if (i == 0) {
                    buf.AppendLine(expr.ToString());
                    continue;
                }

                buf.Append("|> ").AppendLine(expr);
            }
        }

        public override string ToString() {
            var sb = new StringBuilder();
            for (var i = 0; i < Exprs.Count; i++) {
                var expr = Exprs[i];
                if (i == 0) {
                    sb.AppendLine(expr.ToString());
                    continue;
                }

                sb.AppendLine($"|> {expr}");
            }
            
            return sb.ToString();
        }
    }
}