Imports MM.FHEMManager.FHEMObjects
Imports MM.FHEMManager.FHEMTemplates

<Assembly: CLSCompliant(True)>
<CLSCompliant(True)> Public Class FHEMKernel
    Implements ComponentModel.INotifyPropertyChanged

    Private glbUnknown As New Text.StringBuilder

    Public Shared ReadOnly cPathSplitChar() As Char = {"\"}
    Public Shared ReadOnly cPathTrimChars() As Char = {"""", " ", vbTab}
    Public Shared ReadOnly cWordSplitChar() As Char = {" "}
    Public Shared ReadOnly cAttributeListSplitChar() As Char = {","}

    Public ReadOnly Property SerializationErrors As New Collection(Of String)

    Friend Function AllGroups() As SortedList(Of String, String)
        Dim myReturn As New SortedList(Of String, String)
        If Me.AllObjectsList Is Nothing OrElse Me.AllObjectsList.Count = 0 Then
            myReturn.Add(String.Empty, String.Empty)
        Else
            Me.AllObjectsList.Where(Function(x) _
                                        x.Attributes.ContainsKey("group")
                                        ).Select(Function(x) _
                                                     x.Groups.Cast(Of String).ToList
                                                     ).ToList.ForEach(Sub(x)
                                                                          For Each myItem As String In x
                                                                              If Not myReturn.ContainsKey(myItem) Then
                                                                                  myReturn.Add(myItem, myItem)
                                                                              End If
                                                                          Next

                                                                      End Sub)
        End If
        Return myReturn
    End Function

    Friend Function AllRooms() As SortedList(Of String, String)
        Dim myReturn As New SortedList(Of String, String)
        If Me.AllObjectsList Is Nothing OrElse Me.AllObjectsList.Count = 0 Then
            myReturn.Add(String.Empty, String.Empty)
        Else
            Me.AllObjectsList.Where(Function(x) _
                                        x.Attributes.ContainsKey("room")
                                        ).Select(Function(x) _
                                                     x.Rooms.Cast(Of String).ToList
                                                     ).ToList.ForEach(Sub(x)
                                                                          For Each myItem As String In x
                                                                              If Not myReturn.ContainsKey(myItem) Then
                                                                                  myReturn.Add(myItem, myItem)
                                                                              End If
                                                                          Next

                                                                      End Sub)
        End If
        Return myReturn
    End Function

    Public ReadOnly Property CurrentObjects As New Collection(Of FHEMObj)
    Private Function AllObjectsList() As List(Of FHEMObj)
        If CurrentObjects IsNot Nothing Then
            Return CurrentObjects.ToList
        Else
            Return Nothing
        End If
    End Function

    Public ReadOnly Property CurrentTemplates As New Collection(Of FHEMTemplate)
    <CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")>
    Private Function AllTemplatesList() As List(Of FHEMTemplate)
        If CurrentTemplates IsNot Nothing Then
            Return CurrentTemplates.ToList
        Else
            Return Nothing
        End If
    End Function
    Public Sub New()
        Init()
    End Sub

    Private Sub Init()
        My.Settings.Reload()
        InitObjects()
        InitTemplates()
    End Sub
    Private Sub InitObjects()
        If Me.CurrentObjects.FirstOrDefault(Function(x) x.Name = "global") Is Nothing Then
            Me.CurrentObjects.Add(New FHEMObj With {.Action = "define",
                                             .Name = "global",
                                             .FHEMType = "global",
                                             .Definition = String.Empty})
        End If
    End Sub
    <CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")>
    Private Sub InitTemplates()
        'nothing to do yet
    End Sub

    Private Sub ResetObjects()
        Me.CurrentObjects.Clear()
        InitObjects()
    End Sub

    Private Sub ResetTemplates()
        Me.CurrentTemplates.Clear()
        Me.SerializationErrors.Clear()
        InitTemplates()
    End Sub

    Public Event PropertyChanged(sender As Object, e As ComponentModel.PropertyChangedEventArgs) Implements ComponentModel.INotifyPropertyChanged.PropertyChanged
End Class

Partial Class FHEMKernel
    ''' <summary>
    ''' Misc Methods
    ''' </summary>
    ''' <remarks></remarks>

    Public Function GetObjectsCount() As Integer
        Return Me.CurrentObjects.Count
    End Function
End Class
