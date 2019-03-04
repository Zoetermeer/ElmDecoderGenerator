using System;
using System.Collections.Generic;
using System.Text;

namespace ElmDecoderGenerator.Ast {
    public class RecordDef : ElmAstNode {
        public RecordDef(string name) {
            Name = name;
        }
        
        public string Name { get; }
        public Dictionary<string, string> Fields { get; } = new Dictionary<string, string>();
        
        public override void Write(CodeBuffer buf) {
            buf.Append("type alias ").Append(Name).AppendLine(" =");
            buf.Indented(() => {
              var i = 0;
              foreach (var field in Fields) {
                  if (i == 0) {
                      buf.Append("{ ");
                  }
                  else {
                      buf.Append(", ");
                  }

                  buf.Append(field.Key).Append(" : ").AppendLine(field.Value);
                  i++;
              }
              
              buf.AppendLine("}");
            });
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append($"type alias {Name} =");
            sb.AppendLine();

            var i = 0;
            foreach (var field in Fields) {
                if (i == 0) {
                    sb.Append($"  {{ {field.Key} : {field.Value} ");
                }
                else {
                    sb.Append($"  , {field.Key} : {field.Value}");
                }

                sb.AppendLine();
                i++;
            }

            sb.Append("  }");
            sb.AppendLine();

            return sb.ToString();
        }
    }
}