using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Infrastructure
{
    /// <inheritdoc/>
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
        
        /// <summary>
        /// Give list of files from a provided directory.
        /// </summary>
        /// <param name="pathToDirectory">Path to the directory</param>
        /// <returns>Array of paths to the files from the directory.</returns>
        private IEnumerable<string> GetFilesFromDirectory(string pathToDirectory)
        {
            return Directory.GetFiles(pathToDirectory);
        }
        
        /// <summary>
        /// Method check if provided path is a directory.
        /// </summary>
        /// <param name="filename">Path</param>
        /// <returns>Result of the check.</returns>
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