Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.Win32

' A creatable COM class must have a Public Sub New() 
' with no parameters, otherwise, the class will not be 
' registered in the COM registry and cannot be created 
' via CreateObject.
<Assembly: InternalsVisibleTo("FHEM2COM")>
<ComVisible(False)> Friend Class COMHelper
    Public Sub New()
        MyBase.New
    End Sub
    Public Shared Sub RegisterFunction(ByVal type As Type)
        Registry.ClassesRoot.CreateSubKey(GetSubkeyName(type, "Programmable"))

        Dim key = Registry.ClassesRoot.OpenSubKey(GetSubkeyName(type, "InprocServer32"), True)
        key.SetValue("", System.Environment.SystemDirectory + "\mscoree.dll", RegistryValueKind.String)
    End Sub

    Public Shared Sub UnregisterFunction(ByVal type As Type)
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

Namespace FHEMObjects
    <CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId:="FHEM")>
    <ClassInterface(ClassInterfaceType.AutoDual), ComVisible(True)> Partial Class FHEMObj
        Public Sub New()
            MyBase.New()
        End Sub

#Region "COM-Functions"
        <ComRegisterFunction()> Private Shared Sub RegisterFunction(ByVal type As Type)
            COMHelper.RegisterFunction(type)
        End Sub
        <ComUnregisterFunction()> Private Shared Sub UnregisterFunction(ByVal type As Type)
            COMHelper.UnregisterFunction(type)
        End Sub
#End Region
    End Class

    <CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId:="FHEM")>
    <ClassInterface(ClassInterfaceType.AutoDual), ComVisible(False)> Partial Class FHEMObjCollection
        Public Sub New()
            MyBase.New()
        End Sub

#Region "COM-Functions"
        <ComRegisterFunction()> Private Shared Sub RegisterFunction(ByVal type As Type)
            COMHelper.RegisterFunction(type)
        End Sub
        <ComUnregisterFunction()> Private Shared Sub UnregisterFunction(ByVal type As Type)
            COMHelper.UnregisterFunction(type)
        End Sub
#End Region
    End Class

    '    <ClassInterface(ClassInterfaceType.AutoDual), ComVisible(False)> Partial Class FHEMAttributesList
    '        Public Sub New()
    '            MyBase.New()
    '        End Sub

    '#Region "COM-Functions"
    '        <ComRegisterFunction()> Private Shared Sub RegisterFunction(ByVal type As Type)
    '            COMHelper.RegisterFunction(type)
    '        End Sub
    '        <ComUnregisterFunction()> Private Shared Sub UnregisterFunction(ByVal type As Type)
    '            COMHelper.UnregisterFunction(type)
    '        End Sub
    '#End Region
    '    End Class

    <CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId:="FHEM")>
    <ClassInterface(ClassInterfaceType.AutoDual), ComVisible(True)> Partial Class FHEMLogEntry
        Public Sub New()
            MyBase.New()
        End Sub

#Region "COM-Functions"
        <ComRegisterFunction()> Private Shared Sub RegisterFunction(ByVal type As Type)
            COMHelper.RegisterFunction(type)
        End Sub
        <ComUnregisterFunction()> Private Shared Sub UnregisterFunction(ByVal type As Type)
            COMHelper.UnregisterFunction(type)
        End Sub
#End Region
    End Class
End Namespace

Namespace FHEMTemplates
    <CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId:="FHEM")>
    <ClassInterface(ClassInterfaceType.AutoDual), ComVisible(True)> Partial Class FHEMTemplateManager
        Public Sub New()
            MyBase.New()
        End Sub

#Region "COM-Functions"
        <ComRegisterFunction()> Private Shared Sub RegisterFunction(ByVal type As Type)
            COMHelper.RegisterFunction(type)
        End Sub
        <ComUnregisterFunction()> Private Shared Sub UnregisterFunction(ByVal type As Type)
            COMHelper.UnregisterFunction(type)
        End Sub
#End Region
    End Class
    <ClassInterface(ClassInterfaceType.AutoDual), ComVisible(True)> Partial MustInherit Class TemplateBase
        Protected Sub New()
            MyBase.New()
        End Sub

#Region "COM-Functions"
        <ComRegisterFunction()> Private Shared Sub RegisterFunction(ByVal type As Type)
            COMHelper.RegisterFunction(type)
        End Sub
        <ComUnregisterFunction()> Private Shared Sub UnregisterFunction(ByVal type As Type)
            COMHelper.UnregisterFunction(type)
        End Sub

#End Region
    End Class
    <ClassInterface(ClassInterfaceType.AutoDual), ComVisible(True)> Partial Class FHEMTemplate
        Public Sub New()
            MyBase.New()
        End Sub

#Region "COM-Functions"
        <ComRegisterFunction()> Private Shared Sub RegisterFunction(ByVal type As Type)
            COMHelper.RegisterFunction(type)
        End Sub
        <ComUnregisterFunction()> Private Shared Sub UnregisterFunction(ByVal type As Type)
            COMHelper.UnregisterFunction(type)
        End Sub
#End Region
    End Class
    <ClassInterface(ClassInterfaceType.AutoDual), ComVisible(True)> Partial Class ObjectTemplate
        Public Sub New()
            MyBase.New()
        End Sub

#Region "COM-Functions"
        <ComRegisterFunction()> Private Shared Sub RegisterFunction(ByVal type As Type)
            COMHelper.RegisterFunction(type)
        End Sub
        <ComUnregisterFunction()> Private Shared Sub UnregisterFunction(ByVal type As Type)
            COMHelper.UnregisterFunction(type)
        End Sub
#End Region
    End Class
    <ClassInterface(ClassInterfaceType.AutoDual), ComVisible(True)> Partial Class StringReplaceObjectBase
        Protected Sub New()
            MyBase.New()
        End Sub

#Region "COM-Functions"
        <ComRegisterFunction()> Private Shared Sub RegisterFunction(ByVal type As Type)
            COMHelper.RegisterFunction(type)
        End Sub
        <ComUnregisterFunction()> Private Shared Sub UnregisterFunction(ByVal type As Type)
            COMHelper.UnregisterFunction(type)
        End Sub
#End Region
    End Class
    <ClassInterface(ClassInterfaceType.AutoDual), ComVisible(True)> Partial Class StringFormatObject
        Public Sub New()
            MyBase.New()
        End Sub

#Region "COM-Functions"
        <ComRegisterFunction()> Private Shared Sub RegisterFunction(ByVal type As Type)
            COMHelper.RegisterFunction(type)
        End Sub
        <ComUnregisterFunction()> Private Shared Sub UnregisterFunction(ByVal type As Type)
            COMHelper.UnregisterFunction(type)
        End Sub
#End Region
    End Class
    <ClassInterface(ClassInterfaceType.AutoDual), ComVisible(True)> Partial Class ReferenceObject
        Public Sub New()
            MyBase.New()
        End Sub

#Region "COM-Functions"
        <ComRegisterFunction()> Private Shared Sub RegisterFunction(ByVal type As Type)
            COMHelper.RegisterFunction(type)
        End Sub
        <ComUnregisterFunction()> Private Shared Sub UnregisterFunction(ByVal type As Type)
            COMHelper.UnregisterFunction(type)
        End Sub
#End Region
    End Class
    <ClassInterface(ClassInterfaceType.AutoDual), ComVisible(True)> Partial Class ObjectFilter
        Public Sub New()
            MyBase.New()
        End Sub
#Region "COM-Functions"
        <ComRegisterFunction()> Private Shared Sub RegisterFunction(ByVal type As Type)
            COMHelper.RegisterFunction(type)
        End Sub
        <ComUnregisterFunction()> Private Shared Sub UnregisterFunction(ByVal type As Type)
            COMHelper.UnregisterFunction(type)
        End Sub
#End Region
    End Class
    <ClassInterface(ClassInterfaceType.AutoDual), ComVisible(True)> Partial Class KeyValueObject
        Public Sub New()
            MyBase.New()
        End Sub

#Region "COM-Functions"
        <ComRegisterFunction()> Private Shared Sub RegisterFunction(ByVal type As Type)
            COMHelper.RegisterFunction(type)
        End Sub
        <ComUnregisterFunction()> Private Shared Sub UnregisterFunction(ByVal type As Type)
            COMHelper.UnregisterFunction(type)
        End Sub
#End Region
    End Class
    '<ClassInterface(ClassInterfaceType.AutoDual), ComVisible(True)> Partial Class StringObject
    '    Public Sub New()
    '        MyBase.New()
    '    End Sub

    '#Region "COM-Functions"
    '    <ComRegisterFunction()> Private Shared Sub RegisterFunction(ByVal type As Type)
    '            COMHelper.RegisterFunction(type)
    '        End Sub
    '        <ComUnregisterFunction()> Private Shared Sub UnregisterFunction(ByVal type As Type)
    '            COMHelper.UnregisterFunction(type)
    '        End Sub
    '#End Region
    '    End Class

End Namespace