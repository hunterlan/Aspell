using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class ResultProcessingFile
    {
        public ResultProcessingFile(bool errorOccured, string textError, string fileName)
        {
            IsErrorOccured = errorOccured;
            TextError = textError;
            FileName = fileName;
        }
        public ResultProcessingFile(List<uint> lineErrors, List<string> content, string fileName)
        {
            LineErrors = lineErrors;
            Content = content;
            FileName = fileName;
            IsErrorOccured = false;
        }
        
        public bool IsErrorOccured { get; }
        
        public string TextError { get; }

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