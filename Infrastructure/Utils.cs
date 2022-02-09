using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Infrastructure
{
    public class Utils : IUtils
    {
        // TODO: Read file parts
        public string LoadFile(string pathToFile)
        {
            using var sr = new StreamReader(pathToFile); 
            var fileData = sr.ReadToEnd();
            
            return fileData;
        }
        
        public List<string> ReplaceDirectoriesByFiles(List<string> pathToDirectories)
        {
            List<string> pathToFiles = new();

            foreach (var path in pathToDirectories)
            {
                if (IsPathDirectory(path))
                {
                    pathToFiles.AddRange(GetFilesFromDirectory(path));
                }
                else
                {
                    pathToFiles.Add(path);
                }
            }

            return pathToFiles;
        }
        
        private string[] GetFilesFromDirectory(string pathToDirectory)
        {
            return Directory.GetFiles(pathToDirectory);
        }
        
        private bool IsPathDirectory(string filename)
        {
            return Directory.Exists(filename);
        }

        public bool IsDocumentType(string fileExtension)
        {
            return fileExtension.Contains(".doc") || fileExtension == ".odt";
        }

        public bool IsWordHexadecimal(string word)
        {
            const string hexademicalStrRegex = @"0x.{1,}";
            Regex regex = new(hexademicalStrRegex);

            return regex.IsMatch(word);
        }

        public bool IsXmlComments(string sourceCode)
        {
            return sourceCode.Contains("///");
        }
    }   
}