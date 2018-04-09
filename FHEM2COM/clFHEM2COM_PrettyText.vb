Partial Class FHEMCOM
    Friend Shared Function GetIndentedText(thisString As String) As String
        Dim myReturn As New Text.StringBuilder

        Dim myIndent As Integer = 0
        Dim myChars = thisString.Replace(vbTab, String.Empty).ToCharArray
        Dim myType As CharTypeEnum = CharTypeEnum.unkown

        For i = 0 To myChars.LongCount - 1
            If i < myChars.Count - 1 Then
                myType = GetCharType(String.Format("{0}{1}", myChars(i).ToString, myChars(i + 1).ToString))
                If Not myType = CharTypeEnum.Ignore Then
                    myReturn.Append(myChars(i))
                    Select Case myType
                        Case CharTypeEnum.In
                            myIndent += 1
                            myReturn.Append(vbCr)
                            myReturn.Append(vbTab, myIndent)
                        Case CharTypeEnum.Out
                            myIndent -= 1
                            'If myIndent < 0 Then myIndent = 0
                            myReturn.Append(vbCr)
                            myReturn.Append(vbTab, myIndent)
                    End Select
                End If
            Else
                myReturn.Append(myChars(i))
            End If
        Next

        Return myReturn.ToString
    End Function

    'Private Shared Function GetNextCharType(thisChars As Char(), currentIndex As Long) As CharTypeEnum
    '    Dim i As Long = currentIndex + 1

    '    If i >= thisChars.Count - 1 Then Return CharTypeEnum.unkown

    '    Dim myReturn As CharTypeEnum = GetCharType(thisChars(i))
    '    While myReturn = CharTypeEnum.Ignore
    '        i += 1
    '        If i <= thisChars.Count - 1 Then
    '            myReturn = GetCharType(thisChars(i))
    '        Else
    '            Return CharTypeEnum.unkown
    '        End If
    '    End While

    '    Return myReturn
    'End Function

    Private Shared Function GetCharType(thisString As String) As CharTypeEnum
        Select Case thisString
            Case "((", "{(", "{" & vbCrLf
                Return CharTypeEnum.In
            Case "))", ")}", "}" & vbCrLf
                Return CharTypeEnum.Out
            Case vbCrLf & vbCrLf, vbCr & vbCr, vbLf & vbLf
                Return CharTypeEnum.Ignore
            Case Else
                Return CharTypeEnum.unkown
        End Select
    End Function

    Private Enum CharTypeEnum
        unkown
        [In]
        [Out]
        Ignore
    End Enum
End Class


