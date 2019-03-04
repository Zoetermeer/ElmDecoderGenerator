namespace ElmDecoderGenerator.Ast {
    public abstract class ElmAstNode {
        public abstract void Write(CodeBuffer buf);
    }
}