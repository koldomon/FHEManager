Imports System.Collections.ObjectModel
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Xml.Serialization
Imports MM.FHEMManager.FHEMObjects

Namespace FHEMTemplates
    <XmlType("StringReplaceObjectBase")> Public MustInherit Class StringReplaceObjectBase
        Inherits TemplateBase
        Public MustOverride Overrides Function ToString() As String
        Protected Friend Overridable Overloads Function Init(thisParent As StringReplaceObjectBase) As Boolean
            If Not Me.Reset() Then Return False

            If thisParent Is Nothing Then Return False

            If Not MyBase.Init(thisParent.Template,
                                thisParent.TemplateFilter,
                                thisParent.TemplateFHEMObj,
                                thisParent.CurrentObjectTemplate,
                                thisParent.CurrentFHEMObj,
                                thisParent.ParentObjectTemplate,
                                thisParent.ParentFHEMObj) Then Return False

            Return True
        End Function

        Protected Overrides Function Reset() As Boolean
            'nothing to do  
            Return True
        End Function

        Protected Shared Function GetPropertyValue(thisObject As Object, thisPropertyName As String) As Object
            If thisObject Is Nothing Then Return Nothing
            If String.IsNullOrEmpty(thisPropertyName) Then Return Nothing

            Dim myProp = GetProperty(thisObject, thisPropertyName)
            If myProp IsNot Nothing Then
                Return myProp.GetValue(thisObject)
            End If

            Return Nothing
        End Function
        Protected Shared Function GetProperty(thisObject As Object, thisPropertyName As String) As Object
            If thisObject Is Nothing Then Return Nothing
            If String.IsNullOrEmpty(thisPropertyName) Then Return Nothing

            Dim myProps = thisObject.GetType.GetProperties(BindingFlags.Public Or BindingFlags.Instance)
            Dim myProp = myProps.FirstOrDefault(Function(x) x.Name = thisPropertyName)
            If myProp IsNot Nothing Then
                Return myProp
            End If

            Return Nothing
        End Function
    End Class
    <XmlType("StringFormatObject")> Public Class StringFormatObject
        Inherits StringReplaceObjectBase

        <XmlElement("FormatString")> Public Property FormatString As String
        <ComVisible(False), XmlArray("Values"), XmlArrayItem("KeyValueObject", GetType(KeyValueObject))> Public Property Values As New Collection(Of KeyValueObject)
        <ComVisible(True)> Public Function ValuesArray() As KeyValueObject()
            Return _Values.ToArray
        End Function

        Protected Friend Overloads Function Init(thisParent As ObjectTemplate) As Boolean
            If thisParent Is Nothing Then Return False

            If Not MyBase.Init(thisParent.Template, thisParent.TemplateFilter, thisParent.TemplateFHEMObj, thisParent.CurrentObjectTemplate, thisParent.CurrentFHEMObj, thisParent.ParentObjectTemplate, thisParent.ParentFHEMObj) Then Return False
            If MyBase.Init(Nothing) Then Return False
            If Not InitSelf() Then Return False

            Return True
        End Function
        Protected Friend Overloads Function Init(thisParent As StringReplaceObjectBase) As Boolean
            If Not MyBase.Init(thisParent) Then Return False
            If Not InitSelf() Then Return False

            Return True
        End Function
        Private Function InitSelf() As Boolean
            Dim myReturn As Boolean = True

            Me.Values.ToList.ForEach(Sub(x) If Not CType(x, KeyValueObject).Init(Me) Then myReturn = False)

            Return myReturn
        End Function

        <ComVisible(False)> Public Overrides Function ValidateAndCreate() As Boolean
            Return ValidateSelf()
        End Function
        Private Function ValidateSelf() As Boolean
            Dim myReturn As Boolean = True

            If String.IsNullOrEmpty(Me.FormatString) Then Return False

            Me.Values.ToList.ForEach(Sub(x) If Not CType(x, KeyValueObject).ValidateAndCreate() Then myReturn = False)
            If myReturn = False Then Return False

            Return (Not String.IsNullOrEmpty(Me.ToString))
        End Function

        <ComVisible(True)> Public Overrides Function ToString() As String
            Dim myReturn As String = Me.FormatString

            For Each myKey In Me.Values
                myReturn = myReturn.Replace(myKey.Key, myKey.Value.ToString)
            Next

            Return myReturn
        End Function
    End Class

    <XmlType("ReferenceObject")> Public Class ReferenceObject
        Inherits StringReplaceObjectBase

        Private glbSpecifiedObject As Object = Nothing

        <XmlElement("ObjectSpecifier")> Public Property ObjectSpecifier As ObjectSpecifier

        <XmlElement("StringObject", GetType(StringObject)),
            XmlElement("KeyValueObject", GetType(KeyValueObject)),
            XmlElement("ReferenceObject", GetType(ReferenceObject))> Public Property [Property] As StringReplaceObjectBase

        Protected Friend Overloads Function Init(thisParent As ObjectTemplate) As Boolean
            If thisParent Is Nothing Then Return False

            If Not MyBase.Init(thisParent.Template, thisParent.TemplateFilter, thisParent.TemplateFHEMObj, thisParent.CurrentObjectTemplate, thisParent.CurrentFHEMObj, thisParent.ParentObjectTemplate, thisParent.ParentFHEMObj) Then Return False
            If MyBase.Init(Nothing) Then Return False
            If Not InitSelf() Then Return False

            Return True
        End Function
        Protected Friend Overloads Function Init(thisParent As StringReplaceObjectBase) As Boolean
            If Not MyBase.Init(thisParent) Then Return False
            If Not InitSelf() Then Return False

            Return True
        End Function
        Private Function InitSelf() As Boolean
            Select Case Me.ObjectSpecifier
                Case ObjectSpecifier.CurrentObjectTemplate
                    glbSpecifiedObject = Me.CurrentObjectTemplate
                Case ObjectSpecifier.CurrentFHEMObj
                    glbSpecifiedObject = Me.CurrentFHEMObj
                Case ObjectSpecifier.ParentObjectTemplate
                    glbSpecifiedObject = Me.ParentObjectTemplate
                Case ObjectSpecifier.ParentFHEMObj
                    glbSpecifiedObject = Me.ParentFHEMObj
                Case ObjectSpecifier.Template
                    glbSpecifiedObject = Me.Template
                Case ObjectSpecifier.TemplateFHEMObj
                    glbSpecifiedObject = Me.TemplateFHEMObj
            End Select

            Select Case Me.Property.GetType
                Case GetType(KeyValueObject)
                    If Not CType(Me.Property, KeyValueObject).Init(Me) Then Return False
                Case GetType(ReferenceObject)
                    If Not CType(Me.Property, ReferenceObject).Init(Me) Then Return False
                Case GetType(StringObject)
                    If Not CType(Me.Property, StringObject).Init(Me) Then Return False
            End Select

            Return True
        End Function

        <ComVisible(False)> Public Overrides Function ValidateAndCreate() As Boolean
            Return ValidateSelf()
        End Function
        Private Function ValidateSelf() As Boolean
            If Not [Enum].GetValues(GetType(ObjectSpecifier)).Cast(Of Integer).Contains(Me.ObjectSpecifier) Then Return False
            If Me.glbSpecifiedObject Is Nothing Then Return False

            Select Case Me.Property.GetType
                Case GetType(KeyValueObject)
                    If Not CType(Me.Property, KeyValueObject).ValidateAndCreate Then Return False
                Case GetType(ReferenceObject)
                    If Not CType(Me.Property, ReferenceObject).ValidateAndCreate Then Return False
                Case GetType(StringObject)
                    If Not CType(Me.Property, StringObject).ValidateAndCreate Then Return False
                Case Else
                    Return False
            End Select

            Return (Not String.IsNullOrEmpty(Me.ToString))
        End Function

        <ComVisible(True)> Public Overrides Function ToString() As String
            Dim myReturn As String = String.Empty

            Select Case Me.Property.GetType
                Case GetType(StringObject)
                    Dim myObject As Object = GetPropertyValue(glbSpecifiedObject, Me.Property.ToString)
                    If myObject IsNot Nothing Then
                        myReturn = myObject.ToString
                    End If
                Case GetType(KeyValueObject)
                    Dim myObject As Object = GetPropertyValue(glbSpecifiedObject, CType(Me.Property, KeyValueObject).Key)
                    If myObject IsNot Nothing Then
                        With CType(Me.Property, KeyValueObject)
                            .SetPropertyObject(myObject)
                            myReturn = .ToString
                        End With
                    End If
                Case GetType(ReferenceObject)
                    myReturn = Me.Property.ToString
            End Select

            Return myReturn
        End Function
    End Class
    <XmlType("KeyValueObject")> Public Class KeyValueObject
        Inherits StringReplaceObjectBase

        Private Property PropertyObject As Object
        <XmlAttribute("Key")> Public Property Key As String

        <XmlElement("StringObject", GetType(StringObject)),
            XmlElement("StringFormatObject", GetType(StringFormatObject)),
            XmlElement("KeyValueObject", GetType(KeyValueObject)),
            XmlElement("ReferenceObject", GetType(ReferenceObject))> Public Property Value As StringReplaceObjectBase

        Protected Friend Overloads Function Init(thisParent As ObjectTemplate) As Boolean
            If thisParent Is Nothing Then Return False

            If Not MyBase.Init(thisParent.Template, thisParent.TemplateFilter, thisParent.TemplateFHEMObj, thisParent.CurrentObjectTemplate, thisParent.CurrentFHEMObj, thisParent.ParentObjectTemplate, thisParent.ParentFHEMObj) Then Return False
            If MyBase.Init(Nothing) Then Return False
            If Not InitSelf() Then Return False

            Return True
        End Function
        Protected Friend Overloads Function Init(thisParent As StringReplaceObjectBase) As Boolean
            If Not MyBase.Init(thisParent) Then Return False
            If Not InitSelf() Then Return False

            Return True
        End Function
        Private Function InitSelf() As Boolean
            Select Case Me.Value.GetType
                Case GetType(StringFormatObject)
                    If Not CType(Me.Value, StringFormatObject).Init(Me) Then Return False
                Case GetType(KeyValueObject)
                    If Not CType(Me.Value, KeyValueObject).Init(Me) Then Return False
                Case GetType(ReferenceObject)
                    If Not CType(Me.Value, ReferenceObject).Init(Me) Then Return False
                Case GetType(StringObject)
                    If Not CType(Me.Value, StringObject).Init(Me) Then Return False
            End Select

            Return True
        End Function

        <ComVisible(False)> Public Overrides Function ValidateAndCreate() As Boolean
            Return ValidateSelf()
        End Function
        Private Function ValidateSelf() As Boolean
            If String.IsNullOrEmpty(Me.Key) Then Return False

            Select Case Me.Value.GetType
                Case GetType(StringFormatObject)
                    If Not CType(Me.Value, StringFormatObject).ValidateAndCreate Then Return False
                Case GetType(KeyValueObject)
                    If Not CType(Me.Value, KeyValueObject).ValidateAndCreate Then Return False
                Case GetType(ReferenceObject)
                    If Not CType(Me.Value, ReferenceObject).ValidateAndCreate Then Return False
                Case GetType(StringObject)
                    If Not CType(Me.Value, StringObject).ValidateAndCreate Then Return False
                Case Else
                    Return False
            End Select

            Return (Not String.IsNullOrEmpty(Me.ToString))
        End Function

        <ComVisible(True)> Public Overrides Function ToString() As String
            If Me.PropertyObject Is Nothing Then
                If Me.Value IsNot Nothing Then
                    Return Me.Value.ToString
                Else
                    Return String.Empty
                End If
            Else
                Return GetFromPropertyObject()
            End If
        End Function
        Protected Friend Sub SetPropertyObject(thisObject As Object)
            Me.PropertyObject = thisObject
        End Sub
        Private Function GetFromPropertyObject() As String
            If TypeOf Me.PropertyObject Is String AndAlso String.IsNullOrEmpty(Me.PropertyObject) Then Return String.Empty

            Dim myReturn As String = String.Empty

            Dim myInterfaces() As Type = Me.PropertyObject.GetType.GetInterfaces
            Dim myObject As Object = Nothing
            Select Case Me.Value.GetType
                Case GetType(StringObject)
                    Dim myIndex As Integer
                    If (Integer.TryParse(Me.Value.ToString, myIndex)) AndAlso (myInterfaces.Contains(GetType(ICollection))) Then
                        myObject = CType(Me.PropertyObject, ICollection)(myIndex)

                        If myObject IsNot Nothing Then
                            myReturn = myObject.ToString
                        End If
                    ElseIf (myInterfaces.Contains(GetType(IDictionary))) AndAlso (Not String.IsNullOrEmpty(Me.Value.ToString)) Then
                        Dim myDict As IDictionary = CType(Me.PropertyObject, IDictionary)
                        myObject = myDict.Item(Me.Value.ToString)

                        If myDict.Contains(Me.Value.ToString) Then
                            myReturn = myDict.Item(Me.Value.ToString).ToString
                        End If
                    Else
                        myReturn = Me.Value.ToString
                    End If
                Case GetType(KeyValueObject)
                    If myInterfaces.Contains(GetType(IDictionary)) Then
                        Dim myKey As String = CType(Me.Value, KeyValueObject).Key
                        myObject = CType(Me.PropertyObject, IDictionary).Item(myKey)

                        If myObject IsNot Nothing Then
                            With CType(Me.Value, KeyValueObject)
                                .SetPropertyObject(myObject)
                                myReturn = .ToString
                            End With
                        End If
                    ElseIf myInterfaces.Contains(GetType(ICollection)) Then
                        Dim myIndex As Integer = Integer.Parse(CType(Me.Value, KeyValueObject).Key.ToString)
                        myObject = CType(Me.PropertyObject, ICollection)(myIndex)

                        If myObject IsNot Nothing Then
                            With CType(Me.Value, KeyValueObject)
                                .SetPropertyObject(myObject)
                                myReturn = .ToString
                            End With
                        End If
                    End If
                Case GetType(ReferenceObject)
                    myReturn = Me.Value.ToString
            End Select
            Return myReturn
        End Function

    End Class
    <XmlType("StringObject")> Public Class StringObject
        Inherits StringReplaceObjectBase

        <XmlElement("Value")> Public Property Value() As String

        Protected Friend Overloads Function Init(thisParent As ObjectTemplate) As Boolean
            If thisParent Is Nothing Then Return False

            If Not MyBase.Init(thisParent.Template, thisParent.TemplateFilter, thisParent.TemplateFHEMObj, thisParent.CurrentObjectTemplate, thisParent.CurrentFHEMObj, thisParent.ParentObjectTemplate, thisParent.ParentFHEMObj) Then Return False
            If MyBase.Init(Nothing) Then Return False
            If Not InitSelf() Then Return False

            Return True
        End Function
        Protected Friend Overloads Function Init(thisParent As StringReplaceObjectBase) As Boolean
            If Not MyBase.Init(thisParent) Then Return False
            If Not InitSelf() Then Return False

            Return True
        End Function
        <CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")>
        Private Function InitSelf() As Boolean
            'nothing to do
            Return True
        End Function

        <ComVisible(False)> Public Overrides Function ValidateAndCreate() As Boolean
            Return (Not String.IsNullOrEmpty(Me.ToString))
        End Function

        <ComVisible(True)> Public Overrides Function ToString() As String
            Return Me.Value
        End Function
    End Class
End Namespace
