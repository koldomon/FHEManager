Imports System.Collections.ObjectModel
Imports System.Windows.Controls
Imports System.Runtime.InteropServices
<Assembly: CLSCompliant(True)>
Namespace FHEMObjects
    <CLSCompliant(True), DebuggerDisplay("{Name}")> Public Class FHEMObj
        Implements ComponentModel.INotifyPropertyChanged, IComparer(Of FHEMObj), IEquatable(Of FHEMObj), IEqualityComparer(Of FHEMObj)

        Public Event PropertyChanged(sender As Object, e As ComponentModel.PropertyChangedEventArgs) Implements ComponentModel.INotifyPropertyChanged.PropertyChanged

#Region "Properties"
        Private _Action As String = String.Empty
        Public Property Action As String
            Get
                Return _Action.ToLower
            End Get
            Set(ByVal value As String)
                _Action = value
            End Set
        End Property

        Public ReadOnly Property AliasName As String
            Get
                If Me.Attributes.ContainsKey("alias") Then
                    Return DirectCast(Me.Attributes("alias"), List(Of String)).First
                Else
                    Return String.Empty
                End If
            End Get
        End Property

        Private _Name As String
        Public Property Name As String
            Get
                Return _Name
            End Get
            Set(ByVal value As String)
                _Name = value
                RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("Name"))
            End Set
        End Property
        Public Property FHEMType As String

        Private _Definition As String
        Public Property Definition As String
            Get
                Return _Definition
            End Get
            Set(ByVal value As String)
                _Definition = value
                RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("Name"))
            End Set
        End Property

        Private _ConfigFileName As String = String.Empty
        <ComVisible(False)> Public Property ConfigFileName As String
            Get
                Return _ConfigFileName
            End Get
            Set(ByVal value As String)
                _ConfigFileName = value
                RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("ConfigFileName"))
            End Set
        End Property

        Public Property IODev As String
            Get
                If Me.Attributes.ContainsKey("IODev") Then
                    Return DirectCast(Me.Attributes("IODev"), List(Of String)).First
                Else
                    Return String.Empty
                End If
            End Get
            Set(ByVal value As String)
                DirectCast(Me.Attributes("IODev"), List(Of String))(0) = value
                RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("IODev"))
            End Set
        End Property
        <ComVisible(False)> Public ReadOnly Property Attributes As New Hashtable
        <ComVisible(False)> Public ReadOnly Property Associations() As New Collection(Of FHEMObj)
        <ComVisible(True)> Public Function AssociationsArray() As FHEMObj()
            Return Associations.ToArray
        End Function
        <ComVisible(False)> Public ReadOnly Property Logs As New Collection(Of FHEMObj)
        <ComVisible(True)> Public Function LogsArray() As FHEMObj()
            Return Logs.ToArray
        End Function
        <CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId:="SVG")>
        <ComVisible(False)> Public ReadOnly Property SVGPlots As New Collection(Of FHEMObj)
        Public Property UserComment As String

        <ComVisible(False)> Public ReadOnly Property Groups As Collection(Of String)
            Get
                Return New Collection(Of String)(DirectCast(Me.Attributes("group"), List(Of String)))
            End Get
        End Property
        <ComVisible(False)> Public ReadOnly Property Group As String
            Get
                If Me.Attributes.ContainsKey("group") Then
                    Return DirectCast(Me.Attributes("group"), List(Of String)).First
                Else
                    Return String.Empty
                End If
            End Get
        End Property
        <ComVisible(False)> Public ReadOnly Property Rooms As Collection(Of String)
            Get
                Return New Collection(Of String)(DirectCast(Me.Attributes("room"), List(Of String)))
            End Get
        End Property
        <ComVisible(False)> Public ReadOnly Property Room As String
            Get
                If Me.Attributes.ContainsKey("room") Then
                    Return DirectCast(Me.Attributes("room"), List(Of String)).First
                Else
                    Return String.Empty
                End If
            End Get
        End Property
#End Region

        <ComVisible(False)> Public Function GetCfgDefineString() As String
            Dim myReturn As String

            Dim myString As New List(Of String)
            Dim myIsGlobal As Boolean = Me.FHEMType.Equals("global")

            If (myIsGlobal) Then
                'on "global" only process Attributes
                myString.AddRange(GetAttributeStrings())
            Else
                'Format Begin for fhem.cfg
                myString.Add(String.Format("# |{0,-25}{1,-35}|", "Begin: ", Me.Name))

                'Format UserComment for fhem.cfg
                If Not String.IsNullOrEmpty(Me.UserComment) Then
                    myString.Add("# -UserComment")
                    For Each mySubString In Me.UserComment.Split(New Char() {Environment.NewLine, vbCrLf, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries).ToList
                        Dim myCleanString As String = mySubString.Trim(New Char() {"#", " ", "!"})
                        If Not String.IsNullOrEmpty(myCleanString) Then myString.Add(String.Format("# {0}", myCleanString))
                    Next
                End If

                'Format Definition
                myString.Add("# -Definition")
                myString.Add(GetDefineString)

                'Format Attributes
                If Me.Attributes.Count > 0 Then
                    myString.AddRange(GetAttributeStrings())
                End If

                'Format End for fhem.cfg
                myString.Add(String.Format("# |{0,-25}{1,-35}|", "End: ", Me.Name))
            End If

            myReturn = Join(myString.ToArray, Environment.NewLine)
            Return myReturn.ToString
        End Function
        <ComVisible(False)> Public Function GetDefineString() As String
            Return String.Format("define {0} {1} {2}", Me.Name, Me.FHEMType, Me.Definition)
        End Function

        <ComVisible(False)> Public Function GetCfgModifyString() As String
            Dim myReturn As String

            Dim myString As New List(Of String)
            Dim myIsGlobal As Boolean = Me.FHEMType.Equals("global")

            If (myIsGlobal) Then
                'on "global" only process Attributes
                myString.AddRange(GetAttributeStrings())
            Else
                myString.Add(GetModifyString)
                myString.AddRange(GetAttributeStrings())
            End If

            myReturn = Join(myString.ToArray, Environment.NewLine)
            Return myReturn.ToString
        End Function
        <ComVisible(False)> Public Function GetModifyString() As String
            Return String.Format("modify {0} {1}", Me.Name, Me.Definition)
        End Function

        <ComVisible(False)> Public Function GetAttributeStrings() As String()
            Dim myReturn As New List(Of String)

            Dim thisAttributes As Hashtable = Me.Attributes.Clone

            If thisAttributes.ContainsKey("userattr") Then
                myReturn.Add(GetAttributeDefineString("userattr"))
                thisAttributes.Remove("userattr")
            End If
            If thisAttributes.ContainsKey("IODev") Then
                myReturn.Add(GetAttributeDefineString("IODev"))
                thisAttributes.Remove("IODev")
            End If

            thisAttributes.Keys.Cast(Of String).OrderBy(Function(x) x).ToList.ForEach(Sub(x) myReturn.Add(GetAttributeDefineString(x)))

            Return myReturn.ToArray
        End Function

        <ComVisible(False)> Public Function GetAttributeString(thisName As String) As String
            Dim myReturn As String = String.Empty

            If Me.Attributes.ContainsKey(thisName) Then
                If TypeOf Me.Attributes(thisName) Is List(Of String) Then
                    myReturn = Join(DirectCast(Me.Attributes(thisName), List(Of String)).ToArray(), ",")
                End If
            End If

            Return myReturn
        End Function

        <ComVisible(False)> Public Function GetAttributeDefineString(thisName As String) As String
            Dim myReturn As String = String.Empty

            If Me.Attributes.ContainsKey(thisName) Then
                If TypeOf Me.Attributes(thisName) Is List(Of String) Then
                    myReturn = String.Format("attr {0} {1} {2}", Me.Name, thisName, Join(DirectCast(Me.Attributes(thisName), List(Of String)).ToArray(), ","))
                End If
            End If

            Return myReturn
        End Function


#Region "Fhem-ConfigFile Routines"
        <ComVisible(False)> Public Sub SetConfigFile()
            Me.ConfigFileName = My.Settings.DefaultFileName
            RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("ConfigFileName"))
        End Sub
        Public Sub SetConfigFile(thisConfigFile As String)
            Me.ConfigFileName = thisConfigFile
            RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("ConfigFileName"))
        End Sub
#End Region



        <ComVisible(False)> Public Function Compare(x As FHEMObj, y As FHEMObj) As Integer Implements IComparer(Of FHEMObj).Compare
            If x Is Nothing Then Return -1
            If y Is Nothing Then Return 1

            Return String.Compare(x.Name, y.Name)
        End Function

        <ComVisible(False)> Public Function Equals1(other As FHEMObj) As Boolean Implements IEquatable(Of FHEMObj).Equals
            If other Is Nothing Then Return False

            Return String.Equals(Me.Name, other.Name)
        End Function

        <ComVisible(False)> Public Function Equals2(x As FHEMObj, y As FHEMObj) As Boolean Implements IEqualityComparer(Of FHEMObj).Equals
            If x Is Nothing Then Return False
            If y Is Nothing Then Return False

            Return String.Equals(x.Name, y.Name)
        End Function

        <ComVisible(False)> Public Function GetHashCode1(obj As FHEMObj) As Integer Implements IEqualityComparer(Of FHEMObj).GetHashCode
            If obj Is Nothing Then Return New Random().NextDouble * Integer.MaxValue

            Return obj.Name.GetHashCode
        End Function


        '#Region "Fhem-Group Routines"
        '        Public Sub GroupUp(thisGroupName As String)
        '            Me.Attributes("group") = MoveUp(Me.Groups, thisGroupName)
        '            RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("Group"))
        '            RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("Groups"))
        '        End Sub
        '        Public Sub GroupDown(thisGroupName As String)
        '            Me.Attributes("group") = MoveDown(Me.Groups, thisGroupName)
        '            RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("Group"))
        '            RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("Groups"))
        '        End Sub
        '        Public Sub SetFirstGroup(thisGroupName As String)
        '            Me.Attributes("group") = SetFirst(Me.Groups, thisGroupName)
        '            RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("Group"))
        '            RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("Groups"))
        '        End Sub
        '        Public Sub AddGroup(thisGroupName As String, thisAfter As String)
        '            If Not Me.Attributes.ContainsKey("group") Then
        '                Me.Attributes.Add("group", New List(Of String))
        '            End If
        '            Me.Attributes("group") = AddItem(Me.Groups, thisGroupName, thisAfter)
        '            RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("Group"))
        '            RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("Groups"))
        '        End Sub
        '        Public Sub RemoveGroup(thisGroupName As String)
        '            DirectCast(Me.Attributes("group"), List(Of String)).Remove(thisGroupName)
        '            If DirectCast(Me.Attributes("group"), List(Of String)).Count = 0 Then
        '                Me.Attributes.Remove("group")
        '            End If
        '            RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("Group"))
        '            RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("Groups"))
        '        End Sub
        '#End Region

        '#Region "Fhem-Room Routines"
        '        Public Sub RoomUp(thisRoomName As String)
        '            Me.Attributes("room") = MoveUp(Me.Rooms, thisRoomName)
        '            RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("Room"))
        '            RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("Rooms"))
        '        End Sub
        '        Public Sub RoomDown(thisRoomName As String)
        '            Me.Attributes("room") = MoveDown(Me.Rooms, thisRoomName)
        '            RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("Room"))
        '            RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("Rooms"))
        '        End Sub
        '        Public Sub SetFirstRoom(thisRoomName As String)
        '            Me.Attributes("room") = SetFirst(Me.Rooms, thisRoomName)
        '            RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("Room"))
        '            RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("Rooms"))
        '        End Sub
        '        Public Sub AddRoom(thisRoomName As String, thisAfter As String)
        '            If Not Me.Attributes.ContainsKey("room") Then
        '                Me.Attributes.Add("room", New List(Of String))
        '            End If
        '            Me.Attributes("room") = AddItem(Me.Rooms, thisRoomName, thisAfter)
        '            RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("Room"))
        '            RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("Rooms"))
        '        End Sub
        '        Public Sub RemoveRoom(thisRoomName As String)
        '            DirectCast(Me.Attributes("room"), List(Of String)).Remove(thisRoomName)

        '            If DirectCast(Me.Attributes("room"), List(Of String)).Count = 0 Then
        '                Me.Attributes.Remove("room")
        '            End If
        '            RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("Room"))
        '            RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("Rooms"))
        '        End Sub
        '#End Region

        '#Region "ListSorting"
        '        Private Shared Function SetFirst(thisList As List(Of String), thisFirstItem As String) As List(Of String)
        '            Dim myReturn As List(Of String) = thisList.ToList
        '            Dim myItemIndex As Integer = myReturn.IndexOf(thisFirstItem)

        '            If myItemIndex > 0 Then
        '                myReturn.Remove(thisFirstItem)
        '                myReturn.Insert(0, thisFirstItem)
        '            End If

        '            Return myReturn
        '        End Function
        '        Private Shared Function AddItem(thisList As List(Of String), thisItem As String, thisAfter As String) As List(Of String)
        '            Dim myReturn As List(Of String) = thisList.ToList
        '            Dim myItemIndex As Integer = myReturn.IndexOf(thisAfter)

        '            If myItemIndex > 0 Then
        '                myReturn.Insert(myItemIndex + 1, thisItem)
        '            End If

        '            Return myReturn
        '        End Function
        '        Private Shared Function MoveUp(thisList As List(Of String), thisItem As String) As List(Of String)
        '            Dim myReturn As New List(Of String)
        '            Dim myIndex As Integer = thisList.IndexOf(thisItem)
        '            Dim myOffSet As Integer = 0

        '            If myIndex > 0 Then
        '                thisList.Remove(thisItem)
        '                thisList.Insert(myIndex - 1, thisItem)
        '            Else
        '                myReturn = thisList
        '            End If

        '            Return myReturn
        '        End Function
        '        Private Function MoveDown(thisList As List(Of String), thisItem As String) As List(Of String)
        '            Dim myReturn As New List(Of String)

        '            Dim myIndex As Integer = thisList.IndexOf(thisItem)

        '            Dim myOffSet As Integer = 0
        '            If myIndex < thisList.Count - 1 Then
        '                thisList.Remove(thisItem)
        '                thisList.Insert(myIndex + 1, thisItem)
        '            Else
        '                myReturn = thisList
        '            End If
        '            Return myReturn
        '        End Function
        '#End Region

        '#Region "ViewItems"
        '        Private _TreeViewItem As TreeViewItem
        '        <ComVisible(False)> Public ReadOnly Property TreeViewItem As TreeViewItem
        '            Get
        '                If _TreeViewItem Is Nothing Then _TreeViewItem = New TreeViewItem With {.Header = Me.Name, .Tag = Me}
        '                Return _TreeViewItem
        '            End Get
        '        End Property

        '        Private _ListViewItem As ListViewItem
        '        <ComVisible(False)> Public ReadOnly Property ListViewItem As ListViewItem
        '            Get
        '                If _ListViewItem Is Nothing Then _ListViewItem = New ListViewItem With {.Name = Me.Name, .Tag = Me}
        '                Return _ListViewItem
        '            End Get
        '        End Property
        '#End Region
    End Class

End Namespace