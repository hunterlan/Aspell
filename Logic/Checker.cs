using System;
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
    /// <inheritdoc/>
    public class Checker : IChecker
    {
        /// <summary>
        /// Field, which contains information about rules.
        /// </summary>
        private List<Rule> _rules;

        private List<string> _wordsException;
        /// <summary>
        /// Path to rules initialized in the constructor.
        /// </summary>
        private readonly string _pathToRules;
        /// <summary>
        /// Characters, which will be removed from text during a check.
        /// </summary>
        private readonly char[] _delimiterChars = { ' ', ',', '.', ':', '\t', '\r', '\n', '-' };
        /// <summary>
        /// Fill, which call useful functions from class <see cref="Utils"/>
        /// </summary>
        private readonly IUtils _utils;
        /// <summary>
        /// Constant, which contains index of last dictionary.
        /// </summary>
        private const int LastDictionary = 2;

        /// <summary>
        /// Constructor, which initialize <see cref="_utils"/>, <see cref="_pathToRules"/> and load rules
        /// to <see cref="_rules"/>
        /// </summary>
        public Checker()
        {
            _wordsException = new List<string>();
            _utils = UtilsFactory.GetUtilsObject();
            _pathToRules = "Resources/rules.json";
            LoadRules();
        }
        
        /// <inheritdoc/>
        public List<ResultProcessingFile> CheckComments(List<string> fileNames, List<string> filesToIgnoreWords)
        {
            List<ResultProcessingFile> infoResult = new(fileNames.Capacity);
            List<Task<ResultProcessingFile>> tasks = new(fileNames.Count);
            var dictionaries = LoadDictionaries();
            var pathToFiles = _utils.ReplaceDirectoriesByFiles(fileNames);

            foreach (var filePath in filesToIgnoreWords)
            {
                LoadWordsException(filePath);
            }
            
            foreach (var filePath in pathToFiles)
            {
                tasks.Add(Task<ResultProcessingFile>.Factory.StartNew(() => StartExtract(filePath, dictionaries)));
            }

            Task.WaitAll(tasks.ToArray());

            infoResult.AddRange(tasks.Select(task => task.Result));

            return infoResult;
        }

        /// <summary>
        /// Function, which check a file for mistake.
        /// Steps:
        /// <para> 1. Determine extension of the file.</para>
        /// 2. Extract text/comments.
        /// <para> 3. Check by Hunspell.</para>
        /// 4. Return by creating new <see cref="ResultProcessingFile"/>
        /// </summary>
        /// <param name="fileName">Path to file.</param>
        /// <param name="dictionaries">Loaded dictionaries for checking.</param>
        /// <returns>Result of checking</returns>
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
                    if (_wordsException.Any(exceptionWord => word.ToLower().Contains(exceptionWord.ToLower()))) continue;

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

        /// <summary>
        /// Class, which load rules from rules.json and deserialize it to <see cref="_rules"/>
        /// </summary>
        private void LoadRules()
        {
            var json = _utils.LoadFile(_pathToRules);
            _rules = JsonConvert.DeserializeObject<Root>(json)?.Rule;
        }

        /// <summary>
        /// Method load words from files, which we doesn't have to check.
        /// </summary>
        /// <param name="fileName">Path to file with words exception.</param>
        private void LoadWordsException(string fileName)
        {
            var contentFile = _utils.LoadFile(fileName);
            var words = contentFile.Split(_delimiterChars);

            foreach (var word in words)
            {
                if (!string.IsNullOrWhiteSpace(word)) _wordsException.Add(word);
            }
        }

        /// <summary>
        /// Get rule(s) for current file.
        /// </summary>
        /// <param name="fileName">Path to file.</param>
        /// <returns>Rule for current file.</returns>
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

        /// <summary>
        /// Hunspell loads dictionaries to program.
        /// </summary>
        /// <returns>Loaded dictionaries.</returns>
        private WordList[] LoadDictionaries()
        {
            WordList[] dictionaries = new WordList[3];

            dictionaries[0] = WordList.CreateFromFiles(@"Resources/ru_RU.dic");
            dictionaries[1] = WordList.CreateFromFiles(@"Resources/uk_UA.dic");
            dictionaries[2] = WordList.CreateFromFiles(@"Resources/en_GB.dic");

            return dictionaries;
        }

        /// <summary>
        /// Extract comments from source code.
        /// </summary>
        /// <param name="sourceCode">Source code</param>
        /// <param name="currentRule">Rule, which was selected for current file, where sourceCode from.</param>
        /// <returns>Extracted text from comments.</returns>
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

        /// <summary>
        /// Extract comments from .NET source code.
        /// </summary>
        /// <param name="sourceCode">Source code of .NET</param>
        /// <returns>Extracted text from comments.</returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
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

        /// <summary>
        /// Extract content from document.
        /// </summary>
        /// <param name="fileName">Path to a document.</param>
        /// <returns>Extracted text from document.</returns>
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

        /// <summary>
        /// If need to find out line of mistake for document, program call this implementation.
        /// </summary>
        /// <param name="word">Word, where program detected an error</param>
        /// <param name="sentences">Sentences, which word can be from</param>
        /// <returns>Line, where mistake was found</returns>
        private uint GetCurrentLineForWord(string word, List<string> sentences)
        {
            uint lineError = 1;
            
            foreach (var line in sentences)
            {
                if (line.Contains(word))
                {
                    return lineError;
                }

                lineError++;
            }

            return lineError;
        }

        /// <summary>
        /// If need to find out line of mistake for source code, program call this implementation.
        /// </summary>
        /// <param name="word">Word, where program detected an error</param>
        /// <param name="source">Comment from source code, where mistake was found</param>
        /// <returns>Line, where mistake was found</returns>
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