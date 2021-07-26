using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using Logic;
using Models;

namespace AspellCLI
{
    class Program
    {
        private static List<string> filesToCheck = new();
        private static bool isHtmlMode = false;
        private static List<string> rulesForIgnore = new();
        private static Checker _checker = new(); 
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(RunOptions);
        }
        
        static void RunOptions(CommandLineOptions opts)
        {
            filesToCheck = opts.Files;
            if (opts.RulesForIgnore.Any()) rulesForIgnore = opts.RulesForIgnore;
            var result = _checker.CheckFiles(filesToCheck, rulesForIgnore);
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
        public List<string> Files { get; set; }
        
        [Option("isHtml", Required = false, HelpText = "Is output result will be in HTML. If not, will be displayed in CLI. Default - false")]
        public bool IsHtml { get; set; } 
        
        [Option("rules-for-ignore", Required = false, HelpText = "Input file, from which will take words to ignore")]
        public List<string> RulesForIgnore { get; set; }
    }
}