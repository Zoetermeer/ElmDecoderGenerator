namespace ElmDecoderGenerator.Ast {
    public class StringLiteral : ElmAstNode {
        public StringLiteral(string content) {
            Content = content;
        }
        
        public string Content { get; }

        public override void Write(CodeBuffer buf) {
            buf.Append($"\"{Content}\"");
        }

        public override string ToString() {
            return $"\"{Content}\"";
        }
    }
}