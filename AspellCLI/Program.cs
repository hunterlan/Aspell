using System;
using System.Collections.Generic;
using CommandLine;

namespace AspellCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(RunOptions);
        }
        
        static void RunOptions(CommandLineOptions opts)
        {
            var gotFiles = opts.Files;
        }
    }

    class CommandLineOptions
    {
        [Option('f', "files", Required = true, HelpText = "Input files, which should be checked")]
        public List<string> Files { get; set; }
    }
}