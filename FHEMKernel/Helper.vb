Imports System.IO
Imports System.Xml
Imports System.Xml.Serialization

Public Module Helper
    Friend SerializationEvents As New List(Of String)
    Public Function ReadFromXML(t As Type, f As String) As Object
        Dim myReturn As Object = Nothing

        Dim myMemStream As MemoryStream = FileToMemStream(f)
        Dim myXMLReader As XmlReader = GetXMLReader(myMemStream)

        SerializationEvents.Clear()

        Dim mySerializer As New XmlSerializer(t)

        AddHandler mySerializer.UnknownAttribute, AddressOf HandleUnknownAttribute
        AddHandler mySerializer.UnknownElement, AddressOf HandleUnknownElement

        If mySerializer.CanDeserialize(myXMLReader) Then
            myReturn = mySerializer.Deserialize(myXMLReader)
        End If

        RemoveHandler mySerializer.UnknownAttribute, AddressOf HandleUnknownAttribute
        RemoveHandler mySerializer.UnknownElement, AddressOf HandleUnknownElement

        Return myReturn
    End Function
    Public Function WriteToXML(o As Object, f As String, mode As IO.FileMode) As String
        Dim myReturn As String = String.Empty

        Dim myMemStream As MemoryStream = New MemoryStream
        Using myXMLWriter As XmlWriter = GetXMLWriter(myMemStream)
            Dim mySerializer As New XmlSerializer(o.GetType)
            mySerializer.Serialize(myXMLWriter, o)
            myXMLWriter.Flush()
            myMemStream.Seek(0, SeekOrigin.Begin)

            If mode = FileMode.Create Or mode = FileMode.CreateNew Then
                myReturn = StreamToNewFile(myMemStream, f)
            ElseIf mode = FileMode.Append Then
                myReturn = StreamAppendToFile(myMemStream, f)
            Else
                myReturn = "Wrong Filemode"
            End If
            myXMLWriter.Close()
        End Using
        If myMemStream IsNot Nothing Then myMemStream.Dispose()

        Return myReturn
    End Function
    Public Function WriteToFile(s As String, f As String, mode As IO.FileMode) As String
        Dim myReturn As String = String.Empty

        Dim myMemStream As MemoryStream = New MemoryStream
        Using myTextWriter As TextWriter = GetTextWriter(myMemStream)
            myTextWriter.WriteLine(s)
            myTextWriter.Flush()
            myMemStream.Seek(0, SeekOrigin.Begin)

            If mode = FileMode.Create Or mode = FileMode.CreateNew Then
                myReturn = StreamToNewFile(myMemStream, f)
            ElseIf mode = FileMode.Append Then
                myReturn = StreamAppendToFile(myMemStream, f)
            Else
                myReturn = "Wrong Filemode"
            End If
            myTextWriter.Close()
        End Using
        If myMemStream IsNot Nothing Then myMemStream.Dispose()

        Return myReturn
    End Function

    Public Function FileToMemStream(thisFileName As String) As MemoryStream
        Dim myReturn As New MemoryStream()

        Using myFileReader = File.OpenRead(thisFileName)
            myFileReader.CopyTo(myReturn)
        End Using

        myReturn.Seek(0, SeekOrigin.Begin)

        Return myReturn
    End Function

    Public Function StreamToNewFile(thisStream As MemoryStream, thisFileName As String) As String
        Dim myReturn As String = String.Empty

        Using myFileWriter = File.Create(thisFileName, 4096, FileOptions.WriteThrough)
            thisStream.Seek(0, SeekOrigin.Begin)
            thisStream.WriteTo(myFileWriter)
            myFileWriter.Flush()
            myReturn = myFileWriter.Name
        End Using

        Return myReturn
    End Function
    Public Function StreamAppendToFile(thisStream As MemoryStream, thisFileName As String) As String
        Dim myReturn As String = String.Empty

        Using myFileWriter = File.Open(thisFileName, FileMode.Append, FileAccess.Write, FileShare.Write)
            thisStream.Seek(0, SeekOrigin.Begin)
            thisStream.WriteTo(myFileWriter)
            myFileWriter.Flush()
            myReturn = myFileWriter.Name
        End Using

        Return myReturn
    End Function

    Private Function GetXMLReader(thisStream As Stream) As XmlReader
        Dim myReturn As XmlReader = Nothing

        Dim myXMLSettings As New XmlReaderSettings With {
            .IgnoreComments = True,
            .IgnoreProcessingInstructions = True,
            .IgnoreWhitespace = True
        }

        myReturn = XmlReader.Create(thisStream, myXMLSettings)

        Return myReturn
    End Function
    Private Function GetXMLWriter(thisStream As Stream) As XmlWriter
        Dim myReturn As XmlWriter = Nothing

        Dim myXMLSettings As New XmlWriterSettings With {
            .Indent = True,
            .CloseOutput = False,
            .WriteEndDocumentOnClose = False
        }

        myReturn = XmlWriter.Create(thisStream, myXMLSettings)

        Return myReturn
    End Function
    Private Function GetTextWriter(thisStream As Stream) As TextWriter
        Dim myReturn As TextWriter = Nothing

        myReturn = New StreamWriter(thisStream)

        Return myReturn
    End Function

    Private Sub HandleUnknownAttribute(sender As Object, e As XmlAttributeEventArgs)
        Dim myEventString As String = String.Format("Object:'{0}'|Unknown Attr: '{1}'|Line:'{2:d4}'|Position:'{3:d3}'|Expected:'{4}'",
                                              New Object() {e.ObjectBeingDeserialized.GetType.Name,
                                                            e.Attr.Name,
                                                            e.LineNumber,
                                                            e.LinePosition,
                                                            e.ExpectedAttributes}
                                              )
        SerializationEvents.Add(myEventString)
        My.Application.Log.WriteEntry(myEventString, TraceEventType.Warning)
    End Sub
    Private Sub HandleUnknownElement(sender As Object, e As XmlElementEventArgs)
        Dim myEventString As String = String.Format("Object:'{0}'|Unknown Element:'{1}'|Line:'{2:d4}'|Position:'{3:d3}'|Expected:'{4}'",
                                              New Object() {e.ObjectBeingDeserialized.GetType.Name,
                                                            e.Element.Name,
                                                            e.LineNumber,
                                                            e.LinePosition,
                                                            e.ExpectedElements}
                                              )
        SerializationEvents.Add(myEventString)
        My.Application.Log.WriteEntry(myEventString, TraceEventType.Warning)
    End Sub
End Module
