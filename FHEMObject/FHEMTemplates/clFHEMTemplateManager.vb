Imports System.Collections.ObjectModel
Imports System.Runtime.InteropServices
Imports System.Xml.Serialization
Imports MM.FHEMManager.FHEMObjects

Namespace FHEMTemplates
    <XmlRoot(IsNullable:=False), XmlType("FHEMTemplateManager")> Public Class FHEMTemplateManager

        <ComVisible(False), XmlArray("Templates"), XmlArrayItem("Template")> Public ReadOnly Property Templates As New Collection(Of FHEMTemplate)
        <ComVisible(True)> Public Function TemplatesArray() As FHEMTemplate()
            Return _Templates.ToArray
        End Function
    End Class
    <XmlType("Template")> Public Class FHEMTemplate
        <CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId:="ID")>
        <XmlAttribute("ID")> Public Property ID As Integer
        <XmlAttribute("Name")> Public Property Name As String
        <ComVisible(False), XmlArray("Filters"), XmlArrayItem("Filter")> Public Property Filters As New Collection(Of ObjectFilter)
        <ComVisible(True)> Public Function FiltersArray() As ObjectFilter()
            Return _Filters.ToArray
        End Function

        <ComVisible(False), XmlArray("ObjectTemplates"), XmlArrayItem("FHEMObject")> Public Property ObjectTemplates As New Collection(Of ObjectTemplate)
        <ComVisible(True)> Public Function ObjectTemplatesArray() As ObjectTemplate()
            Return _ObjectTemplates.ToArray
        End Function
    End Class
    <XmlType("ObjectFilter")> Public Class ObjectFilter
        <XmlAttribute("Name")> Public Property Name As String

        <CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId:="FHEM")>
        <XmlElement("Type")> Public Property FHEMType As String
        <CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId:="FHEM")>
        <XmlElement("Name")> Public Property FHEMName As String
        <ComVisible(False), XmlArray("Attributes"), XmlArrayItem("KeyValueObject")> Public Property Attributes As New Collection(Of KeyValueObject)
        <ComVisible(True)> Public Function AttributesArray() As KeyValueObject()
            Return _Attributes.ToArray
        End Function
    End Class
End Namespace