using System.Collections.Generic;
using System.Text;

namespace ElmDecoderGenerator.Ast {
    public class AdtDef : ElmAstNode {
        public AdtDef(string name) {
            this.Name = name;
        }
        
        public string Name { get; }
        public List<string> Alternatives { get; } = new List<string>();
        
        public override void Write(CodeBuffer buf) {
            buf.Append("type ").Append(Name).AppendLine(" =");
            buf.Indented(() => {
              for (var i = 0; i < Alternatives.Count; i++) {
                  var altName = Alternatives[i];
                  if (i > 0) {
                      buf.Append("| ").AppendLine(altName);
                  }
                  else {
                      buf.Indented(() => buf.AppendLine(altName));
                  }
              }
            });

        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append($"type {Name} =");
            sb.AppendLine();
            for (var i = 0; i < Alternatives.Count; i++) {
                var altName = Alternatives[i];
                if (i == 0) {
                    sb.Append($"    {altName}");
                }
                else {
                    sb.Append($"  | {altName}");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}