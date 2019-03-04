namespace ElmDecoderGenerator.Ast {
    public class Id : ElmAstNode {
        public Id(string id) {
            Name = id;
        }
        
        public string Name { get; }
        
        public override void Write(CodeBuffer buf) {
            buf.Append(Name);
        }

        public override string ToString() {
            return Name;
        }
    }
}