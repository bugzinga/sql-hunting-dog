
dim paramsList 
paramsList = split(session.property("CustomActionData"), ",") 
dim param1 
param1 = paramsList(0) 



Set objFSO = CreateObject("Scripting.FileSystemObject")

Const ssfPROFILE = &H1A
Set oShell = CreateObject("Shell.Application")
strAppFolder = oShell.NameSpace(ssfPROFILE).Self.Path


rem msgbox strAppFolder,1,"AppData folder:"

dim replacementPath
replacementPath = objFSO.BuildPath(param1, "SSMS2012\HuntingDog.dll") 

Const ForReading = 1
Const ForWriting = 2

Set WshShell = CreateObject("Wscript.Shell")
dim rootFolder
rootFolder = WshShell.ExpandEnvironmentStrings("%APPDATA%")

dim addinFilePath
addinFilePath = rootFolder & "\Microsoft\MSEnvShared\Addins\HuntingDog.AddIn"

msgbox  addinFilePath,1,"HutingDog.Addin folder:" 
rem dim strText
rem strText = "TARGET"
rem Set objFile = objFSO.OpenTextFile(addinFilePath, ForReading)
rem strText = objFile.ReadAll
rem objFile.Close

strText = "<Assembly>TARGET</Assembly>"
dim strNewText
strNewText = Replace(strText, "TARGET",replacementPath)

Set objFileWrite = objFSO.CreateTextFile(addinFilePath, True, True)

 objFileWrite.WriteLine "<?xml version=""1.0"" encoding=""UTF-16"" standalone=""no""?>"
 objFileWrite.WriteLine "<Extensibility xmlns=""http://schemas.microsoft.com/AutomationExtensibility"">"
 objFileWrite.WriteLine "<HostApplication><Name>Microsoft SQL Server Management Studio</Name><Version>*</Version></HostApplication>"
 objFileWrite.WriteLine "<Addin><FriendlyName>Hunting Dog</FriendlyName><Description>Hunting Dog</Description>"
 objFileWrite.WriteLine strNewText
 objFileWrite.WriteLine	"<FullClassName>HuntingDog.Connect</FullClassName><LoadBehavior>0</LoadBehavior><CommandPreload>1</CommandPreload><CommandLineSafe>0</CommandLineSafe></Addin></Extensibility>"



objFileWrite.Close


