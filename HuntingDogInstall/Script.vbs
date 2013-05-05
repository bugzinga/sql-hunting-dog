
Dim data
data = Session.Property("CustomActionData")

Dim parameters
parameters = split(data, ",") 

Dim installFolder
installFolder = parameters(0) 

Set fileSystem = CreateObject("Scripting.FileSystemObject")

Dim assemblyPath
assemblyPath = fileSystem.BuildPath(installFolder, "SSMS2012\HuntingDog.dll") 

Set shell = CreateObject("Wscript.Shell")

Dim appDataFolder
appDataFolder = shell.ExpandEnvironmentStrings("%APPDATA%")

Dim microsoftFolder
microsoftFolder = appDataFolder & "\Microsoft"

If Not fileSystem.FolderExists(microsoftFolder) Then
	fileSystem.CreateFolder(microsoftFolder)
End If

Dim envSharedFolder
envSharedFolder = microsoftFolder & "\MSEnvShared"

If Not fileSystem.FolderExists(envSharedFolder) Then
	fileSystem.CreateFolder(envSharedFolder)
End If

Dim addinConfigFolder
addinConfigFolder = envSharedFolder & "\Addins"

If Not fileSystem.FolderExists(addinConfigFolder) Then
	fileSystem.CreateFolder(addinConfigFolder)
End If

Dim addinConfigPath
addinConfigPath = addinConfigFolder & "\HuntingDog.AddIn"

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

Set config = New XmlConfig
Call config.Save(addinConfigPath, assemblyPath)
