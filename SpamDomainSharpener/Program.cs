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
            string inFile = @"junk_mail.txt";
            string outFile = @"junk_domain.txt";
            string outFileFiltered = @"junk_domain_Greater2.txt";
            List<string> emailList = new List<string>();
            List<string> domainList = new List<string>();
            List<string> finalList = new List<string>();
            emailList.AddRange(File.ReadAllLines(inFile));
            domainList.AddRange(emailList.Select(e => "@" + e.Split('@')[1]).GroupBy(d => d).Select(g => g.Key + ";" + g.Count().ToString()).OrderBy(r=>r));
            File.WriteAllLines(outFile, domainList);
            domainList.Clear();
            domainList.AddRange(emailList.Select(e => "@" + e.Split('@')[1]).GroupBy(d => d).Where(g=>g.Count()>2).Select(g => g.Key).OrderBy(r=>r));           
            File.WriteAllLines(outFileFiltered, domainList);
        }
    }
}
