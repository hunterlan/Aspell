using System.Collections.Generic;
using System.Text;

namespace Models
{
    /// <summary>
    /// Model class, which save result of checking file
    /// </summary>
    public class ResultProcessingFile
    {
        /// <summary>
        /// If during checking was error, program call this function.
        /// </summary>
        /// <param name="errorOccured">Is there error?</param>
        /// <param name="textError">What kind of error?</param>
        /// <param name="fileName">A file, which was checking</param>
        public ResultProcessingFile(bool errorOccured, string textError, string fileName)
        {
            IsErrorOccured = errorOccured;
            TextError = textError;
            FileName = fileName;
        }
        /// <summary>
        /// If checking was successful, program call this function
        /// </summary>
        /// <param name="lineErrors">Where mistakes was found</param>
        /// <param name="content">Mistakes</param>
        /// <param name="fileName">A file, which was checking</param>
        public ResultProcessingFile(List<uint> lineErrors, List<string> content, string fileName)
        {
            LineErrors = lineErrors;
            Content = content;
            FileName = fileName;
            IsErrorOccured = false;
        }
        
        /// <summary>
        /// This field show us, if during checking was error.
        /// </summary>
        public bool IsErrorOccured { get; }
        
        /// <summary>
        /// This field contains text of an error.
        /// </summary>
        public string TextError { get; }

        /// <summary>
        /// File, which was checked.
        /// </summary>
        public string FileName { get; }
        
        /// <summary>
        /// List of lines, where mistakes are.
        /// </summary>
        public List<uint> LineErrors { get; } 
        
        /// <summary>
        /// List of mistakes, which was made by user.
        /// </summary>
        public List<string> Content { get; }

        /// <summary>
        /// Override function, where creating detailed information.
        /// </summary>
        /// <returns>String with detailed information about error.</returns>
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