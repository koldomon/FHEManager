Imports MM.FHEMManager.FHEMTemplates
Imports MM.FHEMManager.FHEMObjects
Imports System.Reflection

Partial Class FHEMKernel
    <Flags> Public Enum TemplateUpdateCommandCreationFlags
        None = 0
        CreateDefineExisting = 1
        CreateDefineNew = 2
        CreateModifyExisting = 4
        CreateModifyNew = 8
        CreateAttributsAll = 16
        CreateAttributsNew = 32
        CreateAttributsUpdatesValue = 64
    End Enum

    ''' <summary>
    ''' Load Methods
    ''' </summary>
    ''' <remarks></remarks>
    Public Function LoadTemplates() As Boolean?
        My.Application.Log.WriteEntry(String.Format("Load using default config: {0}{2}{1}", My.Settings.DefaultTemplatePath, My.Settings.DefaultTemplateName, cPathSplitChar(0)), TraceEventType.Information)
        If Not Me.LoadTemplates(String.Format("{0}{2}{1}", My.Settings.DefaultTemplatePath, My.Settings.DefaultTemplateName, cPathSplitChar(0))) Then
            Return False
        End If

        RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("AllTemplates"))

        Return True
    End Function
    Public Function LoadTemplates(ByVal thisTemplatePathAndFileName As String) As Boolean?
        If String.IsNullOrEmpty(thisTemplatePathAndFileName) Then Return False

        Me.ResetTemplates()

        Dim myPath As String = thisTemplatePathAndFileName.Trim(cPathTrimChars)

        My.Application.Log.WriteEntry(String.Format("Load using given template: {0}", thisTemplatePathAndFileName), TraceEventType.Information)
        If IO.File.Exists(myPath) Then
            Dim myManager As FHEMTemplateManager = Helper.ReadFromXML(GetType(FHEMTemplateManager), myPath)
            If Helper.SerializationEvents.Count = 0 Then
                If (myManager IsNot Nothing) AndAlso (myManager.Templates) IsNot Nothing AndAlso (myManager.Templates.Count > 0) Then
                    myManager.Templates.ToList.ForEach(Sub(x) Me.CurrentTemplates.Add(x))
                End If
                Return True
            Else
                Helper.SerializationEvents.ToList.ForEach(Sub(x) Me.SerializationErrors.Add(x))
                Return False
            End If
        Else
            My.Application.Log.WriteEntry(String.Format("IO.File.Exists({0}) returned False", myPath), TraceEventType.Warning)
            Return False
        End If
        Return False
    End Function

    Public Function GetTemplateNames() As Collection(Of String)
        Dim myReturn As New Collection(Of String)

        Me.CurrentTemplates.ToList.ForEach(Sub(x) myReturn.Add(x.Name))

        Return myReturn
    End Function

    Public Function GetTemplate(ByVal thisTemplateName As String) As FHEMTemplate
        If String.IsNullOrEmpty(thisTemplateName) Then Return Nothing

        Return Me.CurrentTemplates.FirstOrDefault(Function(x) x.Name = thisTemplateName)
    End Function

    Public Function GetTemplatesCount() As Integer
        Return Me.CurrentTemplates.Count
    End Function

End Class

Partial Class FHEMKernel
    Public Function GetTemplateFilterNames(ByVal thisTemplateName As String) As Collection(Of String)
        If String.IsNullOrEmpty(thisTemplateName) Then Return Nothing

        Dim myTemplate = Me.CurrentTemplates.FirstOrDefault(Function(x) x.Name = thisTemplateName)
        If myTemplate IsNot Nothing Then Return GetTemplateFilterNames(myTemplate)

        Return Nothing
    End Function
    <CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")>
    Public Function GetTemplateFilterNames(ByVal thisTemplate As FHEMTemplate) As Collection(Of String)
        If thisTemplate Is Nothing Then Return Nothing

        Dim myReturn As New Collection(Of String)
        thisTemplate.Filters.ToList.ForEach(Sub(x) myReturn.Add(x.Name))

        Return myReturn
    End Function
    Public Function GetTemplateFilter(ByVal thisTemplateName As String, ByVal thisFilterName As String) As ObjectFilter
        If String.IsNullOrEmpty(thisTemplateName) Then Return Nothing
        If String.IsNullOrEmpty(thisFilterName) Then Return Nothing

        Dim myTemplate = Me.CurrentTemplates.FirstOrDefault(Function(x) x.Name = thisTemplateName)
        If myTemplate IsNot Nothing Then Return GetTemplateFilter(myTemplate, thisFilterName)

        Return Nothing
    End Function
    <CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")>
    Public Function GetTemplateFilter(ByVal thisTemplate As FHEMTemplate, ByVal thisFilterName As String) As ObjectFilter
        If thisTemplate Is Nothing Then Return Nothing
        If String.IsNullOrEmpty(thisFilterName) Then Return Nothing

        Return thisTemplate.Filters.FirstOrDefault(Function(x) x.Name = thisFilterName)

        Return Nothing
    End Function

    Public Function GetObjectTemplates(ByVal thisTemplateName As String, ByVal thisFilterName As String) As Collection(Of ObjectTemplate)
        If String.IsNullOrEmpty(thisTemplateName) Then Return Nothing
        If String.IsNullOrEmpty(thisFilterName) Then Return Nothing

        Dim myTemplate = Me.CurrentTemplates.FirstOrDefault(Function(x) x.Name = thisTemplateName)
        If myTemplate IsNot Nothing Then
            Dim myFilter = myTemplate.Filters.FirstOrDefault(Function(x) x.Name = thisFilterName)
            If myFilter IsNot Nothing Then
                Return GetObjectTemplates(myTemplate, myFilter)
            End If
        End If

        Return Nothing
    End Function
    <CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")>
    Public Function GetObjectTemplates(ByVal thisTemplate As FHEMTemplate, ByVal thisFilter As ObjectFilter) As Collection(Of ObjectTemplate)
        If thisTemplate Is Nothing Then Return Nothing
        If thisFilter Is Nothing Then Return Nothing

        Dim myObjectTemplates = thisTemplate.ObjectTemplates.Where(Function(x) x.Filters.Contains(thisFilter.Name))
        If myObjectTemplates IsNot Nothing Then Return New Collection(Of ObjectTemplate)(myObjectTemplates)

        Return Nothing
    End Function
    Public Function GetObjectTemplate(ByVal thisTemplateName As String, ByVal thisFilterName As String, ByVal thisFHEMType As String) As ObjectTemplate
        If String.IsNullOrEmpty(thisTemplateName) Then Return Nothing
        If String.IsNullOrEmpty(thisFilterName) Then Return Nothing

        Dim myTemplate = Me.CurrentTemplates.FirstOrDefault(Function(x) x.Name = thisTemplateName)
        If myTemplate IsNot Nothing Then
            Dim myFilter = myTemplate.Filters.FirstOrDefault(Function(x) x.Name = thisFilterName)
            If myFilter IsNot Nothing Then Return GetObjectTemplate(myTemplate, myFilter, thisFHEMType)
        End If

        Return Nothing
    End Function
    <CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")>
    Public Function GetObjectTemplate(ByVal thisTemplate As FHEMTemplate, ByVal thisFilter As ObjectFilter, ByVal thisFHEMType As String) As ObjectTemplate
        If thisTemplate Is Nothing Then Return Nothing
        If thisFilter Is Nothing Then Return Nothing

        Return thisTemplate.ObjectTemplates.FirstOrDefault(Function(x) x.Filters.Contains(thisFilter.Name) AndAlso x.FHEMType.Equals(thisFHEMType))
    End Function

    Public Function GetObjectsByFilter(ByVal thisTemplateName As String, ByVal thisFilterName As String) As Collection(Of FHEMObj)
        If String.IsNullOrEmpty(thisTemplateName) Then Return Nothing
        If String.IsNullOrEmpty(thisFilterName) Then Return Nothing

        Dim myTemplate = Me.CurrentTemplates.FirstOrDefault(Function(x) x.Name = thisTemplateName)
        If myTemplate IsNot Nothing Then
            Dim myFilter = myTemplate.Filters.FirstOrDefault(Function(x) x.Name = thisFilterName)
            If myFilter IsNot Nothing Then
                Return GetObjectsByFilter(myFilter)
            End If
        End If

        Return Nothing
    End Function
    Public Function GetObjectsByFilter(ByVal thisFilter As ObjectFilter) As Collection(Of FHEMObj)
        If thisFilter Is Nothing Then Return Nothing

        Dim myReturn As List(Of FHEMObj) = Me.CurrentObjects.ToList

        If (myReturn.Count > 0) AndAlso (thisFilter.FHEMName IsNot Nothing) AndAlso (Not String.IsNullOrEmpty(thisFilter.FHEMName.ToString)) Then
            myReturn = myReturn.Where(Function(x) x.Name.Contains(thisFilter.FHEMName.ToString)).ToList
        End If

        If (myReturn.Count > 0) AndAlso (thisFilter.FHEMType IsNot Nothing) AndAlso (Not String.IsNullOrEmpty(thisFilter.FHEMType.ToString)) Then
            myReturn = myReturn.Where(Function(x) x.FHEMType.Equals(thisFilter.FHEMType.ToString)).ToList
        End If

        For Each myAttr In thisFilter.Attributes.ToList
            If (myReturn.Count > 0) Then
                myReturn = myReturn.Where(Function(x) x.Attributes.ContainsKey(myAttr.Key) AndAlso CType(x.Attributes(myAttr.Key), List(Of String)).Contains(myAttr.Value.ToString)).ToList
            End If
        Next

        Return New Collection(Of FHEMObj)(myReturn)
    End Function
End Class

Partial Class FHEMKernel
    Public Function CreateAllTemplateObjects(ByVal thisTemplate As FHEMTemplate) As Collection(Of FHEMObj)
        If thisTemplate Is Nothing Then Return Nothing

        Dim myReturn As New Collection(Of FHEMObj)

        For Each myFilter In thisTemplate.Filters.ToList
            CreateTemplateObjects(thisTemplate, myFilter).ToList.ForEach(Sub(x) myReturn.Add(x))
        Next

        Return myReturn
    End Function
    Public Function CreateTemplateObjects(ByVal thisTemplate As FHEMTemplate, ByVal thisFilterName As String) As Collection(Of FHEMObj)
        If thisTemplate Is Nothing Then Return Nothing
        If String.IsNullOrEmpty(thisFilterName) Then Return Nothing

        Dim myFilter As ObjectFilter = thisTemplate.Filters.FirstOrDefault(Function(x) x.Name = thisFilterName)
        If myFilter IsNot Nothing Then Return CreateTemplateObjects(thisTemplate, myFilter)

        Return Nothing
    End Function
    Public Function CreateTemplateObjects(ByVal thisTemplate As FHEMTemplate, ByVal thisFilter As ObjectFilter) As Collection(Of FHEMObj)
        If thisTemplate Is Nothing Then Return Nothing
        If thisFilter Is Nothing Then Return Nothing

        Dim myReturn As New Collection(Of FHEMObj)

        Dim myObjectTemplates = thisTemplate.ObjectTemplates.Where(Function(x) x.Filters.Contains(thisFilter.Name))
        If (myObjectTemplates IsNot Nothing) AndAlso (myObjectTemplates.Count > 0) Then
            Dim myAffectedObjects = GetObjectsByFilter(thisFilter)

            For Each myFhemObj In myAffectedObjects.OrderBy(Function(x) x.AliasName).ToList
                For Each myObjectTemplate In myObjectTemplates.ToList
                    TemplateCreateObjects(thisTemplate, thisFilter, myObjectTemplate, myFhemObj).ToList.ForEach(Sub(x) myReturn.Add(x))
                Next
            Next
        End If

        Return myReturn
    End Function

    Public Shared Function TemplateCreateObjects(ByVal thisTemplate As FHEMTemplate, ByVal thisTemplateFilter As ObjectFilter, ByVal thisObjectTemplate As ObjectTemplate, ByVal thisFHEMObj As FHEMObj) As Collection(Of FHEMObj)
        If thisTemplate Is Nothing Then Return Nothing
        If thisTemplateFilter Is Nothing Then Return Nothing
        If thisObjectTemplate Is Nothing Then Return Nothing
        If thisFHEMObj Is Nothing Then Return Nothing

        Dim myReturn As New Collection(Of FHEMObj)

        If thisObjectTemplate.Init(thisTemplate, thisTemplateFilter.Name, thisFHEMObj) AndAlso thisObjectTemplate.ValidateAndCreate() Then
            thisObjectTemplate.GetCreatedFHEMObjs.ToList.ForEach(Sub(x) myReturn.Add(x))
        End If

        Return myReturn
    End Function
End Class

Partial Class FHEMKernel
    Public Function GetTemplateUpdateCommands(ByVal thisNewFHEMObjs As Collection(Of FHEMObj), ByVal thisUpdateOptions As TemplateUpdateCommandCreationFlags) As String
        If thisNewFHEMObjs Is Nothing Then Return Nothing

        Dim myReturn As New List(Of String)

        For Each myNewObj In thisNewFHEMObjs.ToList
            myReturn.Add(GetTemplateUpdateCommand(myNewObj, thisUpdateOptions))
            myReturn.Add(String.Empty)
        Next

        Return String.Join(vbCrLf, myReturn.ToArray)
    End Function

    Public Function GetTemplateUpdateCommand(ByVal thisNewFhemObj As FHEMObj, ByVal thisUpdateOptions As TemplateUpdateCommandCreationFlags) As String
        If thisNewFhemObj Is Nothing Then Return Nothing

        Dim myReturn As New List(Of String)

        Dim myCurrentObj As FHEMObj = Me.CurrentObjects.FirstOrDefault(Function(x) x.Name = thisNewFhemObj.Name)
        If myCurrentObj Is Nothing Then
            If (thisUpdateOptions And TemplateUpdateCommandCreationFlags.CreateDefineNew) Or (thisUpdateOptions And TemplateUpdateCommandCreationFlags.CreateDefineExisting) Then
                myReturn.Add(thisNewFhemObj.GetDefineString())

                If (thisUpdateOptions And TemplateUpdateCommandCreationFlags.CreateModifyNew) Or (thisUpdateOptions And TemplateUpdateCommandCreationFlags.CreateModifyExisting) Then
                    myReturn.Add(thisNewFhemObj.GetModifyString())
                End If

                myReturn.AddRange(thisNewFhemObj.GetAttributeStrings)
            End If
        Else
            If (thisUpdateOptions And TemplateUpdateCommandCreationFlags.CreateDefineNew) Then
                myReturn.Add(thisNewFhemObj.GetDefineString())
            ElseIf (thisUpdateOptions And TemplateUpdateCommandCreationFlags.CreateDefineExisting) Then
                myReturn.Add(thisNewFhemObj.GetDefineString)
            ElseIf (thisUpdateOptions And TemplateUpdateCommandCreationFlags.CreateModifyNew) Then
                If (Not (String.IsNullOrEmpty(thisNewFhemObj.Definition) Or String.IsNullOrEmpty(myCurrentObj.Definition))) AndAlso (Not myCurrentObj.Definition.Equals(thisNewFhemObj.Definition)) Then
                    myReturn.Add(thisNewFhemObj.GetModifyString)
                End If
            ElseIf (thisUpdateOptions And TemplateUpdateCommandCreationFlags.CreateModifyExisting) Then
                If (Not (String.IsNullOrEmpty(thisNewFhemObj.Definition) Or String.IsNullOrEmpty(myCurrentObj.Definition))) AndAlso (Not myCurrentObj.Definition.Equals(thisNewFhemObj.Definition)) Then
                    myReturn.Add(thisNewFhemObj.GetModifyString)
                End If
            End If

            If (thisUpdateOptions And TemplateUpdateCommandCreationFlags.CreateAttributsAll) Then
                myReturn.AddRange(thisNewFhemObj.GetAttributeStrings)
            ElseIf (thisUpdateOptions And TemplateUpdateCommandCreationFlags.CreateAttributsNew) Then
                For Each myKey In thisNewFhemObj.Attributes.Keys.Cast(Of String).ToList
                    If Not myCurrentObj.Attributes.ContainsKey(myKey) Then
                        myReturn.Add(thisNewFhemObj.GetAttributeDefineString(myKey))
                    End If
                Next
            ElseIf (thisUpdateOptions And TemplateUpdateCommandCreationFlags.CreateAttributsUpdatesValue) Then
                For Each myKey In thisNewFhemObj.Attributes.Keys.Cast(Of String).ToList
                    If Not myCurrentObj.Attributes.ContainsKey(myKey) Then
                        myReturn.Add(thisNewFhemObj.GetAttributeDefineString(myKey))
                    Else
                        If Not thisNewFhemObj.GetAttributeDefineString(myKey).Equals(myCurrentObj.GetAttributeDefineString(myKey)) Then myReturn.Add(thisNewFhemObj.GetAttributeDefineString(myKey))
                    End If
                Next
            End If
        End If

        Return String.Join(vbCrLf, myReturn.ToArray)
    End Function
End Class

Partial Class FHEMKernel
    Public Function GenerateTemplate(ByVal thisTemplateName As String, ByVal thisCurrentObjectTemplateName As String, ByVal thisTemplateFHEMObjName As String) As FHEMTemplate
        If String.IsNullOrEmpty(thisTemplateName) Then Return Nothing
        If String.IsNullOrEmpty(thisCurrentObjectTemplateName) Then Return Nothing
        If String.IsNullOrEmpty(thisTemplateFHEMObjName) Then Return Nothing

        Dim myReturn As FHEMTemplate = Me.CurrentTemplates.FirstOrDefault(Function(x) x.Name = thisTemplateName)
        If myReturn Is Nothing Then myReturn = New FHEMTemplate With {.Name = thisTemplateName}

        Dim myCurrentObjectTemplate = Me.CurrentObjects.FirstOrDefault(Function(x) x.Name = thisCurrentObjectTemplateName)
        If myCurrentObjectTemplate Is Nothing Then
            My.Application.Log.WriteEntry(String.Format("TemplateObject ""{0}"" not found!", thisCurrentObjectTemplateName))
            Return Nothing
        End If

        Dim myTemplateFHEMObj = Me.CurrentObjects.FirstOrDefault(Function(x) x.Name = thisTemplateFHEMObjName)
        If myTemplateFHEMObj Is Nothing Then
            My.Application.Log.WriteEntry(String.Format("FilterObject ""{0}"" not found!", thisTemplateFHEMObjName))
            Return Nothing
        End If

        myReturn.ObjectTemplates.Add(GenerateObjectTemplate(myReturn, Nothing, myCurrentObjectTemplate, myTemplateFHEMObj, True))
        Return myReturn
    End Function
    Public Function GenerateObjectTemplate(ByVal thisTemplate As FHEMTemplate, ByVal thisParentFHEMObj As FHEMObj, ByVal thisCurrentObjectTemplate As FHEMObj, ByVal thisTemplateFHEMObj As FHEMObj, ByVal thisAddAssociatedObjects As Boolean) As ObjectTemplate
        If thisTemplate Is Nothing Then Return Nothing
        If thisCurrentObjectTemplate Is Nothing Then Return Nothing
        If thisTemplateFHEMObj Is Nothing Then Return Nothing

        Dim myReturn As New ObjectTemplate With {
                                                .FHEMType = thisCurrentObjectTemplate.FHEMType
                                                }

        Dim myAddObjects As New List(Of String)
        Dim myInspectionResults = GetInspectionResults(thisTemplate, thisParentFHEMObj, thisCurrentObjectTemplate, thisTemplateFHEMObj)

        'Process Name
        myReturn.Name = GenerateStringReplaceObject(thisCurrentObjectTemplate.Name, myInspectionResults, myAddObjects)

        'Process Definition
        If Not String.IsNullOrEmpty(thisCurrentObjectTemplate.Definition) Then
            myReturn.Definition = GenerateStringReplaceObject(thisCurrentObjectTemplate.Definition, myInspectionResults, myAddObjects)
        End If

        'Process Attributes
        For Each myKey In thisCurrentObjectTemplate.Attributes.Keys
            myReturn.Attributes.Add(GenerateKeyValueObject(myKey, thisCurrentObjectTemplate.GetAttributeString(myKey), myInspectionResults, myAddObjects))
        Next

        'Process found dummies
        For Each myAddObject In myAddObjects
            Dim myDummy As FHEMObj = Me.CurrentObjects.FirstOrDefault(Function(x) x.FHEMType = "dummy" AndAlso x.Name = myAddObject)
            Dim myDummyTemplate = GenerateObjectTemplate(thisTemplate, Nothing, myDummy, thisTemplateFHEMObj, False)
            If myDummyTemplate IsNot Nothing Then
                thisTemplate.Filters.ToList.ForEach(Sub(x) myDummyTemplate.Filters.Add(x.Name))
                If (myDummy IsNot Nothing) Then thisTemplate.ObjectTemplates.Add(myDummyTemplate)
            End If
        Next

        If thisAddAssociatedObjects Then
            'Process accociated Objects
            For Each myObj In thisCurrentObjectTemplate.Associations.Where(Function(x) x.FHEMType <> myReturn.FHEMType)
                Select Case myObj.FHEMType
                    Case "at", "DOIF", "FileLog", "notify", "SVG"
                        myReturn.ObjectTemplates.Add(GenerateObjectTemplate(thisTemplate, thisCurrentObjectTemplate, myObj, thisTemplateFHEMObj, True))
                    Case Else
                        My.Application.Log.WriteEntry(String.Format("Skip templating nested Type {0}", myObj.FHEMType), TraceEventType.Information)
                End Select
            Next
        End If

        Return myReturn
    End Function
    Private Function GenerateStringReplaceObject(ByVal thisOrgString As String, ByRef thisInspectionResults As List(Of InspectResult), ByRef thisAddObjects As List(Of String)) As StringReplaceObjectBase
        Dim myReturn As StringReplaceObjectBase = Nothing

        Dim funcAffectedResults = Function(x As InspectResult) (Not String.Equals(thisOrgString, x.PropValue)) AndAlso (thisOrgString.Contains(x.PropValue))

        If thisInspectionResults.FirstOrDefault(funcAffectedResults) IsNot Nothing Then
            Dim myReplaceCounter As Integer = 0
            myReturn = New StringFormatObject
            DirectCast(myReturn, StringFormatObject).FormatString = thisOrgString

            For Each myIR In thisInspectionResults.Where(funcAffectedResults).OrderByDescending(Function(x) x.PropValue.Length).ThenBy(Function(x) x.SourceSpecifier).ToList
                If DirectCast(myReturn, StringFormatObject).FormatString.Contains(myIR.PropValue) Then
                    Dim myReplace As KeyValueObject = Nothing
                    Dim myKey As String = String.Format("{{{0}}}", myReplaceCounter.ToString("d2"))

                    Select Case myIR.SourceSpecifier
                        Case ObjectSpecifier.Other
                            myReplace = New KeyValueObject With {.Key = myKey,
                                                                 .Value = GenerateStringReplaceObject(myIR.PropValue, thisInspectionResults, thisAddObjects)
                                                                }
                            If Not thisAddObjects.Contains(myIR.PropValue) Then thisAddObjects.Add(myIR.PropValue)
                        Case Else
                            Select Case myIR.PropName
                                Case "Attributes"
                                    myReplace = New KeyValueObject With {.Key = myKey,
                                                                             .Value = New ReferenceObject With {.ObjectSpecifier = myIR.SourceSpecifier,
                                                                                                                .Property = New KeyValueObject With {.Key = myIR.PropName,
                                                                                                                                                     .Value = New KeyValueObject With {.Key = myIR.AttributeName,
                                                                                                                                                                                       .Value = New StringObject With {.Value = myIR.AttributeIndex}
                                                                                                                                                                                      }
                                                                                                                                                    }
                                                                                                                }
                                                                            }
                                Case Else
                                    myReplace = New KeyValueObject With {.Key = myKey,
                                                                     .Value = New ReferenceObject With {.ObjectSpecifier = myIR.SourceSpecifier,
                                                                                                        .Property = New StringObject With {.Value = myIR.PropName}
                                                                                                        }
                                                                    }
                            End Select
                    End Select

                    If myReturn IsNot Nothing Then
                        DirectCast(myReturn, StringFormatObject).FormatString = DirectCast(myReturn, StringFormatObject).FormatString.Replace(myIR.PropValue, myReplace.Key)
                        DirectCast(myReturn, StringFormatObject).Values.Add(myReplace)
                        myReplaceCounter += 1
                    End If
                End If
            Next
        Else
            myReturn = New StringObject With {.Value = thisOrgString}
        End If

        Return myReturn
    End Function
    Private Function GenerateKeyValueObject(ByVal thisKey As String, ByVal thisOrgString As String, ByRef thisInspectionResults As List(Of InspectResult), ByRef thisAddObjects As List(Of String)) As KeyValueObject
        Dim myReturn As New KeyValueObject With {.Key = thisKey}

        Dim funcAffectedResults = Function(x) (Not thisOrgString.Equals(x.PropValue)) AndAlso (thisOrgString.Contains(x.PropValue))

        If thisInspectionResults.FirstOrDefault(funcAffectedResults) IsNot Nothing Then
            myReturn.Value = GenerateStringReplaceObject(thisOrgString, thisInspectionResults, thisAddObjects)
        Else
            myReturn.Value = New StringObject With {.Value = thisOrgString}
        End If

        Return myReturn
    End Function

    Private Function GetInspectionResults(ByVal thisTemplate As FHEMTemplate, thisParentFHEMObj As FHEMObj, thisCurrentObjectTemplate As FHEMObj, thisTemplateFHEMObj As FHEMObj) As List(Of InspectResult)
        Dim myReturn As New List(Of InspectResult)
        'Add Template
        myReturn.Add(New InspectResult With {.PropName = "Name", .PropValue = thisTemplate.Name, .SourceSpecifier = ObjectSpecifier.Template})
        'Add ObjectTemplate
        myReturn.Add(New InspectResult With {.PropName = "FHEMType", .PropValue = thisCurrentObjectTemplate.FHEMType, .SourceSpecifier = ObjectSpecifier.CurrentObjectTemplate})
        'Add ParentObject is not null
        If thisParentFHEMObj IsNot Nothing Then myReturn.AddRange(InspectFhemObject(thisParentFHEMObj, ObjectSpecifier.ParentFHEMObj))
        'Add FilterObject
        myReturn.AddRange(InspectFhemObject(thisTemplateFHEMObj, ObjectSpecifier.TemplateFHEMObj))
        'Add Names from all Objects
        myReturn.AddRange(GetGlobalInspectionResults)

        Dim myDuplicate As InspectResult = Nothing
        'Remove the ParrentFHEMObj from the GlobalInspectionResults if not null
        If thisParentFHEMObj IsNot Nothing Then
            myDuplicate = myReturn.FirstOrDefault(Function(x) x.SourceSpecifier = ObjectSpecifier.Other AndAlso x.PropValue = thisParentFHEMObj.Name)
            If myDuplicate IsNot Nothing Then myReturn.Remove(myDuplicate)
        End If
        'Remove the CurrentObject from the GlobalInspectionResults
        myDuplicate = myReturn.FirstOrDefault(Function(x) x.SourceSpecifier = ObjectSpecifier.Other AndAlso x.PropValue = thisCurrentObjectTemplate.Name)
        If myDuplicate IsNot Nothing Then myReturn.Remove(myDuplicate)
        'Remove the TemplateFHEMObject from the GlobalInspectionResults
        myDuplicate = myReturn.FirstOrDefault(Function(x) x.SourceSpecifier = ObjectSpecifier.Other AndAlso x.PropValue = thisTemplateFHEMObj.Name)
        If myDuplicate IsNot Nothing Then myReturn.Remove(myDuplicate)

        Return myReturn
    End Function
    Private Class InspectResult
        Public Property SourceSpecifier As ObjectSpecifier
        Public Property PropName As String
        Public Property PropValue As String
        Public Property AttributeName As String
        Public Property AttributeIndex As Integer
    End Class
    Private GlobalInspectionResults As New List(Of InspectResult)
    Private Function GetGlobalInspectionResults() As List(Of InspectResult)
        If Me.GlobalInspectionResults.Count = 0 Then
            Me.CurrentObjects.ToList.ForEach(Sub(x) Me.GlobalInspectionResults.Add(New InspectResult With {.SourceSpecifier = ObjectSpecifier.Other,
                                                                                                            .PropName = "Name",
                                                                                                            .PropValue = x.Name}))
            Return Me.GlobalInspectionResults
        Else
            Return Me.GlobalInspectionResults
        End If
    End Function
    Private Function InspectFhemObject(ByVal thisObj As FHEMObj, ByVal thisSourceSpecifier As ObjectSpecifier) As List(Of InspectResult)
        Dim myReturn As New List(Of InspectResult)

        Dim myInspectProperties() As String = New String() {"Name", "Attributes"}

        For Each myInspectProperty In myInspectProperties
            Dim myProp = thisObj.GetType.GetProperty(myInspectProperty)
            If myProp IsNot Nothing Then
                Select Case myProp.PropertyType
                    Case GetType(String)
                        If Not myProp.GetValue(thisObj).ToString.Contains(" ") Then myReturn.Add(New InspectResult With {.SourceSpecifier = thisSourceSpecifier,
                                                                .PropName = myProp.Name,
                                                                .PropValue = myProp.GetValue(thisObj).ToString})
                    Case GetType(Hashtable)
                        Dim myAtttributes As Hashtable = DirectCast(myProp.GetValue(thisObj), Hashtable)
                        For Each myKey In myAtttributes.Keys
                            DirectCast(myAtttributes(myKey), List(Of String)).ForEach(Sub(x) If (x.Length >= 2) AndAlso (Not x.Contains(" ")) Then myReturn.Add(New InspectResult With {.SourceSpecifier = thisSourceSpecifier,
                                                                                                                                    .PropName = myProp.Name,
                                                                                                                                    .PropValue = x,
                                                                                                                                    .AttributeName = myKey,
                                                                                                                                    .AttributeIndex = DirectCast(myAtttributes(myKey), List(Of String)).IndexOf(x)}))
                        Next
                End Select
            End If
        Next

        Return myReturn
    End Function
End Class