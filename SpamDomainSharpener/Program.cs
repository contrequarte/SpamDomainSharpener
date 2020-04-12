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
            emailList.AddRange(File.ReadAllLines(inFile)
                               .Where(e=>!(e.StartsWith(@"at") && e.EndsWith(@"@timestamped-at.koeln")))
                               .Select(e=> e.Contains("@")?e:string.Format("@{0}",e)));
            domainList.AddRange(emailList.Select(e => "@" + e.Split('@')[1])
                                         .GroupBy(d => d)
                                         .Select(g => g.Key + ";" + g.Count().ToString())
                                         .OrderBy(r => r));
            File.WriteAllLines(outFile, domainList);

            domainList.Clear();
            domainList.AddRange(emailList.Select(e => "@" + e.Split('@')[1]).GroupBy(d => d).Where(g => g.Count() > 2).Select(g => g.Key).OrderBy(r => r));          
            domainList.AddRange(emailList.Select(e => new { Domain = string.Format("@{0}", e.Split('@')[1]), Email = e }).Where(d => !domainList.Contains(d.Domain)).Select(a => a.Email));

            domainList.Insert(0, string.Format("at{0}@timestamped.koeln", DateTime.Now.ToString("yyyy_MM_dd_hh_mm")));
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
