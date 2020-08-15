from datetime import datetime

sourceFile = 'Q:\\Development\\GitHubHostedProjects\\SpamDomainSharpener\\PythonVersion\\data\\junk_mail.txt'

targetFile = 'Q:\\Development\\GitHubHostedProjects\\SpamDomainSharpener\\PythonVersion\\data\\junk_mail_sharpened{0}.txt'\
              .format(datetime.now().strftime("%Y%m%d"))

#domains to exclude from being added as spam domain
exclusionList = ['gmail.com', 'googlemail.com', 'hotmail.com', 'outlook.com', \
                 'yahoo.com', 'web.de', 'gmx.de', 'gmx.net', 'live.de']

domainDict = {}

newlyAddedDomains = []
class SpamAddress:
     
    def __init__(self, emailAddress):
        self.name \
        , dummy \
        , self.domain = emailAddress.lower().partition('@')

    def IsDomainOnly(self):
        return (self.name == '')
    
    def CompleteAddress(self):
        return ("{0}@{1}".format(self.name, self.domain))

def readFiletoList(sourceFile):
    myFile = open(sourceFile, encoding="utf-16")
    myList = myFile.readlines()
    myFile.close() # closing the file
    for index, data in enumerate(myList):
        myList[index] = SpamAddress(myList[index].rstrip("\r\n"))
    return myList

myData = readFiletoList(sourceFile)

for spamAddress in myData:
  
  if spamAddress.domain not in exclusionList:
      #to avoid blocking gmail.com etc...,
    if spamAddress.IsDomainOnly():
       #print(spamAddress.domain + ' is a blocked domain already!')
       domainDict[spamAddress.domain] = 100000 #signalling, this blocked domain has to stay
    else:
        if spamAddress.domain in domainDict:
            domainDict[spamAddress.domain] = domainDict[spamAddress.domain] + 1
        else:
            #if addressParts.domain != '@timestamped.koeln'
            domainDict[spamAddress.domain] = 1

# Removing domains having less than 3 entries
for key in list(domainDict):
    if domainDict[key] < 3:
        domainDict.pop(key)

# Writing new spam list file
# First: domains blocked entirely (and adding newly added domains in a separate list)
myOutFile = open(targetFile, "w")
#for key in list(domainDict):
for domainItem in sorted(domainDict.items(), key=lambda item: item[0]):
    myOutFile.write('@'+domainItem[0]+"\n")
    if domainItem[1] < 100000:
        newlyAddedDomains.append("Newly added domain: {d} num of occurences: {n}".format(d = domainItem[0], n = domainItem[1]))
# Second: complete email addresses for domains appearing fewer than 3 times
for spamAddress in sorted(myData, key=lambda x: x.domain, reverse=False):
    if spamAddress.domain not in domainDict:
        if spamAddress.domain != "timestamped.koeln":
            myOutFile.write(spamAddress.CompleteAddress()+"\n")
# Third: Adding a new "timestamp" entry to the spam list
myOutFile.write("at{0}@timestamped.koeln".format(datetime.now().strftime("%Y_%m_%d_%H_%M")))
myOutFile.close()

# Show newly added domains
for domain in sorted(newlyAddedDomains):
    print(domain)