

Dim data
data = Session.Property("CustomActionData")

Dim parameters
parameters = split(data, ",") 

Dim installFolder
installFolder = parameters(0) 

Set fileSystem = CreateObject("Scripting.FileSystemObject")

Private Sub BuildFullPath(FullPath)
    If Not fileSystem.FolderExists(FullPath) Then
        Call BuildFullPath(fso.GetParentFolderName(FullPath))
        fileSystem.CreateFolder(FullPath)
    End If
End Sub

Dim assemblyPath2012
assemblyPath2012 = fileSystem.BuildPath(installFolder, "SSMS2012\HuntingDog.dll") 

Dim assemblyPath2014
assemblyPath2014 = fileSystem.BuildPath(installFolder, "SSMS2014\HuntingDog.dll") 

Set shell = CreateObject("Wscript.Shell")

Dim appDataFolder
appDataFolder = shell.ExpandEnvironmentStrings("%ProgramData%")

dim addinConfigFolder2012
addinConfigFolder2012 = appDataFolder & "\Microsoft\SQL Server Management Studio\11.0\Addins"
Call BuildFullPath(addinConfigFolder2012)

Dim addinConfigPath2012
addinConfigPath2012 = addinConfigFolder2012 & "\HuntingDog.AddIn"


dim addinConfigFolder2014
addinConfigFolder2014 = appDataFolder & "\Microsoft\SQL Server Management Studio\12.0\Addins"
Call BuildFullPath(addinConfigFolder2014)

Dim addinConfigPath2014
addinConfigPath2014 = addinConfigFolder2014 & "\HuntingDog.AddIn"


Dim oldAppData
oldAppData = shell.ExpandEnvironmentStrings("%APPDATA%")
dim previousAddinFile
previousAddinFile = oldAppData & "\Microsoft\MSEnvShared\Addins\HuntingDog.AddIn"
if fileSystem.FileExists(previousAddinFile) then
    fileSystem.DeleteFile(previousAddinFile)
end if

Class XmlConfig

	Private xml
	
	Private Sub Class_Initialize
		Set xml = CreateObject("Microsoft.XMLDOM")
	End Sub
	
	Private Function AddNode(root, childName)
		Set child = xml.CreateElement(childName)
		root.AppendChild(child)
		Set AddNode = child
	End Function
	
	Private Sub AddTextNode(parent, childName, childValue)
		Set child = AddNode(parent, childName)
		child.Text = childValue
	End Sub
	
	Public Sub Save(destination, assemblyPath)

		Set root = AddNode(xml, "Extensibility")
		Call root.SetAttribute("xmlns", "http://schemas.microsoft.com/AutomationExtensibility")

		Set app = AddNode(root, "HostApplication")
		Call AddTextNode(app, "Name", "Microsoft SQL Server Management Studio")
		Call AddTextNode(app, "Version", "*")
		
		Set addin = AddNode(root, "Addin")
		Call AddTextNode(addin, "FriendlyName", "Hunting Dog")
		Call AddTextNode(addin, "Description", "Hunting Dog")
		Call AddTextNode(addin, "Assembly", assemblyPath)
		Call AddTextNode(addin, "FullClassName", "HuntingDog.Connect")
		Call AddTextNode(addin, "LoadBehavior", "1")
		Call AddTextNode(addin, "CommandPreload", "0")
		Call AddTextNode(addin, "CommandLineSafe", "0")

		Set stream = CreateObject("ADODB.Stream")
		With stream
			.Open
			.Type = 1
			Set writer = CreateObject("MSXML2.MXXMLWriter")
			With writer
				.OmitXMLDeclaration = False
				.Standalone = False
				.ByteOrderMark = True
				.Encoding = "UTF-16"
				.Indent = True
				.Output = stream
				Set reader = CreateObject("MSXML2.SAXXMLReader")
				With reader
					.ContentHandler = writer
					.DTDHandler = writer
					.errorHandler = writer
					.Parse(xml)
				End With
			End With
			.SaveToFile destination, 2
			.Close
		End With
	
	End Sub

End Class

Set config2012 = New XmlConfig
Call config2012.Save(addinConfigPath2012, assemblyPath2012)

Set config2014 = New XmlConfig
Call config2014.Save(addinConfigPath2014, assemblyPath2014)