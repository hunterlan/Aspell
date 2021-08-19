using System;
using System.Collections.Generic;
using System.IO;
using Models;
using Newtonsoft.Json;
using WeCantSpell.Hunspell;

namespace Logic
{
    //TODO: Multithreading
    public class Checker
    {
        [JsonProperty("rule")]
        private Rule Rules;
        private readonly string _pathToRules; 

        public Checker()
        {
            _pathToRules = "rules.json";
            LoadRules();
        }
        
        //TODO: Several dictionaries
        public List<InfoFile> CheckFiles(List<string> fileNames, List<string> ruleIgnore)
        {
            List<InfoFile> infoErrors = new(fileNames.Capacity);
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
                    List<uint> lineErrors = new(10);
                    List<string> mistakes = new(10);
                    var dictionary = WordList.CreateFromFiles(@"ru_RU.dic");
                    
                    var extractedComments = ExtractComments(sourceCode);
                    foreach (var comment in extractedComments)
                    {
                        var words = comment.Split(' ');

                       
                        foreach (var word in words)
                        {
                            if (!dictionary.Check(word))
                            {
                                lineErrors.Add(GetCurrentLineForWord(word, sourceCode));
                                mistakes.Add(word);
                            }
                        }
                    }
                    
                    var info = new InfoFile(lineErrors, mistakes, fileName);
                    infoErrors.Add(info);
                }
            }

            return infoErrors;
        }

        //TODO: Several rules
        private void LoadRules()
        {
            var json = LoadFile(_pathToRules);
            Rules = JsonConvert.DeserializeObject<Root>(json)?.Rule;
        }
        
        // TODO: Read file parts
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
        
        // TODO: Check for type file
        private List<string> ExtractComments(string sourceCode)
        {
            List<string> comments = new();
            foreach (var typeComment in Rules.Comments)
            {
                while (true)
                {
                    var temp = sourceCode;
                    int indexOpenComments = 0;
                    int indexCloseComments = 0;
                    var stringDividedComment = typeComment.Split("br");
                    
                    if (stringDividedComment.Length > 1)
                    {
                        indexOpenComments = temp.IndexOf(stringDividedComment[0], StringComparison.Ordinal);
                        indexCloseComments = temp.IndexOf(stringDividedComment[1], StringComparison.Ordinal) 
                                             - stringDividedComment[0].Length;
                    }
                    else
                    {
                        indexOpenComments = temp.IndexOf(typeComment, StringComparison.Ordinal);
                        indexCloseComments = temp.IndexOf("\\n", StringComparison.Ordinal);
                    }
                    
                    if (indexOpenComments <= 0 || indexCloseComments <= 0)
                    {
                        break;
                    }

                    if (stringDividedComment.Length > 1 == false)
                    {
                        temp = temp.Remove(indexOpenComments, typeComment.Length);
                        sourceCode = temp;
                    }
                    else
                    {
                        temp = temp.Remove(indexOpenComments, stringDividedComment[0].Length);
                        temp = temp.Remove(indexCloseComments, stringDividedComment[1].Length);
                        sourceCode = temp;
                    }

                    int length = indexCloseComments - indexOpenComments;
                    comments.Add(temp.Substring(indexOpenComments, length));
                    sourceCode = sourceCode.Remove(indexOpenComments, length);  
                }
            }
            return comments; 
        }

        private uint GetCurrentLineForWord(string word, string sourceCode)
        {
            var splitCode = sourceCode.Split('\n');
            
            uint line = 1;
            foreach (var stringCode in splitCode)
            {
                if (!stringCode.Contains(word))
                {
                    line++;
                }
                else
                {
                    break;
                }
            }

            return line;
        }
    }
}