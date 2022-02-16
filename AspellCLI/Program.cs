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

        static Program()
        {
            Checker = CheckerFactory.GetCheckerObject();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(RunOptions);
        }

        private static void RunOptions(CommandLineOptions opts)
        {
            _filesToCheck = opts.Files.ToList();
            _isHtmlMode = opts.IsHtml;
            if (!string.IsNullOrWhiteSpace(opts.RulesForIgnore)) _rulesForIgnore.Add(opts.RulesForIgnore);
            
            var result = Checker.CheckComments(_filesToCheck, _rulesForIgnore);
            ShowResult(result);
        }

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
                throw new NotImplementedException();
                throw new NotImplementedException("HTML mode isn't implemented yet.");
            }
        }
    }

    class CommandLineOptions
    {
        [Option('f', "files", Separator = ';', Required = true, HelpText = "Input files, which should be checked")]
        public IEnumerable<string> Files { get; set; }
        
        [Option("isHtml", Required = false, Default = false, HelpText = "Is output result will be in HTML. If not, will be displayed in CLI. Default - false")]
        public bool IsHtml { get; set; } 
        
        [Option("rules-for-ignore", Required = false, HelpText = "Input file, from which will take words to ignore", Default = "")]
        public string RulesForIgnore { get; set; }
    }
}