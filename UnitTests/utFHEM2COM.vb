Imports MM.FHEMManager.FHEMObjects
Imports MM.FHEMManager.FHEMTemplates
Imports MM.FHEMManager.FHEM2COM
<TestClass()> Public Class UnitTestFHEM2COM
    Private Shared CurrentFHEM As FHEMCOM
    Private Shared SyncObj As New Object()
    Public Sub Initialize()
        If CurrentFHEM Is Nothing Then
            SyncLock SyncObj
                If CurrentFHEM Is Nothing Then CurrentFHEM = GetFhem()
            End SyncLock
        End If
    End Sub
    Private Function GetFhem() As FHEMCOM
        Dim myReturn As FHEMCOM
        myReturn = New FHEMCOM

        Dim myCfgFileName As String = String.Format("{0}\TestFiles\fhem.cfg", My.Application.Info.DirectoryPath)
        Assert.IsTrue(IO.File.Exists(myCfgFileName))
        Assert.IsTrue(myReturn.LoadCfg(myCfgFileName))
        Assert.IsTrue(myReturn.ObjectCount > 0)

        Dim myTemplateFileName As String = String.Format("{0}\TestFiles\Templates\FHEMTemplate.xml", My.Application.Info.DirectoryPath)
        Assert.IsTrue(IO.File.Exists(myTemplateFileName))
        Assert.IsTrue(myReturn.LoadTemplates(myTemplateFileName))
        Assert.IsTrue(myReturn.TemplateCount > 0)

        Return myReturn
    End Function
    <TestMethod()> Public Sub TestFhem()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)
        Assert.IsInstanceOfType(CurrentFHEM, GetType(FHEMCOM))
        Assert.IsTrue(CurrentFHEM.ObjectCount > 0)
        Assert.IsTrue(CurrentFHEM.TemplateCount > 0)
    End Sub

    <TestMethod> Public Sub TestGetTypes()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)

        Dim myTest As String() = Nothing
        myTest = CurrentFHEM.GetTypes
        Assert.IsNotNull(myTest)
        Assert.IsInstanceOfType(myTest, GetType(String()))
        Assert.IsTrue(myTest.Length > 0)
    End Sub

    <TestMethod> Public Sub TestGetByType()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)

        Dim myTest As FHEMObj() = Nothing
        myTest = CurrentFHEM.GetObjectsByType("dummy")
        Assert.IsNotNull(myTest)
        Assert.IsTrue(myTest.Length > 0)
    End Sub

    <TestMethod> Public Sub TestGetObjByName()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)

        Dim myTest As FHEMObj = Nothing
        myTest = CurrentFHEM.GetObjByName("global")
        Assert.IsNotNull(myTest)
        Assert.IsInstanceOfType(myTest, GetType(FHEMObj))
        Assert.IsTrue(myTest.Name.Equals("global"))
    End Sub

    <TestMethod> Public Sub TestGetObjAttributes()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)

        Dim myObj As FHEMObj = Nothing
        myObj = CurrentFHEM.GetObjByName("global")
        Assert.IsNotNull(myObj)

        Dim myTest As String() = Nothing
        myTest = CurrentFHEM.GetObjAttributes("global")
        Assert.IsNotNull(myTest)
        Assert.IsInstanceOfType(myTest, GetType(String()))
        Assert.IsTrue(myTest.Length > 0)
        Assert.IsTrue(myTest(0).Length > 0)

        myTest.ToList.ForEach(Sub(x) Debug.WriteLine(x))
        Debug.WriteLine(Nothing)
    End Sub
    <TestMethod> Public Sub TestGetObjAttribute()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)

        Dim myObj As FHEMObj = Nothing
        myObj = CurrentFHEM.GetObjByName("global")
        Assert.IsNotNull(myObj)

        Dim myTest As String() = Nothing
        myTest = CurrentFHEM.GetObjAttribute("global", "verbose")
        Assert.IsNotNull(myTest)
        Assert.IsInstanceOfType(myTest, GetType(String()))
        Assert.IsTrue(myTest.Length > 0)
        Assert.IsTrue(myTest(0).Length > 0)
    End Sub

    <TestMethod> Public Sub TestGetDoIfCheckItems_1() '00_KG_Gang_NAS01_Watch_DoIf
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)

        Dim myTestString As String = Nothing

        Dim myObj As FHEMObj = Nothing
        myObj = CurrentFHEM.GetObjByName("00_KG_Gang_NAS01_Watch_DoIf")
        Assert.IsNotNull(myObj)

        myTestString = myObj.Definition
        Assert.IsFalse(String.IsNullOrEmpty(myTestString))

        Dim myTest As String() = Nothing
        myTest = CurrentFHEM.GetDoIfCheckItems(myTestString)
        Assert.IsNotNull(myTest)
        Assert.IsInstanceOfType(myTest, GetType(String()))
        Assert.IsTrue(myTest.Length > 0)
        Assert.IsTrue(myTest(0).Length > 0)
    End Sub
    <TestMethod> Public Sub TestGetDoIfCheckItems_2() '00_KG_Schlaf_GBF_2_Control_2_DoIf
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)

        Dim myTestString As String = Nothing

        Dim myObj As FHEMObj = Nothing
        myObj = CurrentFHEM.GetObjByName("00_KG_Schlaf_GBF_2_Control_2_DoIf")
        Assert.IsNotNull(myObj)

        myTestString = myObj.Definition
        Assert.IsFalse(String.IsNullOrEmpty(myTestString))

        Dim myTest As String() = Nothing
        myTest = CurrentFHEM.GetDoIfCheckItems(myTestString)
        Assert.IsNotNull(myTest)
        Assert.IsInstanceOfType(myTest, GetType(String()))
        Assert.IsTrue(myTest.Length > 0)
        Assert.IsTrue(myTest(0).Length > 0)
    End Sub
    <TestMethod> Public Sub TestGetDoIfCheckItems_3() '02_OG_Wohn_Heizung_Timer_DoIf
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)

        Dim myTestString As String = Nothing

        Dim myObj As FHEMObj = Nothing
        myObj = CurrentFHEM.GetObjByName("02_OG_Wohn_Heizung_Timer_DoIf")
        Assert.IsNotNull(myObj)

        myTestString = myObj.Definition
        Assert.IsFalse(String.IsNullOrEmpty(myTestString))

        Dim myTest As String() = Nothing
        myTest = CurrentFHEM.GetDoIfCheckItems(myTestString)
        Assert.IsNotNull(myTest)
        Assert.IsInstanceOfType(myTest, GetType(String()))
        Assert.IsTrue(myTest.Length > 0)
        Assert.IsTrue(myTest(0).Length > 0)
    End Sub
    <TestMethod> Public Sub TestGetDoIfTrigger() '02_OG_Wohn_Heizung_Timer_DoIf
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)

        Dim myTestString As String = Nothing

        Dim myObj As FHEMObj = Nothing
        myObj = CurrentFHEM.GetObjByName("02_OG_Wohn_Heizung_Timer_DoIf")
        Assert.IsNotNull(myObj)

        myTestString = myObj.Definition
        Assert.IsFalse(String.IsNullOrEmpty(myTestString))

        Dim myTest As String = Nothing
        myTest = CurrentFHEM.GetDoIfTrigger(myTestString)
        Assert.IsNotNull(myTest)
        Assert.IsInstanceOfType(myTest, GetType(String))
        Assert.IsTrue(myTest.Length > 0)
    End Sub
    <TestMethod> Public Sub TestGetNotifyTrigger() '00_KG_Gang_NAS01_Notify
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)

        Dim myTestString As String = Nothing

        Dim myObj As FHEMObj = Nothing
        myObj = CurrentFHEM.GetObjByName("00_KG_Gang_NAS01_Notify")
        Assert.IsNotNull(myObj)

        myTestString = myObj.Definition
        Assert.IsFalse(String.IsNullOrEmpty(myTestString))

        Dim myTest As String = Nothing
        myTest = CurrentFHEM.GetNotifyTrigger(myTestString)
        Assert.IsNotNull(myTest)
        Assert.IsInstanceOfType(myTest, GetType(String))
        Assert.IsTrue(myTest.Length > 0)
    End Sub
    <TestMethod> Public Sub TestGetNotifyActorItems() '00_KG_Gang_NAS01_Notify
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)

        Dim myTestString As String = Nothing

        Dim myObj As FHEMObj = Nothing
        myObj = CurrentFHEM.GetObjByName("00_KG_Gang_NAS01_Notify")
        Assert.IsNotNull(myObj)

        myTestString = myObj.Definition
        Assert.IsFalse(String.IsNullOrEmpty(myTestString))

        Dim myTest As String() = Nothing
        myTest = CurrentFHEM.GetNotifyActorItems(myTestString)
        Assert.IsNotNull(myTest)
        Assert.IsInstanceOfType(myTest, GetType(String()))
        Assert.IsTrue(myTest.Length > 0)
        Assert.IsTrue(myTest(0).Length > 0)
    End Sub
    <TestMethod> Public Sub TestGetNotifyCheckItems() '00_KG_Heiz_Heizkessel_Notify
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)

        Dim myTestString As String = Nothing

        Dim myObj As FHEMObj = Nothing
        myObj = CurrentFHEM.GetObjByName("00_KG_Heiz_Heizkessel_Notify")
        Assert.IsNotNull(myObj)

        myTestString = myObj.Definition
        Assert.IsFalse(String.IsNullOrEmpty(myTestString))

        Dim myTest As String() = Nothing
        myTest = CurrentFHEM.GetNotifyCheckItems(myTestString)
        Assert.IsNotNull(myTest)
        Assert.IsInstanceOfType(myTest, GetType(String()))
        Assert.IsTrue(myTest.Length > 0)
        Assert.IsTrue(myTest(0).Length > 0)
    End Sub

    <TestMethod> Public Sub TestGetTemplates()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)

        SyncLock (SyncObj)
            Dim myTest As String() = Nothing
            myTest = CurrentFHEM.GetTemplateNames
            Assert.IsNotNull(myTest)
            Assert.IsInstanceOfType(myTest, GetType(String()))
            Assert.IsTrue(myTest.Length > 0)
        End SyncLock
    End Sub
    <TestMethod> Public Sub TestGetTemplatesFault()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)

        SyncLock (SyncObj)
            Dim myTemplateFileName As String = String.Format("{0}\TestFiles\Templates\FHEMTemplateFault.xml", My.Application.Info.DirectoryPath)
            Assert.IsTrue(IO.File.Exists(myTemplateFileName))
            Assert.IsFalse(CurrentFHEM.LoadTemplates(myTemplateFileName))
            Assert.IsTrue(CurrentFHEM.TemplateCount = 0)

            Dim mySerializationEvents = CurrentFHEM.GetSerializationEvents
            Assert.IsNotNull(mySerializationEvents)
            Assert.IsTrue(mySerializationEvents.Count > 0)
            mySerializationEvents.ToList.ForEach(Sub(x) Debug.WriteLine(x))

            myTemplateFileName = String.Format("{0}\TestFiles\Templates\FHEMTemplate.xml", My.Application.Info.DirectoryPath)
            Assert.IsTrue(IO.File.Exists(myTemplateFileName))
            Assert.IsTrue(CurrentFHEM.LoadTemplates(myTemplateFileName))
            Assert.IsTrue(CurrentFHEM.TemplateCount > 0)
        End SyncLock
    End Sub

    <TestMethod> Public Sub TestGetObjectsByFilter()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)

        SyncLock (SyncObj)
            Dim myTemplates As String() = Nothing
            myTemplates = CurrentFHEM.GetTemplateNames
            Assert.IsNotNull(myTemplates)
            Assert.IsInstanceOfType(myTemplates, GetType(String()))
            Assert.IsTrue(myTemplates.Length > 0)

            Dim myTemplate As FHEMTemplate = CurrentFHEM.GetTemplate(myTemplates(0))
            Assert.IsNotNull(myTemplate)
            Assert.IsNotNull(myTemplate.Filters)
            Assert.IsNotNull(myTemplate.ObjectTemplates)

            Dim myFilter As ObjectFilter = myTemplate.Filters(0)
            Assert.IsNotNull(myFilter)

            Dim myObjects As FHEMObj() = CurrentFHEM.GetObjectsByObjectFilter(myFilter)
            Assert.IsNotNull(myObjects)
            Assert.IsTrue(myObjects.Length > 0)
        End SyncLock
    End Sub
    <TestMethod> Public Sub TestObjectTemplate()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)

        SyncLock (SyncObj)
            Dim myTemplates As String() = Nothing
            myTemplates = CurrentFHEM.GetTemplateNames
            Assert.IsNotNull(myTemplates)
            Assert.IsInstanceOfType(myTemplates, GetType(String()))
            Assert.IsTrue(myTemplates.Length > 0)

            Dim myTemplate As FHEMTemplate = CurrentFHEM.GetTemplate(myTemplates(0))
            Assert.IsNotNull(myTemplate)
            Assert.IsNotNull(myTemplate.Filters)
            Assert.IsNotNull(myTemplate.ObjectTemplates)

            Dim myFilter As ObjectFilter = myTemplate.Filters(0)
            Assert.IsNotNull(myFilter)

            Dim myObjects As FHEMObj() = CurrentFHEM.GetObjectsByObjectFilter(myFilter)
            Assert.IsNotNull(myObjects)
            Assert.IsTrue(myObjects.Length > 0)

            Dim myObject As FHEMObj = myObjects.First
            Assert.IsNotNull(myObject)

            Dim myObjectTemplate = myTemplate.ObjectTemplates.FirstOrDefault(Function(x) x.Filters.Contains(myFilter.Name))
            Assert.IsNotNull(myObjectTemplate)

            myObjectTemplate.Init(myTemplate, myFilter.Name, myObject)

            Assert.IsFalse(String.IsNullOrEmpty(myObjectTemplate.Name.ToString))
            Debug.WriteLine("Name:") : Debug.WriteLine("{0}", New Object() {myObjectTemplate.Name.ToString}) : Debug.WriteLine(Nothing)

            Assert.IsFalse(String.IsNullOrEmpty(myObjectTemplate.Definition.ToString))
            Debug.WriteLine("Definition:") : Debug.WriteLine("{0}", New Object() {myObjectTemplate.Definition.ToString}) : Debug.WriteLine(Nothing)

            Assert.IsNotNull(myObjectTemplate.Attributes)
            Assert.IsTrue(myObjectTemplate.Attributes.Count > 0)
            Debug.WriteLine("Attributes: {0}", New Object() {myObjectTemplate.Attributes.Count})
            Debug.Indent()
            myObjectTemplate.Attributes.ToList.ForEach(Sub(x) Debug.WriteLine("{0}:{1}", New Object() {x.Key, x.Value.ToString}))
            Debug.Unindent()
            Debug.WriteLine(Nothing)

            Dim myCreatedObjects = myObjectTemplate.GetCreatedFHEMObjs.ToList
            Assert.IsNotNull(myCreatedObjects)
            Assert.IsTrue(myCreatedObjects.Count > 0)
            myCreatedObjects.ForEach(Sub(x) Debug.WriteLine(x.GetCfgDefineString()))
            Debug.WriteLine(Nothing)
        End SyncLock

    End Sub

    <Ignore> Public Sub TestPrettyText()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)

        Dim myTestString As String = Nothing
        Dim myTest As String = Nothing

        Dim myObj As FHEMObj = Nothing
        myObj = CurrentFHEM.GetObjByName("02_OG_Schlaf_Heizung_Timer_DoIf")
        Assert.IsNotNull(myObj)

        myTestString = myObj.Definition
        Assert.IsFalse(String.IsNullOrEmpty(myTestString))

        Dim myint As New PrivateType(GetType(FHEM2COM.FHEMCOM))
        myTest = myint.InvokeStatic("GetIndentedText", New Object() {myTestString})
        'myTest = FHEM.GetIndentedText(myTestString)
        Assert.IsFalse(String.IsNullOrEmpty(myTest))
        Debug.WriteLine(myTest)

        myObj = CurrentFHEM.GetObjByName("02_OG_Schlaf_Heizung_Notify")
        Assert.IsNotNull(myObj)

        myTestString = myObj.Definition
        Assert.IsFalse(String.IsNullOrEmpty(myTestString))

        myTest = myint.InvokeStatic("GetIndentedText", New Object() {myTestString})
        'myTest = FHEM.GetIndentedText(myTestString)
        Assert.IsFalse(String.IsNullOrEmpty(myTest))
        Debug.WriteLine(myTest)
    End Sub
End Class