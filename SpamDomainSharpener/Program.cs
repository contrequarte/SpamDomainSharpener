using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace SpamDomainSharpener
{
    class Program
    {
        private const int thresholdValueToAddDomainInsteadOfEmail = 2;
        private const string protectedDomainList = "@gmail.com;@googlemail.com;@gmx.de;@web.de;@yahoo.com";

        static void Main(string[] args)
        {
            var parameterCheckResult = ExtractParameters(args);
            if (!parameterCheckResult.IsValid)
            {
                Console.WriteLine("SpamDomainSharpener inFileLocation [outFileLocation]");
                return;
            }
            string inFile = parameterCheckResult.InFile;

            string outFileFiltered =parameterCheckResult.OutFile;
            List<string> emailList = new List<string>();
            List<string> domainList = new List<string>();
            List<string> finalList = new List<string>();
            emailList.AddRange(File.ReadAllLines(inFile)
                               .Where(e=>!(e.StartsWith(@"at") && e.EndsWith(@"@timestamped.koeln")))
                               .Select(e=> e.Contains("@")?e:string.Format("@{0}",e)));

            domainList.Clear();
            
            domainList.AddRange(emailList.Where(e => e.StartsWith('@'))     // add domains already existing in the blocked list
                                         .Union(emailList.Select(e => "@" + e.Split('@')[1]) 
                                                          .GroupBy(d => d).Where(g => g.Count() > thresholdValueToAddDomainInsteadOfEmail)
                                                          .Where(d => !domainList.Contains(d.Key))
                                                          .Select(g => g.Key) // add new domains when number of emails of this domain greater threshold value
                                                )
                                         .OrderBy(d => d)
                                );
            //remove protected domains from the list, to avoid blocking of all gmail.com accounts etc.
            domainList = RemoveProtectedDomains(domainList, protectedDomainList.Split(';'));
            //now add full email addresses sent from domains still not reaching threshold value.
            domainList.AddRange(emailList.Select(e => new { Domain = string.Format("@{0}", e.Split('@')[1]), Email = e })
                            .Where(d => !domainList.Contains(d.Domain))
                            .OrderBy(e => e.Domain)
                            .Select(a => a.Email));
            //finally add a fake timestamp email address
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

        private static List<string> RemoveProtectedDomains(List<string> domainList, IEnumerable<string> domainsToProtect)
        {
            return domainList.Where(d => !(domainsToProtect.Contains(d))).Select(d => d).ToList();
        }
    }
}
