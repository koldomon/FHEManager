Imports System.Collections.ObjectModel
Imports System.Runtime.InteropServices
Imports System.Xml.Serialization
Imports MM.FHEMManager.FHEMObjects

Namespace FHEMTemplates
    <XmlType("TemplateBase")> Public MustInherit Class TemplateBase
        <XmlIgnore()> Protected Friend ReadOnly Property Template As FHEMTemplate = Nothing
        <XmlIgnore()> Protected Friend ReadOnly Property TemplateFilter As String = Nothing
        <XmlIgnore()> Protected Friend ReadOnly Property TemplateFHEMObj As FHEMObj = Nothing
        <XmlIgnore()> Protected Friend ReadOnly Property CurrentObjectTemplate As ObjectTemplate = Nothing
        <XmlIgnore()> Protected Friend ReadOnly Property CurrentFHEMObj As FHEMObj = Nothing
        <XmlIgnore()> Protected Friend ReadOnly Property ParentObjectTemplate As ObjectTemplate = Nothing
        <XmlIgnore()> Protected Friend ReadOnly Property ParentFHEMObj As FHEMObj = Nothing

        Protected Friend Overridable Function Init(thisFHEMTemplate As FHEMTemplate,
                                                   thisTemplateFilterName As String,
                                                    thisTemplateFHEMObj As FHEMObj,
                                                    thisCurrentObjectTemplate As ObjectTemplate,
                                                    thisCurrentFHEMObj As FHEMObj,
                                                    thisParentObjectTemplate As ObjectTemplate,
                                                    thisParentFHEMObj As FHEMObj) As Boolean
            If Not Me.Reset() Then Return False
            Me._Template = thisFHEMTemplate
            Me._TemplateFilter = thisTemplateFilterName
            Me._TemplateFHEMObj = thisTemplateFHEMObj
            Me._CurrentObjectTemplate = thisCurrentObjectTemplate
            Me._CurrentFHEMObj = thisCurrentFHEMObj
            Me._ParentObjectTemplate = thisParentObjectTemplate
            Me._ParentFHEMObj = thisParentFHEMObj
            Return True
        End Function
        Protected Overridable Function Reset() As Boolean
            Me._Template = Nothing
            Me._TemplateFilter = Nothing
            Me._TemplateFHEMObj = Nothing
            Me._CurrentObjectTemplate = Nothing
            Me._CurrentFHEMObj = Nothing
            Me._ParentObjectTemplate = Nothing
            Me._ParentFHEMObj = Nothing
            Return True
        End Function

        Public MustOverride Function ValidateAndCreate() As Boolean

    End Class
    <ComVisible(True), XmlType()> Public Enum ObjectSpecifier
        <XmlEnum> Template
        <XmlEnum> TemplateFHEMObj
        <XmlEnum> CurrentObjectTemplate
        <XmlEnum> CurrentFHEMObj
        <XmlEnum> ParentObjectTemplate
        <XmlEnum> ParentFHEMObj
        <XmlEnum> ParentObject
        Other
    End Enum


    <XmlType("FHEMObjectTemplate")> Public Class ObjectTemplate
        Inherits TemplateBase

        '<XmlAttribute("FilterName")> Public Property FilterName As String
        <ComVisible(False), XmlArray("Filters"), XmlArrayItem("Filter", GetType(String))> Public Property Filters As New Collection(Of String)
        <XmlAttribute("FHEMType")> Public Property FHEMType As String
        <XmlElement("Name", GetType(StringFormatObject)), XmlElement("SimpleName", GetType(StringObject))> Public Property Name As StringReplaceObjectBase
        <XmlElement("Definition", GetType(StringFormatObject)), XmlElement("SimpleDefinition", GetType(StringObject))> Public Property Definition As StringReplaceObjectBase
        <ComVisible(False), XmlArray("Attributes"), XmlArrayItem("KeyValueObject")> Public Property Attributes As New Collection(Of KeyValueObject)
        <ComVisible(True)> Public Function AttributesArray() As KeyValueObject()
            Return _Attributes.ToArray
        End Function

        <ComVisible(False), XmlArray("ObjectTemplates"), XmlArrayItem("FHEMObject")> Public Property ObjectTemplates As New Collection(Of ObjectTemplate)
        <ComVisible(True)> Public Function ObjectTemplatesArray() As ObjectTemplate()
            Return _ObjectTemplates.ToArray
        End Function

        Protected Overloads Function Init(thisParent As ObjectTemplate) As Boolean
            If thisParent Is Nothing Then Return False

            If Not MyBase.Init(thisParent.Template, thisParent.TemplateFilter, thisParent.TemplateFHEMObj, Me, New FHEMObj With {.Action = "define", .FHEMType = Me.FHEMType}, thisParent.CurrentObjectTemplate, thisParent.CurrentFHEMObj) Then Return False
            If Not InitSelf() Then Return False

            Return True
        End Function
        <ComVisible(False)> Public Overloads Function Init(thisTemplate As FHEMTemplate, thisTemplateFilter As String, thisTemplateApplyToObject As FHEMObj) As Boolean
            If Not MyBase.Init(thisTemplate, thisTemplateFilter, thisTemplateApplyToObject, Me, New FHEMObj With {.Action = "define", .FHEMType = Me.FHEMType}, Nothing, Nothing) Then Return False
            If Not InitSelf() Then Return False

            Return True
        End Function
        Private Function InitSelf() As Boolean
            If Me.Name IsNot Nothing Then
                Select Case Me.Name.GetType
                    Case GetType(StringFormatObject)
                        If Not CType(Me.Name, StringFormatObject).Init(Me) Then Return False
                    Case GetType(StringObject)
                        If Not CType(Me.Name, StringObject).Init(Me) Then Return False
                End Select
            End If
            If Me.Definition IsNot Nothing Then
                Select Case Me.Definition.GetType
                    Case GetType(StringFormatObject)
                        If Not CType(Me.Definition, StringFormatObject).Init(Me) Then Return False
                    Case GetType(StringObject)
                        If Not CType(Me.Definition, StringObject).Init(Me) Then Return False
                End Select
            End If

            Dim myReturn As Boolean = True
            Me.Attributes.ToList.ForEach(Sub(x) If Not x.Init(Me) Then myReturn = False)
            If Not myReturn Then Return False

            Me.ObjectTemplates.ToList.ForEach(Sub(x) If Not x.Init(Me) Then myReturn = False)
            If Not myReturn Then Return False

            Return True
        End Function

        <ComVisible(False)> Public Overrides Function ValidateAndCreate() As Boolean
            Return ValidateSelf()
        End Function
        Private Function ValidateSelf() As Boolean
            If Me.Name IsNot Nothing Then
                If Not Me.Name.ValidateAndCreate() Then Return False
                Me.CurrentFHEMObj.Name = Me.Name.ToString
            End If
            If Me.Definition IsNot Nothing Then
                If Not Me.Definition.ValidateAndCreate() Then Return False
                Me.CurrentFHEMObj.Definition = Me.Definition.ToString
            End If

            Dim myTemp As Boolean = True

            Me.Attributes.ToList.ForEach(Sub(x) If Not x.ValidateAndCreate() Then myTemp = False)
            If Not myTemp Then Return False
            Me.Attributes.ToList.ForEach(Sub(x) If Not Me.CurrentFHEMObj.Attributes.ContainsKey(x.Key) Then Me.CurrentFHEMObj.Attributes.Add(x.Key, New List(Of String) From {x.Value.ToString}))

            Me.ObjectTemplates.ToList.ForEach(Sub(x) If Not x.ValidateAndCreate() Then myTemp = False)
            If Not myTemp Then Return False
            Me.CurrentFHEMObj.UserComment = String.Format("Created by '{0} ({1})' in Template '{2}' at '{3}'",
                                                 My.Application.Info.Title,
                                                 My.Application.Info.Version.ToString,
                                                 Me.Template.Name,
                                                 DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss"))

            Return True
        End Function
        <ComVisible(False)> Public Function GetCreatedFHEMObjs() As Collection(Of FHEMObj)
            Dim myReturn As New List(Of FHEMObj) From {
                Me.CurrentFHEMObj()
            }
            Me.ObjectTemplates.ToList.ForEach(Sub(x) myReturn.AddRange(x.GetCreatedFHEMObjs)) '.OrderBy(Function(x) x.Name.ToString)
            Return New Collection(Of FHEMObj)(myReturn)
        End Function

        'Public Overrides Function ToString() As String
        '    Return String.Format("{0} as {1}", Me.CurrentFHEMObj.Name.ToString, Me.FHEMType)
        'End Function
    End Class
End Namespace