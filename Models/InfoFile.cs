using System.Collections.Generic;

namespace Models
{
    public class InfoFile
    {
        public InfoFile(List<uint> lineErrors, List<string> wordsWithMistake)
        {
            LineErrors = lineErrors;
            WordsWithMistake = wordsWithMistake;
        }

        public string FileName { get; init; }
        
        public List<uint> LineErrors { get; } 
        
        public List<string> WordsWithMistake { get; }
    }
}