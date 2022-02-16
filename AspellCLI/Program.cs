using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using Logic;
using Models;

namespace AspellCLI
{
    class Program
    {
        private static List<string> _filesToCheck = new();
        private static bool _isHtmlMode;
        private static List<string> _rulesForIgnore = new();
        private static readonly IChecker Checker;

        /// <summary>
        /// Constructor
        /// </summary>
        static Program()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Checker = CheckerFactory.GetCheckerObject();
        }

        /// <summary>
        /// Main function, which start the program.
        /// </summary>
        /// <param name="args">Arguments, which user provide to the program</param>
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(RunOptions);
        }

        /// <summary>
        /// This function get arguments and then, call checking method.
        /// </summary>
        /// <param name="opts">Options provided by user and parsed by library</param>
        private static void RunOptions(CommandLineOptions opts)
        {
            _filesToCheck = opts.Files.ToList();
            _isHtmlMode = opts.IsHtml;
            if (opts.RulesForIgnore != null && opts.RulesForIgnore.Any())
            {
                _rulesForIgnore = opts.RulesForIgnore.ToList();
            }
            
            var result = Checker.CheckComments(_filesToCheck, _rulesForIgnore);
            ShowResult(result);
        }

        /// <summary>
        /// This function show result of the checking.
        /// </summary>
        /// <param name="resultInfo">Results of checking files</param>
        /// <exception cref="NotImplementedException">Unfortunately, HTML mode isn't implemented.</exception>
        private static void ShowResult(List<ResultProcessingFile> resultInfo)
        {
            if (!_isHtmlMode)
            {
                foreach (var info in resultInfo)
                {
                    Console.WriteLine(info.IsErrorOccured ? $"{info.FileName}: {info.TextError}" : 
                        (info.Content.Count != 0 ? info.ToString() : $"There aren't any mistakes in file {info.FileName}"));
                }   
            }
            else
            {
                throw new NotImplementedException("HTML mode isn't implemented yet.");
            }
        }
    }

    /// <summary>
    /// This class contains option, which user will user, when start the program.
    /// </summary>
    class CommandLineOptions
    {
        /// <summary>
        /// List of files, which have to be checked.
        /// </summary>
        [Option('f', "files", Separator = ';', Required = true, HelpText = "Input files, or directories, which should be checked. Can be put several by separator \';\'")]
        public IEnumerable<string> Files { get; set; }
        
        /// <summary>
        /// This defined, will user see results in the terminal, or get HTML file.
        /// </summary>
        [Option("isHtml", Required = false, Default = false, HelpText = "Is output result will be in HTML. If not, will be displayed in CLI. Default - false")]
        public bool IsHtml { get; set; } 
        
        /// <summary>
        /// File, where some rules will be ignored.
        /// </summary>
        [Option('i', "rules-for-ignore", Required = false, Separator = ';', HelpText = "Input files, from which will take words to ignore. Can be put several by separator \';\'")]
        public IEnumerable<string> RulesForIgnore { get; set; }
    }
}