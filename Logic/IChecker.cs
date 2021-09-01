using System.Collections.Generic;
using Models;

namespace Logic
{
    public interface IChecker
    {
        List<InfoFile> CheckFiles(List<string> fileNames, List<string> ruleIgnore);
    }
}