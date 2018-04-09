Imports System.Runtime.InteropServices

Namespace FHEMObjects
    <CLSCompliant(True)> Public Class FHEMLogEntry
        Implements IEqualityComparer, IEqualityComparer(Of FHEMLogEntry)
        Public Property LogTime() As DateTime
        <ComVisible(False)> Public Property LogLevel() As Integer?
        <ComVisible(True)> Public Property LogLevelValue As Integer
            Get
                If Not Me.LogLevel.HasValue Then Return -1
                Return Me.LogLevel.Value
            End Get
            Set(value As Integer)
                Me.LogLevel = value
            End Set
        End Property
        Public Property LogObj() As String

        Private _LogText As New Text.StringBuilder
        Public ReadOnly Property LogText() As String
            Get
                Return _LogText.ToString
            End Get
        End Property
        Public Property Logline() As New Text.StringBuilder


        <ComVisible(False)> Public Function AppendLogText(value As String) As Boolean
            _LogText.Append(value)
            Return True
        End Function

        <ComVisible(False)> Public Function Save() As String
            Return Me.Logline.ToString
        End Function

        <ComVisible(False)> Public Function Equals1(x As FHEMLogEntry, y As FHEMLogEntry) As Boolean Implements IEqualityComparer(Of FHEMLogEntry).Equals
            If x Is Nothing Then Return False
            If y Is Nothing Then Return False

            Return String.Equals(x.Logline, y.Logline)
        End Function

        <ComVisible(False)> Public Function GetHashCode1(obj As FHEMLogEntry) As Integer Implements IEqualityComparer(Of FHEMLogEntry).GetHashCode
            If obj Is Nothing Then Return New Random().NextDouble * Integer.MaxValue

            Return obj.Logline.GetHashCode
        End Function

        <ComVisible(False)> Public Function Equals2(x As Object, y As Object) As Boolean Implements IEqualityComparer.Equals
            If x Is Nothing Then Return False
            If y Is Nothing Then Return False

            If Not (TypeOf (x) Is FHEMLogEntry Or TypeOf (y) Is FHEMLogEntry) Then
                Return False
            Else
                Return String.Equals(CType(x, FHEMLogEntry).Logline, CType(y, FHEMLogEntry).Logline)
            End If
        End Function

        <ComVisible(False)> Public Function GetHashCode2(obj As Object) As Integer Implements IEqualityComparer.GetHashCode
            If obj Is Nothing Then Return New Random().NextDouble * Integer.MaxValue

            If Not (TypeOf (obj) Is FHEMLogEntry) Then
                Return New Random().NextDouble * Integer.MaxValue
            Else
                Return CType(obj, FHEMLogEntry).Logline.GetHashCode
            End If
        End Function
    End Class
End Namespace