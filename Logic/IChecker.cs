using System.Collections.Generic;
using Models;

namespace Logic
{
    /// <summary>
    /// Class, which responsible for checking text in files (comments in source codes; document; markdowns).
    /// </summary>
    public interface IChecker
    {
        /// <summary>
        /// Checking all provided paths and execute checking process on threads.
        /// </summary>
        /// <param name="fileNames">Paths, provided by user.</param>
        /// <param name="ruleIgnore">Paths to files, where can get rules, which have to be ignored.</param>
        /// <returns>List of checked files, where can get mistakes.</returns>
        List<ResultProcessingFile> CheckComments(List<string> fileNames, List<string> ruleIgnore);
    }
}