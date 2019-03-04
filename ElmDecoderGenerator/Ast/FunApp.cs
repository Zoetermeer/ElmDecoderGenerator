using System.Collections.Generic;

namespace ElmDecoderGenerator.Ast {
    public class FunApp : ElmAstNode {
        public FunApp() {
            
        }

        public FunApp(ElmAstNode rator) {
            Rator = rator;
        }
        
        public ElmAstNode Rator { get; set; }
        public List<ElmAstNode> Rands { get; } = new List<ElmAstNode>();
        
        public override void Write(CodeBuffer buf) {
            buf.Append(Rator).Append(" ").Append(string.Join(' ', Rands.ConvertAll(n => n.ToString())));
        }

        public override string ToString() {
            return $"{Rator} {string.Join(' ', Rands.ConvertAll(n => n.ToString()))}";
        }
    }
}