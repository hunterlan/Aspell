using System.Collections.Generic;
using Models;

namespace Logic
{
    public interface IChecker
    {
        List<ResultProcessingFile> CheckComments(List<string> fileNames, List<string> ruleIgnore);
    }
}