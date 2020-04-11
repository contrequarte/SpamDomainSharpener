using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace SpamDomainSharpener
{
    class Program
    {

        static void Main(string[] args)
        {
            var parameterCheckResult = ExtractParameters(args);
            if (!parameterCheckResult.IsValid)
            {
                Console.WriteLine("SpamDomainSharpener inFileLocation [outFileLocation]");
                return;
            }
            string inFile = parameterCheckResult.InFile;
            string outFile = @"junk_domain.txt";
            string outFileFiltered =parameterCheckResult.OutFile;
            List<string> emailList = new List<string>();
            List<string> domainList = new List<string>();
            List<string> finalList = new List<string>();
            emailList.AddRange(File.ReadAllLines(inFile));
            domainList.AddRange(emailList.Select(e => "@" + e.Split('@')[1]).GroupBy(d => d).Select(g => g.Key + ";" + g.Count().ToString()).OrderBy(r => r));
            File.WriteAllLines(outFile, domainList);
            domainList.Clear();
            domainList.AddRange(emailList.Select(e => "@" + e.Split('@')[1]).GroupBy(d => d).Where(g => g.Count() > 2).Select(g => g.Key).OrderBy(r => r));
            File.WriteAllLines(outFileFiltered, domainList);
        }

        private static (bool IsValid, string InFile, string OutFile) ExtractParameters(string[] args)
        {
            string inFile = null;
            string outFile = null;
            bool valid = true;
            if (args.Length >= 1)
                if (File.Exists(args[0]))
                {
                    inFile = args[0];
                    outFile = inFile + ".out";
                    if (args.Length > 1)
                         if (Directory.Exists(Path.GetDirectoryName(args[1])))
                            if (!string.IsNullOrEmpty(Path.GetFileName(args[1])))
                                outFile = args[1];
                            else
                                outFile = Path.Combine(args[1], Path.GetFileName(args[0]) + ".out");
                        else
                            valid = false;
                    else
                        outFile = inFile + ".out";
                }
                else
                    valid = false;
            else
                valid = false;
            return (IsValid: valid, InFile: inFile, OutFile: outFile);
        } 
    }
}
