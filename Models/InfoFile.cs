using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class InfoFile
    {
        public InfoFile(List<uint> lineErrors, List<string> content, string fileName)
        {
            LineErrors = lineErrors;
            Content = content;
            FileName = fileName;
        }

        public string FileName { get; }
        
        public List<uint> LineErrors { get; } 
        
        public List<string> Content { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < Content.Count; i++)
            {
                sb.Append($"In file {FileName} error on line {LineErrors[i]}: {Content[i]}\n");
            }

            return sb.ToString();
        }
    }
}