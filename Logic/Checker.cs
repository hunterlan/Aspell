using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Models;

namespace Logic
{
    public class Checker
    {
        private List<Rule> Rules;
        private readonly string _pathToRules; 

        public Checker()
        {
            _pathToRules = "rules.json";
            LoadRules();
        }
        public List<InfoFile> CheckFiles(List<string> fileNames, List<string> ruleIgnore)
        {
            // TODO: Use Hunspell and ignore words which in rule ignore
            foreach (var fileName in fileNames)
            {
                string sourceCode = LoadFile(fileName);
                if (sourceCode == null)
                {
                    Console.WriteLine("File cannot be load");
                }
                else
                {
                    var parsedSourceCode = ParseSourceCode(sourceCode);
                    var words = parsedSourceCode.Split(' ');
                }
            }
            throw new NotImplementedException();
        }

        private async Task LoadRules()
        {
            await using var fs = new FileStream(_pathToRules, FileMode.Open);
            Rules = await JsonSerializer.DeserializeAsync<List<Rule>>(fs);
        }
        
        // TODO: Write reading of file
        private string LoadFile(string pathToFile)
        {
            var fileData = string.Empty;
            try
            {
                using (StreamReader sr = new StreamReader(pathToFile))
                {
                    fileData = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                return null;
            }

            return fileData;
        }
        
        // TODO: Write parsing special symbols from file
        private string ParseSourceCode(string sourceCode)
        {
            var temp = sourceCode;
            temp = temp.Replace("/*", " ").Replace('*', ' ').Replace("*/", " ");

            return temp;
        }
    }
}