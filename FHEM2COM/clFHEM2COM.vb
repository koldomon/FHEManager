Imports Microsoft.Win32
Imports System.Runtime.InteropServices
Imports System.Text.RegularExpressions
Imports MM.FHEMManager.FHEMObjects
Imports MM.FHEMManager.FHEMTemplates

<Assembly: CLSCompliant(True)>
<ComVisible(True)> Public Class FHEMCOM
    Implements IDisposable

    '#Region "COM GUIDs"
    '    ' These  GUIDs provide the COM identity for this class 
    '    ' and its COM interfaces. If you change them, existing 
    '    ' clients will no longer be able to access the class.
    '    Public Const ClassId As String = "7179f600-d4d7-41ad-81fe-19d516a334fe"
    '    Public Const InterfaceId As String = "b8b47c61-05e5-4b66-bbd4-7569ae303c7b"
    '    Public Const EventsId As String = "bf9fc8ff-10a2-4dff-a2e2-6fe590bb4d98"
    '#End Region

    ' A creatable COM class must have a Public Sub New() 
    ' with no parameters, otherwise, the class will not be 
    ' registered in the COM registry and cannot be created 
    ' via CreateObject.

    Private Shared glbKernel As New FHEMKernel
    Public Sub New()
        MyBase.New()
    End Sub
    Public Sub New(thisConfigFilename As String, thisTemplateFile As String)
        MyBase.New()
        If String.IsNullOrEmpty(thisConfigFilename) Then
            glbKernel.LoadCfg()
        Else
            glbKernel.LoadCfg(thisConfigFilename)
        End If

        If String.IsNullOrEmpty(thisTemplateFile) Then
            glbKernel.LoadTemplates()
        Else
            glbKernel.LoadTemplates(thisTemplateFile)
        End If

    End Sub


    <ComVisible(True)> Public Function LoadCfg() As Boolean
        Return glbKernel.LoadCfg()
    End Function
    <ComVisible(True)> Public Function LoadCfg(thisConfigFilename As String) As Boolean
        If String.IsNullOrEmpty(thisConfigFilename) Then Return False
        Return glbKernel.LoadCfg(thisConfigFilename)
    End Function
    <ComVisible(True)> Public Function LoadTemplate() As Boolean
        Return glbKernel.LoadTemplates()
    End Function
    <ComVisible(True)> Public Function LoadTemplates(thisTemplateFilename As String) As Boolean
        If String.IsNullOrEmpty(thisTemplateFilename) Then Return False
        Return glbKernel.LoadTemplates(thisTemplateFilename)
    End Function
    <ComVisible(True)> Public Function GetSerializationEvents() As String()
        Return glbKernel.SerializationErrors.ToArray
    End Function

    <ComVisible(True)> Public Function ObjectCount() As Integer
        Return glbKernel.GetObjectsCount
    End Function
End Class

#Region "ObjectOperations"
Partial Class FHEMCOM
    <ComVisible(True)> Public Function GetTypes() As String()
        Dim myReturn As New List(Of String)
        myReturn.AddRange(glbKernel.CurrentObjects.Select(Of String)(Function(x) x.FHEMType).Distinct)
        Return myReturn.OrderBy(Function(x) x).ToArray
    End Function
    <ComVisible(True)> Public Function GetTypeAttributes(thisTypeName As String) As String()
        Dim myReturn As New List(Of String)

        Dim myObjects As New List(Of FHEMObj)
        myObjects.AddRange(glbKernel.CurrentObjects.Where(Function(x) x.FHEMType = thisTypeName))

        Dim myAttributes As New List(Of String)
        myObjects.ForEach(Sub(x) myAttributes.AddRange(x.Attributes.Keys.Cast(Of String)))

        myReturn = myAttributes.Distinct.ToList
        Return myReturn.OrderBy(Function(x) x).ToArray
    End Function

    <ComVisible(True)> Public Function GetObjectsByType(thisTypeName As String) As FHEMObj()
        Dim myReturn As New List(Of FHEMObj)
        myReturn.AddRange(glbKernel.CurrentObjects.Where(Function(x) x.FHEMType.Equals(thisTypeName)))
        Return myReturn.OrderBy(Function(x) x.Name).ToArray
    End Function
    <ComVisible(True)> Public Function FindObjectsByName(thisSearchString As String) As FHEMObj()
        Dim myReturn As New List(Of FHEMObj)
        myReturn.AddRange(glbKernel.CurrentObjects.Where(Function(x) x.Name.Contains(thisSearchString)))
        Return myReturn.OrderBy(Function(x) x.Name).ToArray
    End Function
    <ComVisible(True)> Public Function GetObjByName(thisName As String) As FHEMObj
        Return glbKernel.CurrentObjects.FirstOrDefault(Function(x) x.Name.Equals(thisName, StringComparison.CurrentCultureIgnoreCase))
    End Function

    <ComVisible(True)> Public Function GetObjAttributes(thisObjName As String) As String()
        Dim myObj As FHEMObj = glbKernel.CurrentObjects.FirstOrDefault(Function(x) x.Name.Equals(thisObjName))
        If myObj IsNot Nothing Then
            Return myObj.Attributes.Keys.Cast(Of String).OrderBy(Function(x) x).ToArray
        End If
        Return Nothing
    End Function
    <ComVisible(True)> Public Function GetObjAttribute(thisObjName As String, thisAttributeName As String) As String()
        Dim myObj As FHEMObj = glbKernel.CurrentObjects.FirstOrDefault(Function(x) x.Name.Equals(thisObjName))
        If myObj IsNot Nothing AndAlso myObj.Attributes.ContainsKey(thisAttributeName) Then
            Return DirectCast(myObj.Attributes(thisAttributeName), List(Of String)).OrderBy(Function(x) x).ToArray
        End If
        Return Nothing
    End Function

    <ComVisible(True)> Public Function GetObjRooms(thisObjName As String) As String()
        Dim myObj As FHEMObj = glbKernel.CurrentObjects.FirstOrDefault(Function(x) x.Name.Equals(thisObjName))
        If myObj IsNot Nothing Then
            Return myObj.Rooms.OrderBy(Function(x) x).ToArray
        End If
        Return Nothing
    End Function
    <ComVisible(True)> Public Function GetObjGroups(thisObjName As String) As String()
        Dim myObj As FHEMObj = glbKernel.CurrentObjects.FirstOrDefault(Function(x) x.Name.Equals(thisObjName))
        If myObj IsNot Nothing Then
            Return myObj.Groups.OrderBy(Function(x) x).ToArray
        End If
        Return Nothing
    End Function
End Class
#End Region

#Region "TemplateOperations"
Partial Class FHEMCOM
    <ComVisible(True)> Public Function GetTemplateNames() As String()
        Return glbKernel.GetTemplateNames.OrderBy(Function(x) x).ToArray
    End Function
    <ComVisible(True)> Public Function TemplateCount() As Integer
        Return glbKernel.GetTemplatesCount
    End Function
    <ComVisible(True)> Public Function GetTemplate(thisTemplateName As String) As FHEMTemplate
        Return glbKernel.GetTemplate(thisTemplateName)
    End Function
    <ComVisible(True)> Public Function GetTemplateFilterNames(thisTemplateName As String) As String()
        Return glbKernel.GetTemplateFilterNames(thisTemplateName).ToArray
    End Function
    <ComVisible(True)> Public Function GetTemplateFilter(thisTemplateName As String, thisFilterName As String) As ObjectFilter
        Return glbKernel.GetTemplateFilter(thisTemplateName, thisFilterName)
    End Function
    <ComVisible(True)> Public Function GetObjectTemplates(thisTemplateName As String, thisFilterName As String) As ObjectTemplate()
        Return glbKernel.GetObjectTemplates(thisTemplateName, thisFilterName).ToArray
    End Function
    <ComVisible(True)> Public Function GetObjectTemplate(thisTemplateName As String, thisFilterName As String, thisFHEMType As String) As ObjectTemplate
        Return glbKernel.GetObjectTemplate(thisTemplateName, thisFilterName, thisFHEMType)
    End Function
    <ComVisible(True)> Public Function GetObjectsByObjectFilter(thisTemplateName As String, thisFilterName As String) As FHEMObj()
        Return glbKernel.GetObjectsByFilter(thisTemplateName, thisFilterName).ToArray
    End Function
    <ComVisible(True)> Public Function GetObjectsByObjectFilter(thisFilter As ObjectFilter) As FHEMObj()
        Return glbKernel.GetObjectsByFilter(thisFilter).ToArray
    End Function
    <ComVisible(True)> Public Function GetTemplateCreatedObjects(thisTemplate, thisObjectFilter, thisObjectTemplate, thisFHEMObj) As FHEMObj()
        Return FHEMKernel.TemplateCreateObjects(thisTemplate, thisObjectFilter, thisObjectTemplate, thisFHEMObj).ToArray
    End Function

End Class
#End Region

#Region "ObjectStringOperations"
Partial Class FHEMCOM
    <ComVisible(True)> Public Function GetDoIfCheckItems(thisDefinition As String) As String()
        Dim myReturn As New List(Of String)
        Dim myRegExString As String = "((?:\(){1}(?:\[){1}(?<timespan>\+[^\)]*)(?:\]){1}(?:\)){1})|((?:\[){1}(?<device>[^\[\]]*)(?:\]){1})"
        Dim myRegEx As New Regex(myRegExString)
        If myRegEx.IsMatch(thisDefinition) Then
            Dim myMatches = myRegEx.Matches(thisDefinition)

            For Each myMatch As Match In myMatches
                If myMatch.Groups("device").Captures.Count > 0 Then myReturn.AddRange(myMatch.Groups("device").Captures.Cast(Of Capture).Select(Function(x) x.Value))
                If myMatch.Groups("timespan").Captures.Count > 0 Then myReturn.AddRange(myMatch.Groups("timespan").Captures.Cast(Of Capture).Select(Function(x) x.Value))
            Next
        End If
        Return myReturn.Distinct.ToArray
    End Function
    <ComVisible(True)> Public Function GetDoIfTrigger(thisDefinition As String) As String
        Dim myReturn As String = String.Empty
        Dim myRegExString As String = "(?<trigger>(?<=trigger )[^ ]*)"
        Dim myRegEx As New Regex(myRegExString)
        Dim myIndex As Integer = myRegEx.GroupNumberFromName("trigger")
        If myRegEx.IsMatch(thisDefinition) Then
            Dim myMatch = myRegEx.Match(thisDefinition)
            myReturn = myMatch.Groups(myIndex).Value
        End If
        Return myReturn
    End Function

    <ComVisible(True)> Public Function GetNotifyTrigger(thisDefinition As String) As String
        Dim myReturn As String = String.Empty
        Dim myRegExString As String = "(?<trigger>^[^\{ ]*(?= \{))"
        Dim myRegEx As New Regex(myRegExString)

        If myRegEx.IsMatch(thisDefinition) Then
            Dim myMatch = myRegEx.Match(thisDefinition)
            myReturn = myMatch.Groups("trigger").Value
        End If
        Return myReturn
    End Function
    <ComVisible(True)> Public Function GetNotifyActorItems(thisDefinition As String) As String()
        Dim myReturn As New List(Of String)

        Dim myRegExString As String = "(?<actor>(?<=\$act.*\="")[^""]*(?=""))"
        Dim myRegEx As New Regex(myRegExString)
        Dim myIndex As Integer = myRegEx.GroupNumberFromName("actor")
        If myRegEx.IsMatch(thisDefinition) Then
            Dim myMatches = myRegEx.Matches(thisDefinition)
            For Each myMatch As Match In myMatches
                myReturn.AddRange(myMatch.Groups(myIndex).Captures.Cast(Of Capture).Select(Function(x) x.Value))
            Next
        End If
        Return myReturn.Distinct.ToArray
    End Function
    <ComVisible(True)> Public Function GetNotifyCheckItems(thisDefinition As String) As String()
        Dim myReturn As New List(Of String)

        Dim myRegExString As String = "(?<check>(?<=\$che.*\="")[^""]*(?=""))"
        Dim myRegEx As New Regex(myRegExString)
        Dim myIndex As Integer = myRegEx.GroupNumberFromName("check")
        If myRegEx.IsMatch(thisDefinition) Then
            Dim myMatches = myRegEx.Matches(thisDefinition)
            For Each myMatch As Match In myMatches
                myReturn.AddRange(myMatch.Groups(myIndex).Captures.Cast(Of Capture).Select(Function(x) x.Value))
            Next
        End If
        Return myReturn.Distinct.ToArray
    End Function
End Class
#End Region

#Region "COM-Functions"
<ClassInterface(Runtime.InteropServices.ClassInterfaceType.AutoDual)> Partial Class FHEMCOM
    <ComRegisterFunction()> Public Shared Sub RegisterFunction(ByVal type As Type)
        Registry.ClassesRoot.CreateSubKey(GetSubkeyName(type, "Programmable"))

        Dim key = Registry.ClassesRoot.OpenSubKey(GetSubkeyName(type, "InprocServer32"), True)
        key.SetValue("", System.Environment.SystemDirectory + "\mscoree.dll", RegistryValueKind.String)
    End Sub

    <ComUnregisterFunction()> Public Shared Sub UnregisterFunction(ByVal type As Type)
        Registry.ClassesRoot.DeleteSubKey(GetSubkeyName(type, "Programmable"), False)
    End Sub

    Private Shared Function GetSubkeyName(ByVal type As Type, ByVal subkey As String) As String
        Dim S As New System.Text.StringBuilder()
        S.Append("CLSID\{")
        S.Append(type.GUID.ToString().ToUpper())
        S.Append("}\")
        S.Append(subkey)
        Return S.ToString()
    End Function
End Class
#End Region


#Region "IDisposable Support"
Partial Class FHEMCOM
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' dispose managed state (managed objects).
            End If

            ' free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' set large fields to null.
            glbKernel.CurrentObjects.Clear()
        End If
        disposedValue = True
    End Sub

    ' override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
    End Sub

End Class
#End Region
