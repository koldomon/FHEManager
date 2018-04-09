Public Class FHEMLogEntry
    Implements IEqualityComparer, IEqualityComparer(Of FHEMLogEntry)

    Private _LogTime As DateTime
    Public Property LogTime() As DateTime
        Get
            Return _LogTime
        End Get
        Set(ByVal value As DateTime)
            _LogTime = value
        End Set
    End Property

    Private _LogLevel As Integer?
    Public Property LogLevel() As Integer?
        Get
            Return _LogLevel
        End Get
        Set(ByVal value As Integer?)
            _LogLevel = value
        End Set
    End Property

    Private _LogObj As String
    Public Property LogObj() As String
        Get
            Return _LogObj
        End Get
        Set(ByVal value As String)
            _LogObj = value
        End Set
    End Property

    Private _LogText As New Text.StringBuilder
    Public ReadOnly Property LogText() As String
        Get
            Return _LogText.ToString
        End Get
    End Property

    Private _LogLine As New Text.StringBuilder
    Public Property LogLine() As Text.StringBuilder
        Get
            Return _LogLine
        End Get
        Set(ByVal value As Text.StringBuilder)
            _LogLine = value
        End Set
    End Property


    Public Sub New()

    End Sub

    Public Function AppendLogText(thisString As String) As Boolean
        _LogText.Append(thisString)
        Return True
    End Function

    Public Function Save() As String
        Return Me.LogLine.ToString
    End Function

    Public Function Equals1(x As FHEMLogEntry, y As FHEMLogEntry) As Boolean Implements IEqualityComparer(Of FHEMLogEntry).Equals
        Return String.Equals(x.LogLine, y.LogLine)
    End Function

    Public Function GetHashCode1(obj As FHEMLogEntry) As Integer Implements IEqualityComparer(Of FHEMLogEntry).GetHashCode
        Return obj.LogLine.GetHashCode
    End Function

    Public Function Equals2(x As Object, y As Object) As Boolean Implements IEqualityComparer.Equals
        If Not (TypeOf (x) Is FHEMLogEntry Or TypeOf (y) Is FHEMLogEntry) Then
            Return False
        Else
            Return String.Equals(CType(x, FHEMLogEntry).LogLine, CType(y, FHEMLogEntry).LogLine)
        End If
    End Function

    Public Function GetHashCode2(obj As Object) As Integer Implements IEqualityComparer.GetHashCode
        If Not (TypeOf (obj) Is FHEMLogEntry) Then
            Return False
        Else
            Return CType(obj, FHEMLogEntry).LogLine.GetHashCode
        End If
    End Function
End Class
