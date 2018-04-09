Imports MM.FHEMManager.FHEMObjects
Imports MM.FHEMManager.FHEMTemplates
Imports System.IO
Imports System.Xml
Imports System.Xml.Serialization

<TestClass()> Public Class UnitTestKernelTemplates
    Private Shared CurrentFHEM As FHEMKernel
    Private Shared SyncObj As New Object()
    Public Sub Initialize()
        If CurrentFHEM Is Nothing Then
            SyncLock SyncObj
                If CurrentFHEM Is Nothing Then CurrentFHEM = GetFhem()
            End SyncLock
        End If
    End Sub
    Private Function GetFhem() As FHEMKernel
        Dim myReturn As FHEMKernel
        myReturn = New FHEMKernel

        Dim myCfgFileName As String = String.Format("{0}\TestFiles\fhem.cfg", My.Application.Info.DirectoryPath)
        Assert.IsTrue(IO.File.Exists(myCfgFileName))
        Assert.IsTrue(myReturn.LoadCfg(myCfgFileName))
        Assert.IsNotNull(myReturn.CurrentObjects)
        Assert.IsTrue(myReturn.CurrentObjects.Count > 0)

        Dim myTemplateFileName As String = String.Format("{0}\TestFiles\Templates\FHEMTemplate.xml", My.Application.Info.DirectoryPath)
        Assert.IsTrue(IO.File.Exists(myTemplateFileName))
        Assert.IsTrue(myReturn.LoadTemplates(myTemplateFileName))
        Assert.IsNotNull(myReturn.CurrentTemplates)
        Assert.IsTrue(myReturn.CurrentTemplates.Count > 0)

        Return myReturn
    End Function
    <TestMethod()> Public Sub TestFhem()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)

        Assert.IsNotNull(CurrentFHEM.CurrentObjects)
        Assert.IsTrue(CurrentFHEM.CurrentObjects.Count > 0)
        Assert.IsNotNull(CurrentFHEM.CurrentTemplates)
        Assert.IsTrue(CurrentFHEM.CurrentTemplates.Count > 0)
    End Sub

    <TestMethod> Public Sub TestGetTemplates()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)
        Assert.IsNotNull(CurrentFHEM.CurrentTemplates)
        Assert.IsTrue(CurrentFHEM.CurrentTemplates.Count > 0)

        Dim myTemplates = CurrentFHEM.CurrentTemplates
        Assert.IsNotNull(myTemplates)
        Assert.IsTrue(myTemplates.Count)

        myTemplates.ToList.ForEach(Sub(x) AssertAndDebugPrintFHEMTemplate(x))
    End Sub
    <TestMethod> Public Sub TestTemplateFilters()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)
        Assert.IsNotNull(CurrentFHEM.CurrentObjects)
        Assert.IsTrue(CurrentFHEM.CurrentObjects.Count > 0)
        Assert.IsNotNull(CurrentFHEM.CurrentTemplates)
        Assert.IsTrue(CurrentFHEM.CurrentTemplates.Count > 0)

        Dim myTemplates = CurrentFHEM.CurrentTemplates
        Assert.IsNotNull(myTemplates)
        Assert.IsTrue(myTemplates.Count)

        myTemplates.ToList.ForEach(Sub(x)
                                       Assert.IsNotNull(x.Filters)
                                       Assert.IsTrue(x.Filters.Count > 0)
                                       x.Filters.ToList.ForEach(Sub(y)
                                                                    AssertAndDebugPrintObjectFilter(y)
                                                                    Dim myFilteredObjectTemplates = x.ObjectTemplates.Where(Function(z) z.Filters.Contains(y.Name))
                                                                    Assert.IsNotNull(myFilteredObjectTemplates)
                                                                    'Assert.IsTrue(myFilteredObjectTemplates.Count > 0)
                                                                    Debug.Indent()
                                                                    Debug.Print(".ObjectTemplates.Where(.FilterName.Equals(.Name)).Count:'{0}' .Items({1})",
                                                                                New Object() {myFilteredObjectTemplates.Count,
                                                                                              String.Join(",", myFilteredObjectTemplates.Select(Of String)(Function(z) String.Format("'{0}'", z.FHEMType)
                                                                                                                                  ).ToArray
                                                                                                          )
                                                                                              }
                                                                                )
                                                                    Debug.Unindent()
                                                                End Sub)

                                   End Sub)
    End Sub
    <TestMethod> Public Sub TestTemplateGetObjectsByFilter()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)
        Assert.IsNotNull(CurrentFHEM.CurrentObjects)
        Assert.IsTrue(CurrentFHEM.CurrentObjects.Count > 0)
        Assert.IsNotNull(CurrentFHEM.CurrentTemplates)
        Assert.IsTrue(CurrentFHEM.CurrentTemplates.Count > 0)

        Dim myTemplates = CurrentFHEM.CurrentTemplates
        Assert.IsNotNull(myTemplates)
        Assert.IsTrue(myTemplates.Count)

        myTemplates.ToList.ForEach(Sub(x)
                                       AssertAndDebugPrintFHEMTemplate(x)
                                       Assert.IsNotNull(x.Filters)
                                       Assert.IsTrue(x.Filters.Count > 0)
                                       x.Filters.ToList.ForEach(Sub(y)
                                                                    AssertAndDebugPrintObjectFilter(y)
                                                                    Dim myFilteredObjects = CurrentFHEM.GetObjectsByFilter(y)
                                                                    Assert.IsNotNull(myFilteredObjects)
                                                                    'Assert.IsTrue(myFilteredObjects.Count > 0)

                                                                    Debug.Indent()
                                                                    Debug.Print(".GetObjectsByFilter.Count:'{0}' .Items({1})",
                                                                                New Object() {myFilteredObjects.Count,
                                                                                              String.Join(",", myFilteredObjects.Select(Of String)(Function(z)
                                                                                                                                                       Dim myRoom As String = String.Empty
                                                                                                                                                       If z.Attributes.ContainsKey("room") Then
                                                                                                                                                           Dim myValues = DirectCast(z.Attributes("room"), List(Of String))
                                                                                                                                                           If myValues.Count = 1 Then
                                                                                                                                                               myRoom = myValues(0)
                                                                                                                                                           ElseIf myValues.Count < 1 Then
                                                                                                                                                               myRoom = myValues(1)
                                                                                                                                                           End If
                                                                                                                                                       End If
                                                                                                                                                       Return String.Format("{{'{0}','{1}'}}", z.Name, myRoom)
                                                                                                                                                   End Function
                                                                                                                                  ).ToArray
                                                                                                          )
                                                                                              }
                                                                                )
                                                                    Debug.Unindent()
                                                                End Sub)

                                   End Sub)
    End Sub
    <TestMethod> Public Sub TestGetTemplateObjectTemplateFHEMObjs()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)
        Assert.IsNotNull(CurrentFHEM.CurrentObjects)
        Assert.IsTrue(CurrentFHEM.CurrentObjects.Count > 0)
        Assert.IsNotNull(CurrentFHEM.CurrentTemplates)
        Assert.IsTrue(CurrentFHEM.CurrentTemplates.Count > 0)

        Dim myTemplates = CurrentFHEM.CurrentTemplates
        Assert.IsNotNull(myTemplates)
        Assert.IsTrue(myTemplates.Count)

        myTemplates.ToList.ForEach(Sub(thisTemplate)
                                       AssertAndDebugPrintFHEMTemplate(thisTemplate)
                                       Assert.IsNotNull(thisTemplate.Filters)
                                       Assert.IsTrue(thisTemplate.Filters.Count > 0)
                                       thisTemplate.Filters.ToList.ForEach(Sub(thisObjectFilter)
                                                                               AssertAndDebugPrintObjectFilter(thisObjectFilter)
                                                                               Dim myFilteredObjects = CurrentFHEM.GetObjectsByFilter(thisObjectFilter)
                                                                               Assert.IsNotNull(myFilteredObjects)
                                                                               'Assert.IsTrue(myFilteredObjects.Count > 0)

                                                                               Debug.Indent()
                                                                               Debug.Print(".GetObjectsByFilter.Count:'{0}'", New Object() {myFilteredObjects.Count})
                                                                               Debug.Unindent()

                                                                               Dim myFilteredObjectTemplates = thisTemplate.ObjectTemplates.Where(Function(thisObjectTemplate) thisObjectTemplate.Filters.Contains(thisObjectFilter.Name))
                                                                               Assert.IsNotNull(myFilteredObjectTemplates)
                                                                               'Assert.IsTrue(myFilteredObjectTemplates.Count > 0)
                                                                               Debug.Indent()
                                                                               Debug.Print(".FilteredObjectTemplates.Count:'{0}'", New Object() {myFilteredObjectTemplates.Count})
                                                                               Debug.Unindent()

                                                                               myFilteredObjects.ToList.ForEach(Sub(thisFHEMObj) myFilteredObjectTemplates.ToList.ForEach(Sub(thisObjectTemplate) AssertAndDebugPrintObjectFilterGetObjects(thisTemplate, thisObjectFilter, thisObjectTemplate, thisFHEMObj)))
                                                                           End Sub)

                                   End Sub)
    End Sub
    <TestMethod> Public Sub TestGetTemplateObjectTemplateFHEMObjsUpdateCommands()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)
        Assert.IsNotNull(CurrentFHEM.CurrentObjects)
        Assert.IsTrue(CurrentFHEM.CurrentObjects.Count > 0)
        Assert.IsNotNull(CurrentFHEM.CurrentTemplates)
        Assert.IsTrue(CurrentFHEM.CurrentTemplates.Count > 0)

        Dim myTemplates = CurrentFHEM.CurrentTemplates
        Assert.IsNotNull(myTemplates)
        Assert.IsTrue(myTemplates.Count)

        Dim myNewObjectsFileName As String = String.Format("{0}\TestFiles\Templates\FHEMGetTemplateObjectTemplateFHEMObjsUpdateCommands.cfg", My.Application.Info.DirectoryPath)
        myTemplates.ToList.ForEach(Sub(thisTemplate)
                                       Dim myNewObjects = CurrentFHEM.CreateAllTemplateObjects(thisTemplate).Distinct()
                                       Assert.IsNotNull(myNewObjects)
                                       Assert.IsTrue(myNewObjects.Count > 0)

                                       myNewObjects.ToList.ForEach(Sub(thisNewObject)
                                                                       Dim myCommand = CurrentFHEM.GetTemplateUpdateCommand(thisNewObject, FHEMKernel.TemplateUpdateCommandCreationFlags.CreateDefineNew Or FHEMKernel.TemplateUpdateCommandCreationFlags.CreateAttributsUpdatesValue)
                                                                       If Not String.IsNullOrEmpty(myCommand) Then
                                                                           Helper.WriteToFile(myCommand, myNewObjectsFileName, FileMode.Append)
                                                                           Helper.WriteToFile(String.Empty, myNewObjectsFileName, FileMode.Append)
                                                                       End If
                                                                   End Sub)
                                   End Sub)
    End Sub

    <TestMethod> Public Sub TestGenerateObjectTemplate()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)
        Assert.IsNotNull(CurrentFHEM.CurrentObjects)
        Assert.IsTrue(CurrentFHEM.CurrentObjects.Count > 0)

        Dim myOrgTemplate As FHEMTemplate = CurrentFHEM.GetTemplate("Heizung")
        If myOrgTemplate IsNot Nothing Then CurrentFHEM.CurrentTemplates.Remove(myOrgTemplate)

        'Create new Template and TemplateManager. Add Template to Manager
        Dim myTemplate As FHEMTemplate = Nothing
        Dim myFHEMTemplateManager As New FHEMTemplateManager

        myTemplate = CurrentFHEM.GenerateTemplate("Heizung", "02_OG_Bad_Heizung_Timer_DOIF", "HM_123456_Clima")
        Assert.IsNotNull(myTemplate)
        Assert.IsTrue(myTemplate.ObjectTemplates.Count > 0)

        myFHEMTemplateManager.Templates.Add(myTemplate)

        Dim myTemplateFileName As String = String.Format("{0}\TestFiles\Templates\FHEMGenerateObjectTemplate.xml", My.Application.Info.DirectoryPath)
        Helper.WriteToXML(myFHEMTemplateManager, myTemplateFileName, FileMode.Create)

        If myOrgTemplate IsNot Nothing Then CurrentFHEM.CurrentTemplates.Add(myOrgTemplate)
    End Sub
    <TestMethod> Public Sub TestGenerateObjectTemplateCreateCommand()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)
        Assert.IsNotNull(CurrentFHEM.CurrentObjects)
        Assert.IsTrue(CurrentFHEM.CurrentObjects.Count > 0)

        Dim myOrgTemplate As FHEMTemplate = CurrentFHEM.GetTemplate("Heizung")
        If myOrgTemplate IsNot Nothing Then CurrentFHEM.CurrentTemplates.Remove(myOrgTemplate)

        'Create new Template and TemplateManager. Add Template to Manager
        Dim myTemplate As FHEMTemplate = Nothing
        Dim myFHEMTemplateManager As New FHEMTemplateManager

        myTemplate = CurrentFHEM.GenerateTemplate("Heizung", "02_OG_Bad_Heizung_Timer_DOIF", "HM_123456_Clima")
        Assert.IsNotNull(myTemplate)
        Assert.IsTrue(myTemplate.ObjectTemplates.Count > 0)

        myFHEMTemplateManager.Templates.Add(myTemplate)

        Dim myNewTemplateFileName As String = String.Format("{0}\TestFiles\Templates\FHEMGenerateObjectTemplateCreateCommand.xml", My.Application.Info.DirectoryPath)
        Helper.WriteToXML(myFHEMTemplateManager, myNewTemplateFileName, FileMode.Create)

        Dim myTestObj As FHEMObj = CurrentFHEM.CurrentObjects.FirstOrDefault(Function(x) x.Name = "HM_123457_Clima")
        Dim myNewObjects As New List(Of FHEMObj)
        myTemplate.ObjectTemplates.ToList.ForEach(Sub(x) myNewObjects.AddRange(FHEMKernel.TemplateCreateObjects(myTemplate, New ObjectFilter, x, myTestObj)))
        Assert.IsNotNull(myNewObjects)
        Assert.IsTrue(myNewObjects.Count > 0)

        Dim myNewObjectsFileName As String = String.Format("{0}\TestFiles\Templates\FHEMGenerateObjectTemplateCreateCommand.cfg", My.Application.Info.DirectoryPath)
        If IO.File.Exists(myNewObjectsFileName) Then IO.File.Delete(myNewObjectsFileName)

        myNewObjects.ToList.ForEach(Sub(thisNewObject)
                                        Helper.WriteToFile(thisNewObject.GetCfgDefineString, myNewObjectsFileName, FileMode.Append)
                                        Helper.WriteToFile(String.Empty, myNewObjectsFileName, FileMode.Append)
                                    End Sub)

        If myOrgTemplate IsNot Nothing Then CurrentFHEM.CurrentTemplates.Add(myOrgTemplate)
    End Sub
    <TestMethod> Public Sub TestGenerateObjectTemplateHeizungMitVorlauf()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)
        Assert.IsNotNull(CurrentFHEM.CurrentObjects)
        Assert.IsTrue(CurrentFHEM.CurrentObjects.Count > 0)

        Dim myOrgTemplate As FHEMTemplate = CurrentFHEM.GetTemplate("Heizung")
        If myOrgTemplate IsNot Nothing Then CurrentFHEM.CurrentTemplates.Remove(myOrgTemplate)

        'Create new Template and TemplateManager. Add Template to Manager
        Dim myTemplate As FHEMTemplate = Nothing
        Dim myFHEMTemplateManager As New FHEMTemplateManager

        'Create new Template and add to CurrentFHEM
        myTemplate = New FHEMTemplate With {.Name = "Heizung mit Vorlauf"}
        myFHEMTemplateManager.Templates.Add(myTemplate)

        CurrentFHEM.CurrentTemplates.Add(myTemplate)

        'Add FilterObjects
        myTemplate.Filters.Add(CreateTestObjectFilter("HM_123456_Clima"))
        myTemplate.Filters.Add(CreateTestObjectFilter("HM_123457_Clima"))

        'Create the ObjectTemplate from current objects
        myTemplate = CurrentFHEM.GenerateTemplate("Heizung mit Vorlauf", "02_OG_Bad_Heizung_Timer_DOIF", "HM_123456_Clima")
        Assert.IsNotNull(myTemplate)
        Assert.IsTrue(myTemplate.ObjectTemplates.Count > 0)

        'Add the Filters to the ObjectTemplate
        Dim myObjectTemplate = myTemplate.ObjectTemplates.FirstOrDefault(Function(x) x.FHEMType = "DOIF")
        If myObjectTemplate IsNot Nothing Then myTemplate.Filters.ToList.ForEach(Sub(x) myObjectTemplate.Filters.Add(x.Name))

        'Write the Template to disk
        Dim myTemplateFileName As String = String.Format("{0}\TestFiles\Templates\FHEMTemplate_Heizung_mit_Vorlauf.xml", My.Application.Info.DirectoryPath)
        Helper.WriteToXML(myFHEMTemplateManager, myTemplateFileName, FileMode.Create)


        'Create AllTemplateObjects
        Dim myTemplateObjs = CurrentFHEM.CreateAllTemplateObjects(myTemplate)
        Assert.IsNotNull(myTemplateObjs)
        Assert.IsTrue(myTemplateObjs.Count > 0)

        Dim myTemplateObjsFileNameNew As String = String.Format("{0}\TestFiles\Templates\FHEMTemplate_Heizung_mit_Vorlauf_New.cfg", My.Application.Info.DirectoryPath)
        If IO.File.Exists(myTemplateObjsFileNameNew) Then IO.File.Delete(myTemplateObjsFileNameNew)
        Dim myTemplateObjsFileNameOrg As String = String.Format("{0}\TestFiles\Templates\FHEMTemplate_Heizung_mit_Vorlauf_Org.cfg", My.Application.Info.DirectoryPath)
        If IO.File.Exists(myTemplateObjsFileNameOrg) Then IO.File.Delete(myTemplateObjsFileNameOrg)
        myTemplateObjs.ToList.ForEach(Sub(thisNewObj)
                                          Helper.WriteToFile(thisNewObj.GetCfgDefineString, myTemplateObjsFileNameNew, FileMode.Append)
                                          Helper.WriteToFile(String.Empty, myTemplateObjsFileNameNew, FileMode.Append)
                                          Dim myOldObj = CurrentFHEM.CurrentObjects.FirstOrDefault(Function(x) x.Name = thisNewObj.Name)
                                          If myOldObj IsNot Nothing Then
                                              Helper.WriteToFile(myOldObj.GetCfgDefineString, myTemplateObjsFileNameOrg, FileMode.Append)
                                              Helper.WriteToFile(String.Empty, myTemplateObjsFileNameOrg, FileMode.Append)
                                          End If

                                      End Sub)
        CurrentFHEM.CurrentTemplates.Remove(myTemplate)

        If myOrgTemplate IsNot Nothing Then CurrentFHEM.CurrentTemplates.Add(myOrgTemplate)
    End Sub
    <TestMethod> Public Sub TestGenerateObjectTemplateHeizungOhneVorlauf()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)
        Assert.IsNotNull(CurrentFHEM.CurrentObjects)
        Assert.IsTrue(CurrentFHEM.CurrentObjects.Count > 0)

        Dim myOrgTemplate As FHEMTemplate = CurrentFHEM.GetTemplate("Heizung")
        If myOrgTemplate IsNot Nothing Then CurrentFHEM.CurrentTemplates.Remove(myOrgTemplate)

        'Create new Template and add to CurrentFHEM
        Dim myTemplate As FHEMTemplate = Nothing
        Dim myFHEMTemplateManager As New FHEMTemplateManager
        myTemplate = New FHEMTemplate With {.Name = "Heizung ohne Vorlauf"}
        CurrentFHEM.CurrentTemplates.Add(myTemplate)

        myFHEMTemplateManager.Templates.Add(myTemplate)

        'Add FilterObjects
        myTemplate.Filters.Add(CreateTestObjectFilter("enO_MD15_12345678"))
        myTemplate.Filters.Add(CreateTestObjectFilter("enO_MD15_12345679"))
        myTemplate.Filters.Add(CreateTestObjectFilter("enO_MD15_12345680"))

        'Create the ObjectTemplate from current objects
        myTemplate = CurrentFHEM.GenerateTemplate("Heizung ohne Vorlauf", "02_OG_Arbeit_Heizung_Timer_DOIF", "enO_MD15_12345678")
        Assert.IsNotNull(myTemplate)
        Assert.IsTrue(myTemplate.ObjectTemplates.Count > 0)

        'Add the Filters to the ObjectTemplate
        Dim myObjectTemplate = myTemplate.ObjectTemplates.FirstOrDefault(Function(x) x.FHEMType = "DOIF")
        If myObjectTemplate IsNot Nothing Then myTemplate.Filters.ToList.ForEach(Sub(x) myObjectTemplate.Filters.Add(x.Name))

        'Write the Template to disk
        Dim myTemplateFileName As String = String.Format("{0}\TestFiles\Templates\FHEMTemplate_Heizung_ohne_Vorlauf.xml", My.Application.Info.DirectoryPath)
        Helper.WriteToXML(myFHEMTemplateManager, myTemplateFileName, FileMode.Create)

        'Create AllTemplateObjects
        Dim myTemplateObjs = CurrentFHEM.CreateAllTemplateObjects(myTemplate)
        Assert.IsNotNull(myTemplateObjs)
        Assert.IsTrue(myTemplateObjs.Count > 0)

        Dim myTemplateObjsFileNameNew As String = String.Format("{0}\TestFiles\Templates\FHEMTemplate_Heizung_ohne_Vorlauf_New.cfg", My.Application.Info.DirectoryPath)
        If IO.File.Exists(myTemplateObjsFileNameNew) Then IO.File.Delete(myTemplateObjsFileNameNew)
        Dim myTemplateObjsFileNameOrg As String = String.Format("{0}\TestFiles\Templates\FHEMTemplate_Heizung_ohne_Vorlauf_Org.cfg", My.Application.Info.DirectoryPath)
        If IO.File.Exists(myTemplateObjsFileNameOrg) Then IO.File.Delete(myTemplateObjsFileNameOrg)
        myTemplateObjs.ToList.ForEach(Sub(thisNewObj)
                                          Helper.WriteToFile(thisNewObj.GetCfgDefineString, myTemplateObjsFileNameNew, FileMode.Append)
                                          Helper.WriteToFile(String.Empty, myTemplateObjsFileNameNew, FileMode.Append)
                                          Dim myOldObj = CurrentFHEM.CurrentObjects.FirstOrDefault(Function(x) x.Name = thisNewObj.Name)
                                          If myOldObj IsNot Nothing Then
                                              Helper.WriteToFile(myOldObj.GetCfgDefineString, myTemplateObjsFileNameOrg, FileMode.Append)
                                              Helper.WriteToFile(String.Empty, myTemplateObjsFileNameOrg, FileMode.Append)
                                          End If

                                      End Sub)

        CurrentFHEM.CurrentTemplates.Remove(myTemplate)

        If myOrgTemplate IsNot Nothing Then CurrentFHEM.CurrentTemplates.Add(myOrgTemplate)
    End Sub
    <TestMethod> Public Sub TestGenerateObjectTemplateCreateUpdateCommand()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)
        Assert.IsNotNull(CurrentFHEM.CurrentObjects)
        Assert.IsTrue(CurrentFHEM.CurrentObjects.Count > 0)

        Dim myOrgTemplate As FHEMTemplate = CurrentFHEM.GetTemplate("Heizung")
        If myOrgTemplate IsNot Nothing Then CurrentFHEM.CurrentTemplates.Remove(myOrgTemplate)

        'Create new Template and TemplateManager. Add Template to Manager
        Dim myTemplate As FHEMTemplate = Nothing
        Dim myFHEMTemplateManager As New FHEMTemplateManager

        myTemplate = CurrentFHEM.GenerateTemplate("Heizung", "02_OG_Bad_Heizung_Timer_DOIF", "HM_123456_Clima")
        Assert.IsNotNull(myTemplate)
        Assert.IsTrue(myTemplate.ObjectTemplates.Count > 0)

        myFHEMTemplateManager.Templates.Add(myTemplate)

        Dim myNewTemplateFileName As String = String.Format("{0}\TestFiles\Templates\FHEMGenerateObjectTemplateCreateUpdateCommand.xml", My.Application.Info.DirectoryPath)
        Helper.WriteToXML(myTemplate, myNewTemplateFileName, FileMode.Create)

        Dim myTestObj As FHEMObj = CurrentFHEM.CurrentObjects.FirstOrDefault(Function(x) x.Name = "HM_123455_Clima")
        Dim myNewObjects = FHEMKernel.TemplateCreateObjects(myTemplate, New ObjectFilter, myTemplate.ObjectTemplates(0), myTestObj)
        Assert.IsNotNull(myNewObjects)
        Assert.IsTrue(myNewObjects.Count > 0)

        Dim myNewObjectsFileName As String = String.Format("{0}\TestFiles\Templates\FHEMGenerateObjectTemplateCreateUpdateCommand.cfg", My.Application.Info.DirectoryPath)
        If IO.File.Exists(myNewObjectsFileName) Then IO.File.Delete(myNewObjectsFileName)

        myNewObjects.ToList.ForEach(Sub(thisNewObject)
                                        Helper.WriteToFile(CurrentFHEM.GetTemplateUpdateCommand(thisNewObject, FHEMKernel.TemplateUpdateCommandCreationFlags.CreateModifyExisting Or FHEMKernel.TemplateUpdateCommandCreationFlags.CreateAttributsUpdatesValue), myNewObjectsFileName, FileMode.Append)
                                        Helper.WriteToFile(String.Empty, myNewObjectsFileName, FileMode.Append)
                                    End Sub)

        If myOrgTemplate IsNot Nothing Then CurrentFHEM.CurrentTemplates.Add(myOrgTemplate)
    End Sub

    Private Sub AssertAndDebugPrintObjectFilterGetObjects(thisTemplate As FHEMTemplate, thisFilter As ObjectFilter, thisObjectTemplate As ObjectTemplate, thisFHEMObj As FHEMObj)

        Dim myNewFHEMObjs = FHEMKernel.TemplateCreateObjects(thisTemplate, thisFilter, thisObjectTemplate, thisFHEMObj)
        Assert.IsNotNull(myNewFHEMObjs)
        Assert.IsTrue(myNewFHEMObjs.Count > 0)
        myNewFHEMObjs.ToList.ForEach(Sub(thisNewFHEMObj)
                                         Assert.IsFalse(String.IsNullOrEmpty(thisNewFHEMObj.Name))
                                         Assert.IsFalse(String.IsNullOrEmpty(thisNewFHEMObj.FHEMType))
                                         Assert.IsNotNull(thisNewFHEMObj.Attributes)
                                         Assert.IsTrue(thisNewFHEMObj.Attributes.Count > 0)
                                         Debug.WriteLine(thisNewFHEMObj.GetCfgDefineString)
                                         Debug.WriteLine(Nothing)
                                     End Sub)
    End Sub
    Private Sub AssertAndDebugPrintFHEMTemplate(thisTemplate As FHEMTemplate)
        Assert.IsFalse(String.IsNullOrEmpty(thisTemplate.ID))
        Assert.IsFalse(String.IsNullOrEmpty(thisTemplate.Name))
        Debug.WriteLine("FHEMTemplate .ID:'{0}' .Name:'{1}'", New Object() {thisTemplate.ID, thisTemplate.Name})
        Assert.IsNotNull(thisTemplate.Filters)
        Assert.IsNotNull(thisTemplate.ObjectTemplates)
        Debug.Indent()
        thisTemplate.ObjectTemplates.ToList.ForEach(Sub(y) AssertAndDebugPrintObjectTemplate(y))
        Debug.Unindent()
        Debug.WriteLine(Nothing)
    End Sub
    Private Sub AssertAndDebugPrintObjectTemplate(thisObjectTemplate As ObjectTemplate)
        Assert.IsFalse(String.IsNullOrEmpty(thisObjectTemplate.FHEMType))
        Assert.IsNotNull(thisObjectTemplate.Filters)
        Assert.IsNotNull(thisObjectTemplate.Name)
        Assert.IsNotNull(thisObjectTemplate.Definition)
        Debug.WriteLine("ObjectTemplate .FHEMType:'{0}' .FilterName:'{1}'", New Object() {thisObjectTemplate.FHEMType, String.Join(",", thisObjectTemplate.Filters.ToArray)})
    End Sub
    Private Sub AssertAndDebugPrintObjectFilter(thisObjectFilter As ObjectFilter)
        Assert.IsFalse(String.IsNullOrEmpty(thisObjectFilter.Name))
        Debug.WriteLine("FHEMFilter .Name:'{0}'", New Object() {thisObjectFilter.Name})

        Assert.IsFalse(String.IsNullOrEmpty(thisObjectFilter.FHEMName) AndAlso String.IsNullOrEmpty(thisObjectFilter.FHEMType) AndAlso thisObjectFilter.Attributes.Count = 0)
        Debug.Indent()
        If Not String.IsNullOrEmpty(thisObjectFilter.FHEMName) Then
            Debug.WriteLine(".FHEMName.Contains(""{0}"")", New Object() {thisObjectFilter.FHEMName})
        End If
        Debug.Unindent()

        Debug.Indent()
        If Not String.IsNullOrEmpty(thisObjectFilter.FHEMType) Then
            Debug.WriteLine(".FHEMType.Equals(""{0}"")", New Object() {thisObjectFilter.FHEMType})
        End If
        Debug.Unindent()

        Debug.Indent()
        thisObjectFilter.Attributes.ToList.ForEach(Sub(z) AssertAndDebugPrintAttributes(z))
        Debug.Unindent()
    End Sub
    Private Sub AssertAndDebugPrintAttributes(thisAttribute As KeyValueObject)
        Assert.IsFalse(String.IsNullOrEmpty(thisAttribute.Key))
        Assert.IsFalse(String.IsNullOrEmpty(thisAttribute.Value.ToString))
        Debug.WriteLine(".Attributes.ContainsKey(""{0}"").Equals(""{1}"")", New Object() {thisAttribute.Key, thisAttribute.Value.ToString})
    End Sub
    Private Function CreateTestObjectFilter(thisOrgName) As ObjectFilter
        Dim myObj As FHEMObj = CurrentFHEM.CurrentObjects.FirstOrDefault(Function(x) x.Name = thisOrgName)
        If myObj IsNot Nothing Then
            Dim myFilter As ObjectFilter = New ObjectFilter With {.Name = myObj.AliasName, .FHEMName = myObj.Name, .FHEMType = myObj.FHEMType}
            myFilter.Attributes.Add(New KeyValueObject With {.Key = "room", .Value = New StringObject With {.Value = DirectCast(myObj.Attributes("room"), List(Of String))(1)}})
            Return myFilter
        End If
        Return Nothing
    End Function
End Class
