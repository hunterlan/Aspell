using System.Collections.Generic;

namespace Infrastructure
{
    public interface IUtils
    {
        public string LoadFile(string pathToFile);

        public List<string> ReplaceDirectoriesByFiles(List<string> pathToDirectories);

        public bool IsDocumentType(string fileExtension);

        public bool IsWordHexadecimal(string word);

        public bool IsXmlComments(string sourceCode);
    }   
}