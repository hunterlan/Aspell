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
        private static bool _isHtmlMode = false;
        private static List<string> _rulesForIgnore = new();
        private static readonly IChecker _checker = new Checker(); 
        
        static Program() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(RunOptions);
        }
        
        static void RunOptions(CommandLineOptions opts)
        {
            _filesToCheck = opts.Files.ToList();
            if (!string.IsNullOrWhiteSpace(opts.RulesForIgnore)) _rulesForIgnore.Add(opts.RulesForIgnore);
            var result = _checker.CheckFiles(_filesToCheck, _rulesForIgnore);
            ShowResult(result);
        }

        static void ShowResult(List<InfoFile> resultInfo)
        {
            foreach (var info in resultInfo)
            {
                Console.WriteLine(info.ToString());
            }
        }
    }

    class CommandLineOptions
    {
        [Option('f', "files", Required = true, HelpText = "Input files, which should be checked")]
        public IEnumerable<string> Files { get; set; }
        
        [Option("isHtml", Required = false, HelpText = "Is output result will be in HTML. If not, will be displayed in CLI. Default - false")]
        public bool IsHtml { get; set; } 
        
        [Option("rules-for-ignore", Required = false, HelpText = "Input file, from which will take words to ignore", Default = "")]
        public string RulesForIgnore { get; set; }
    }
}