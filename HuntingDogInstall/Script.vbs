
dim paramsList 
paramsList = split(session.property("CustomActionData"), ",") 
dim param1 
param1 = paramsList(0) 


Const ForReading = 1
Const ForWriting = 2

Set objFSO = CreateObject("Scripting.FileSystemObject")
Set objFile = objFSO.OpenTextFile("C:\test.txt", ForReading)

strText = objFile.ReadAll
objFile.Close

set replacementPath = param1 & "SSMS2012\HuntingDog.dll"
strNewText = Replace(strText, "TARGET",replacementPath)

Set objFile = objFSO.OpenTextFile("C:\test.txt", ForWriting)
objFile.WriteLine strNewText
objFile.Close



