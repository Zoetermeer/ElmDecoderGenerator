using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Internal;

namespace ElmDecoderGenerator.Ast {
    public class TupleExp : ElmAstNode {
        public TupleExp(params ElmAstNode[] elements) {
            Elements.AddRange(elements);
        }

        public List<ElmAstNode> Elements { get; } = new List<ElmAstNode>();

        public override void Write(CodeBuffer buf) {
            buf.Append("( ");
            buf.Append(Elements.ConvertAll(e => e.ToString()).Join(", "));
            buf.Append(" )");
        }

        public override string ToString() {
            var buf = new CodeBuffer();
            Write(buf);

            return buf.ToString();
        }
    }
}