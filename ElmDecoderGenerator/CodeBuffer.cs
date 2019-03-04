using System;
using System.Text;
using ElmDecoderGenerator.Ast;

namespace ElmDecoderGenerator {
    public class CodeBuffer {
        private StringBuilder sb = new StringBuilder();
        private bool inline = false;
        private int indent;

        public void Indent() {
            indent += 2;
        }

        public void DeIndent() {
            indent = Math.Max(0, indent - 2);
        }

        public void Indented(Action thunk) {
            Indent();
            thunk();
            DeIndent();
        }
        
        public CodeBuffer AppendLine(string line) {
            var str = inline ? line : line.PadLeft(indent + line.Length);
            sb.AppendLine(str);
            inline = false;
            return this;
        }

        public CodeBuffer AppendLine() {
            sb.AppendLine();
            inline = false;
            return this;
        }

        public CodeBuffer AppendLine(ElmAstNode node) {
            node.Write(this);
            sb.AppendLine();
            inline = false;
            return this;
        }

        public CodeBuffer Append(string code) {
            var str = inline ? code : code.PadLeft(indent + code.Length);
            sb.Append(str);
            inline = true;
            return this;
        }

        public CodeBuffer Append(ElmAstNode node) {
            node.Write(this);
            inline = true;
            return this;
        }

        public override string ToString() {
            return sb.ToString();
        }
    }
}