using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using Models;
using Newtonsoft.Json;
using WeCantSpell.Hunspell;

namespace Logic
{
    //TODO: Multithreading
    public class Checker : IChecker
    {
        private List<Rule> _rules;
        private readonly string _pathToRules;
        private readonly char[] _delimiterChars = { ' ', ',', '.', ':', '\t', '\r', '\n', '-' };
        private const int LastDictionary = 2;

        public Checker()
        {
            _pathToRules = "rules.json";
            LoadRules();
        }
        
        //TODO: Use threads
        public List<ResultProcessingFile> CheckFiles(List<string> fileNames, List<string> ruleIgnore)
        {
            List<ResultProcessingFile> infoResult = new(fileNames.Capacity);
            var dictionaries = LoadDictionaries();
            // TODO: Use Hunspell and ignore words which in rule ignore
            foreach (var fileName in fileNames)
            {
                var fileExtension = Path.GetExtension(fileName);
                var sourceCode = string.Empty;

                if (!IsDocumentType(fileExtension))
                {
                    try
                    {
                        sourceCode = LoadFile(fileName);
                    }
                    catch (Exception e)
                    {
                        var result = new ResultProcessingFile(true, $"Can't load file. {e.Message}", fileName);
                        infoResult.Add(result);
                        continue;
                    }
                }
                
                List<uint> lineErrors = new(10);
                List<string> mistakes = new(10);
                List<string> extractedText;

                switch (fileExtension)
                {
                    case ".adoc":
                    case ".md":
                    {
                        extractedText = ExtractTextFromMarkup(sourceCode, fileName);
                    } break;
                    case ".doc":
                    case ".docx":
                    case ".odt":
                    {
                        extractedText = ExtractTextFromDoc(fileName);
                    } break;
                    default:
                    {
                        var currentRule = GetRuleForFile(fileName);
                        if (currentRule == null)
                        {
                            var result = new ResultProcessingFile(true, 
                                "File doesn't have supported file extension!", fileName);
                            infoResult.Add(result);
                            continue;
                        }

                        extractedText = ExtractComments(sourceCode, currentRule);
                    } break;
                }

                foreach (var word in extractedText.Select(comment => comment.Split(_delimiterChars))
                    .SelectMany(words => words))
                {
                    if (string.IsNullOrWhiteSpace(word)) continue;
                    if (IsWordHexademical(word)) continue;
                    
                    for (var i = 0; i < dictionaries.Length; i++)
                    {
                        if (dictionaries[i].Check(word))
                        {
                            break;
                        }

                        if (i == LastDictionary)
                        {
                            if (IsDocumentType(fileExtension))
                            {
                                lineErrors.Add(GetCurrentLineForWord(word, extractedText[0]));
                            }
                            else
                            {
                                lineErrors.Add(GetCurrentLineForWord(word, sourceCode));
                            }
                            mistakes.Add(word);
                        }
                    }
                }

                var info = new ResultProcessingFile(lineErrors, mistakes, fileName);
                infoResult.Add(info);
            }

            return infoResult;
        }

        private void LoadRules()
        {
            var json = LoadFile(_pathToRules);
            _rules = JsonConvert.DeserializeObject<Root>(json)?.Rule;
        }

        private Rule GetRuleForFile(string fileName)
        {
            Rule foundRule = null;

            var fileExtension = Path.GetExtension(fileName);
            if (fileExtension != null)
            {
                foreach (var rule in _rules.Where(rule => rule.TypeFile.Any(type => type == fileExtension)))
                {
                    foundRule = rule;
                    break;
                }
            }

            return foundRule;
        }

        // TODO: Read file parts
        private string LoadFile(string pathToFile)
        {
            using var sr = new StreamReader(pathToFile); 
            var fileData = sr.ReadToEnd();
            
            return fileData;
        }

        private WordList[] LoadDictionaries()
        {
            WordList[] dictionaries = new WordList[3];

            dictionaries[0] = WordList.CreateFromFiles(@"ru_RU.dic");
            dictionaries[1] = WordList.CreateFromFiles(@"uk_UA.dic");
            dictionaries[2] = WordList.CreateFromFiles(@"en_GB.dic");

            return dictionaries;
        }

        private List<string> ExtractComments(string sourceCode, Rule currentRule)
        {
            List<string> comments = new();
            foreach (var typeComment in currentRule.Comments)
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

                    if (indexOpenComments < 0 || indexCloseComments < 0)
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

        private List<string> ExtractTextFromMarkup(string text, string fileName)
        {
            var fileExtension = Path.GetExtension(fileName);
            if (fileExtension == ".md")
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private List<string> ExtractTextFromDoc(string fileName)
        {
            List<string> result = new();
            using var wordDocument = WordprocessingDocument.Open(fileName, false);
            // Assign a reference to the existing document body.  
            var body = wordDocument.MainDocumentPart?.Document.Body;
            //text of Docx file 
            if (body != null) result.Add(body.InnerText);

            return result;
        }

        private uint GetCurrentLineForWord(string word, string source)
        {
            var splitCode = source.Split('\n');

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

        private bool IsDocumentType(string fileExtension)
        {
            return fileExtension.Contains(".doc") || fileExtension == ".odt";
        }

        private bool IsWordHexademical(string word)
        {
             const string hexademicalStrRegex = @"0x.{1,}";
             Regex regex = new(hexademicalStrRegex);

             return regex.IsMatch(word);
        }
    }
}