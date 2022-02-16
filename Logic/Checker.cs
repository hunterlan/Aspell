﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using Infrastructure;
using Models;
using Newtonsoft.Json;
using WeCantSpell.Hunspell;

namespace Logic
{
    public class Checker : IChecker
    {
        private List<Rule> _rules;
        private readonly string _pathToRules;
        private readonly char[] _delimiterChars = { ' ', ',', '.', ':', '\t', '\r', '\n', '-' };
        private readonly IUtils _utils;
        private const int LastDictionary = 2;

        public Checker()
        {
            _utils = UtilsFactory.GetUtilsObject();
            _pathToRules = "rules.json";
            LoadRules();
        }
        
        public List<ResultProcessingFile> CheckComments(List<string> fileNames, List<string> ruleIgnore)
        {
            List<ResultProcessingFile> infoResult = new(fileNames.Capacity);
            List<Task<ResultProcessingFile>> tasks = new(fileNames.Count);
            var dictionaries = LoadDictionaries();
            var pathToFiles = _utils.ReplaceDirectoriesByFiles(fileNames);
            
            foreach (var filePath in pathToFiles)
            {
                tasks.Add(Task<ResultProcessingFile>.Factory.StartNew(() => StartExtract(filePath, dictionaries)));
            }

            Task.WaitAll(tasks.ToArray());

            infoResult.AddRange(tasks.Select(task => task.Result));

            return infoResult;
        }

        private ResultProcessingFile StartExtract(string fileName, WordList[] dictionaries)
        {
            var fileExtension = Path.GetExtension(fileName);
                var sourceCode = string.Empty;

                if (!_utils.IsDocumentType(fileExtension))
                {
                    try
                    {
                        sourceCode = _utils.LoadFile(fileName);
                    }
                    catch (Exception e)
                    {
                        return new ResultProcessingFile(true, $"Can't load file. {e.Message}", fileName);
                    }
                }
                
                List<uint> lineErrors = new(10);
                List<string> mistakes = new(10);
                List<string> extractedText = new();

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
                        bool sharpCommentExtracted = false;
                        
                        if (fileExtension.Equals(".cs"))
                        {
                            if (_utils.IsXmlComments(sourceCode))
                            {
                                extractedText = ExtractXmlComments(sourceCode);
                                sharpCommentExtracted = true;
                            }
                        }

                        if (!sharpCommentExtracted)
                        {
                            var currentRule = GetRuleForFile(fileName);
                            if (currentRule == null)
                            {
                                return new ResultProcessingFile(true, 
                                    "File doesn't have supported file extension!", fileName);
                            }

                            extractedText = ExtractComments(sourceCode, currentRule);
                        }
                    } break;
                }

                foreach (var word in extractedText.Select(comment => comment.Split(_delimiterChars))
                    .SelectMany(words => words))
                {
                    if (string.IsNullOrWhiteSpace(word)) continue;
                    if (_utils.IsWordHexadecimal(word)) continue;
                    
                    for (var i = 0; i < dictionaries.Length; i++)
                    {
                        if (dictionaries[i].Check(word))
                        {
                            break;
                        }

                        if (i == LastDictionary)
                        {
                            if (_utils.IsDocumentType(fileExtension))
                            {
                                lineErrors.Add(GetCurrentLineForWord(word, extractedText));
                            }
                            else
                            {
                                lineErrors.Add(GetCurrentLineForWord(word, sourceCode));
                            }
                            mistakes.Add(word);
                        }
                    }
                }

                return new ResultProcessingFile(lineErrors, mistakes, fileName);
        }

        private void LoadRules()
        {
            var json = _utils.LoadFile(_pathToRules);
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

        private List<string> ExtractXmlComments(string sourceCode)
        {
            var tagsRegex = new Regex(@"<(.)*?>");
            List<string> extractedComments = new();
            string onlyComments = sourceCode;

            while (true)
            {
                int indexOpenComments = 0;
                int indexCloseComments = 0;
                
                indexOpenComments = onlyComments.IndexOf("///", StringComparison.Ordinal);

                if (indexOpenComments < 0)
                {
                    break;
                }
                
                indexCloseComments = indexOpenComments + onlyComments[indexOpenComments..].IndexOf('\n', StringComparison.OrdinalIgnoreCase);
                
                if (indexCloseComments < 0)
                {
                    break;
                }
                
                onlyComments = onlyComments.Remove(indexOpenComments, 3); // "///" has 3 length
                sourceCode = onlyComments;
                
                int length = indexCloseComments - indexOpenComments;
                var comment = onlyComments.Substring(indexOpenComments, length);
                var substringComment = tagsRegex.Replace( new string(comment.Where(c => !char.IsControl(c)).ToArray()), "");

                if (!string.IsNullOrWhiteSpace(substringComment))
                {
                    extractedComments.Add(substringComment);   
                }
                
                sourceCode = sourceCode.Remove(indexOpenComments, length);
            }
            
            return extractedComments;
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

            if (body != null)
            {
                result.AddRange(from paragraph in body where !string.IsNullOrWhiteSpace(paragraph.InnerText) select paragraph.InnerText);
            }

            return result;
        }

        private uint GetCurrentLineForWord(string word, List<string> lines)
        {
            uint lineError = 1;
            
            foreach (var line in lines)
            {
                if (line.Contains(word))
                {
                    return lineError;
                }

                lineError++;
            }

            return lineError;
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
    }
}