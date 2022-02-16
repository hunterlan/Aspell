using System.Collections.Generic;

namespace Infrastructure
{
    /// <summary>
    /// This class helps to get need information without breaking structure of the others classes.
    /// </summary>
    public interface IUtils
    {
        /// <summary>
        /// Method load content of a file.
        /// </summary>
        /// <param name="pathToFile">Path to the file</param>
        /// <returns>Content from the file.</returns>
        public string LoadFile(string pathToFile);

        /// <summary>
        /// Method will give new files, if was provided path to the directory.
        /// </summary>
        /// <param name="pathToDirectories">List of paths.</param>
        /// <returns>New list of paths to files.</returns>
        public List<string> ReplaceDirectoriesByFiles(List<string> pathToDirectories);

        /// <summary>
        /// Method check, if provided file extension is document
        /// </summary>
        /// <param name="fileExtension">File extension.</param>
        /// <returns>Result of the check.</returns>
        public bool IsDocumentType(string fileExtension);

        /// <summary>
        /// Method detect hexadecimal numbers
        /// </summary>
        /// <param name="word">Provided word</param>
        /// <returns>Result of the check.</returns>
        public bool IsWordHexadecimal(string word);

        /// <summary>
        /// Method check is comments contain XML comments.
        /// </summary>
        /// <param name="sourceCode">Provided source code.</param>
        /// <returns>Result of the check.</returns>
        public bool IsXmlComments(string sourceCode);
    }   
}