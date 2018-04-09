Imports MM.FHEMManager.FHEMObjects

Partial Class FHEMKernel
#Region "Load Methods"
    Public Function LoadCfg() As Boolean?
        My.Application.Log.WriteEntry(String.Format("Load using default config: {0}{2}{1}", My.Settings.DefaultPath, My.Settings.DefaultFileName, cPathSplitChar(0)), TraceEventType.Information)

        If Not Me.LoadCfg(String.Format("{0}{2}{1}", My.Settings.DefaultPath, My.Settings.DefaultFileName, cPathSplitChar(0))) Then
            Return False
        End If

        RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("AllGroups"))
        RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs("AllRooms"))

        Return True
    End Function

    Public Function LoadCfg(ByVal thisCfgPathAndFileName As String) As Boolean?
        If String.IsNullOrEmpty(thisCfgPathAndFileName) Then Return False


        Dim myPath As String = thisCfgPathAndFileName.Trim(cPathTrimChars)
        If IO.File.Exists(myPath) Then
            ResetObjects()
            My.Application.Log.WriteEntry(String.Format("Load using given config: {0}", thisCfgPathAndFileName), TraceEventType.Information)

            Dim myMemStream As IO.MemoryStream = Helper.FileToMemStream(myPath)
            Using myCfg As IO.StreamReader = New IO.StreamReader(myMemStream)
                Try
                    If myCfg.Peek > -1 Then
                        If ReadCfgStream(myCfg, ExtractCfgFileName(myPath)) Then
                            If BuildAssociations() Then
                                Return True
                            End If
                        End If
                    End If
                Catch ex As Exception
                    My.Application.Log.WriteException(ex, TraceEventType.Error, ex.StackTrace)
                    Return False
                End Try
            End Using
            If myMemStream IsNot Nothing Then myMemStream.Dispose()

        Else
            My.Application.Log.WriteEntry(String.Format("IO.File.Exists({0}) returned False", myPath), TraceEventType.Warning)
            Return False
        End If

        Return False
    End Function

    Private Function ReadCfgStream(ByRef thisStream As IO.StreamReader, ByVal thisCfgFile As String) As Boolean?
        If thisStream Is Nothing Then Return False
        If String.IsNullOrEmpty(thisCfgFile) Then Return False

        Dim myUserComments As New Text.StringBuilder

        While thisStream.Peek > -1
            Dim myLine As String = thisStream.ReadLine()
            If Not String.IsNullOrEmpty(myLine) Then
                If myLine.StartsWith("# [") Or myLine.StartsWith("# |") Or myLine.StartsWith("# -") Then
                    'Programm Comment - ignore!
                ElseIf myLine.StartsWith("##") Or myLine.StartsWith("#") Then
                    myUserComments.AppendLine(myLine)
                Else
                    Dim myWords As List(Of String) = myLine.Split(cWordSplitChar, StringSplitOptions.RemoveEmptyEntries).ToList
                    If myWords.Count >= 2 Then
                        My.Application.Log.WriteEntry(myLine, TraceEventType.Verbose)
                        Select Case myWords(0).ToLower
                            Case "include"
                                glbUnknown.Append(myUserComments.ToString)
                                myUserComments.Clear()
                                If Not ProcessFhemInclude(myWords) Then
                                    Return False
                                End If
                            Case "define"
                                If Not ProcessFhemDefine(myWords, thisStream, thisCfgFile, myUserComments) Then
                                    Return False
                                End If
                            Case "attr"
                                glbUnknown.Append(myUserComments.ToString)
                                myUserComments.Clear()
                                If Not ProcessFhemAttribute(myWords) Then
                                    Return False
                                End If
                            Case Else
                                glbUnknown.Append(myLine)
                                My.Application.Log.WriteEntry(String.Format("Unknown: {0}", myLine), TraceEventType.Information)
                        End Select
                    End If
                End If
            End If
        End While

        Return True
    End Function

    Private Function ProcessFhemInclude(ByVal thisWords As List(Of String)) As Boolean?
        If thisWords Is Nothing Then Return False

        My.Application.Log.WriteEntry(String.Format("Including Definition: {0}", thisWords(1)), TraceEventType.Information)

        Dim myFileNames As List(Of String) = thisWords(1).Replace("/", cPathSplitChar(0)).Split(cPathSplitChar, StringSplitOptions.RemoveEmptyEntries).ToList
        If myFileNames.Contains(".") Then
            myFileNames.Remove(".")
        End If
        Dim myFilename As String = String.Format("{0}{2}{1}", My.Settings.DefaultPath, Join(myFileNames.ToArray, cPathSplitChar(0)), cPathSplitChar(0))
        If IO.File.Exists(myFilename) Then
            My.Application.Log.WriteEntry(String.Format("Load Configfile: {0}", myFilename), TraceEventType.Verbose)
            Dim myIncStream As IO.StreamReader = IO.File.OpenText(myFilename)
            Return ReadCfgStream(myIncStream, ExtractCfgFileName(myFilename))
        End If
        Return False
    End Function

    Private Function ProcessFhemDefine(ByVal thisWords As List(Of String), ByRef thisStream As IO.StreamReader, ByVal thisCfgFile As String, ByRef thisUserComments As Text.StringBuilder) As Boolean?
        If thisWords Is Nothing Then Return False
        If thisStream Is Nothing Then Return False
        If thisCfgFile Is Nothing Then Return False
        If thisUserComments Is Nothing Then Return False

        My.Application.Log.WriteEntry(String.Format("Defining: ""{0}"" as ""{1}""", thisWords(1), thisWords(2)), TraceEventType.Information)

        Dim myDef As New Text.StringBuilder
        If Not ProcessFhemDefinition(myDef, thisWords, thisStream) Then Return False

        Dim myObj As FHEMObj = New FHEMObj() With {.Action = thisWords(0),
                                                   .Name = thisWords(1),
                                                   .FHEMType = thisWords(2),
                                                   .Definition = myDef.ToString,
                                                   .ConfigFileName = thisCfgFile,
                                                   .UserComment = thisUserComments.ToString}
        thisUserComments.Clear()

        If Not AddFhemObject(myObj) Then Return False

        If myObj.FHEMType = "FileLog" Then
            If Not ProcessFhemDefineLog(thisWords, myObj) Then Return False
        ElseIf myObj.FHEMType = "SVG" Then
            If Not ProcessFhemDefineSvg(thisWords, myObj) Then Return False
        End If

        myDef.Clear()

        Return True
    End Function

    Private Function ProcessFhemDefineLog(ByVal thisWords As List(Of String), ByVal thisObj As FHEMObj) As Boolean?
        If thisWords Is Nothing Then Return False
        If thisObj Is Nothing Then Return False

        Dim funcFindFromLog = Function(x As FHEMObj) x.Name = thisWords(4).Split(":".ToCharArray, StringSplitOptions.RemoveEmptyEntries)(0) _
                                  AndAlso Not x.Logs.Contains(thisObj)

        If Me.AllObjectsList.Exists(funcFindFromLog) Then
            Dim myObj As FHEMObj = Me.AllObjectsList.Find(funcFindFromLog)
            myObj.Logs.Add(thisObj)
            myObj.Associations.Add(thisObj)
        End If
        Return True
    End Function

    Private Function ProcessFhemDefineSvg(ByVal thisWords As List(Of String), ByVal thisObj As FHEMObj) As Boolean?
        If thisWords Is Nothing Then Return False
        If thisObj Is Nothing Then Return False

        Dim funcFindFromPlot = Function(x As FHEMObj) (x.Name = thisWords(3).Split(":".ToCharArray, StringSplitOptions.RemoveEmptyEntries)(0)) _
                                   AndAlso (x.FHEMType = "FileLog") _
                                   AndAlso Not (x.SVGPlots.Contains(thisObj))

        If Me.AllObjectsList.Exists(funcFindFromPlot) Then
            Dim myObj As FHEMObj = Me.AllObjectsList.Find(funcFindFromPlot)
            myObj.SVGPlots.Add(thisObj)
            myObj.Associations.Add(thisObj)
        End If
        Return True
    End Function

    Private Function ProcessFhemAttribute(ByVal thisWords As List(Of String)) As Boolean?
        If thisWords Is Nothing Then Return False

        Dim funcFindByName = Function(x As FHEMObj) x.Name = thisWords(1)

        If Me.AllObjectsList.Exists(funcFindByName) Then
            Dim myObj As FHEMObj = Me.AllObjectsList.Find(funcFindByName)

            Dim myValues As New List(Of String)
            Try
                If thisWords.Count > 3 Then
                    If thisWords(3).Contains(cAttributeListSplitChar(0)) Then
                        myValues.AddRange(thisWords(3).Split(cAttributeListSplitChar, StringSplitOptions.RemoveEmptyEntries))
                    Else
                        myValues.Add(Join(thisWords.GetRange(3, thisWords.Count - 3).ToArray, cWordSplitChar(0)))
                    End If
                End If
            Catch ex As Exception

            End Try

            If Not myObj.Attributes.Contains(thisWords(2)) Then
                myObj.Attributes.Add(thisWords(2), myValues)
                Return True
            Else
                myObj.Attributes(thisWords(2)) = myValues
                Return True
            End If
            Return True
        End If

        Return False
    End Function

    Private Shared Function ProcessFhemDefinition(ByRef thisDef As Text.StringBuilder, ByVal thisWords As List(Of String), ByRef thisStream As IO.StreamReader) As Boolean?
        If thisDef Is Nothing Then Return False
        If thisWords Is Nothing Then Return False
        If thisStream Is Nothing Then Return False

        If thisWords.Last.Equals("{\") Or thisWords.Last.Equals("{\\") Then 'Notify
            If Not thisWords(3) = thisWords.Last Then
                thisDef.Append(Join(thisWords.GetRange(3, thisWords.Count - 3 - 1).ToArray, " "))
                thisDef.Append(" ")
            End If
            thisDef.AppendLine(thisWords.Last)

            Dim myBrckCnt As Int16 = 1
            Do While myBrckCnt > 0 And thisStream.Peek > -1
                Dim myChar As Char = Chr(thisStream.Read)
                If myChar = "{" Then
                    myBrckCnt += 1
                ElseIf myChar = "}" Then
                    myBrckCnt -= 1
                End If
                thisDef.Append(myChar)
            Loop
        ElseIf thisWords.Last.Equals("\") Then 'DOIF
            If Not thisWords(3) = thisWords.Last Then
                thisDef.Append(Join(thisWords.GetRange(3, thisWords.Count - 3 - 1).ToArray, " "))
                thisDef.Append(" ")
            End If
            thisDef.AppendLine(thisWords.Last)

            Dim myExitLoop As Boolean = False
            Do While myExitLoop = False And thisStream.Peek > -1
                Dim myLine As String = thisStream.ReadLine

                If myLine.EndsWith("\") Then
                    thisDef.AppendLine(myLine)
                Else
                    thisDef.Append(myLine)
                    myExitLoop = True
                End If
            Loop
        Else
            If thisWords.Count > 2 Then
                thisDef.Append(Join(thisWords.GetRange(3, thisWords.Count - 3).ToArray, " "))
            Else
                Return False
            End If
        End If

        thisDef = thisDef.Replace("\\", "\")

        If thisWords(2).Equals("DOIF") AndAlso thisDef.ToString.Contains(" DOELSE") Then
            thisDef = thisDef.Replace(" DOELSE", " \" & vbCrLf & "DOELSE")
        End If

        Return True
    End Function

    Private Function BuildAssociations() As Boolean?
        For Each myObj As FHEMObj In Me.CurrentObjects.ToList
            Dim myAssociations = Function(x As FHEMObj) _
                                     Not (String.IsNullOrEmpty(x.Definition)) _
                                     AndAlso Not ((x.FHEMType = "FileLog") Or (x.FHEMType = "SVG")) _
                                     AndAlso ((x.Definition.Contains(myObj.Name)) Or (myObj.Definition.Contains(x.Name))) _
                                     AndAlso (Not myObj.Associations.Contains(x)) _
                                     AndAlso Not (x.Name = myObj.Name)
            For Each myAssObj As FHEMObj In Me.CurrentObjects.Where(myAssociations)
                Select Case myObj.FHEMType
                    Case "global" ', "readingsProxy"
                        If Not myObj.Associations.Contains(myAssObj) Then myObj.Associations.Add(myAssObj)
                    Case "FileLog"
                        If myObj.Name.EndsWith(myAssObj.Name) Then
                            If Not myAssObj.Associations.Contains(myObj) Then myAssObj.Associations.Add(myObj)
                        End If
                    Case "SVG"

                    Case Else
                        If Not myAssObj.Associations.Contains(myObj) Then myAssObj.Associations.Add(myObj)
                End Select
            Next
        Next
        Return True
    End Function

    Private Function AddFhemObject(ByVal thisObj As FHEMObj) As Boolean?
        If thisObj Is Nothing Then Return False

        Dim funcFindByName = Function(x) x.Name = thisObj.Name
        If Me.AllObjectsList.Exists(funcFindByName) Then
            Me.CurrentObjects(Me.AllObjectsList.FindIndex(funcFindByName)) = thisObj
            Return True
        Else
            Me.CurrentObjects.Add(thisObj)
            Return True
        End If
        Return False
    End Function

    Private Shared Function ExtractCfgFileName(ByVal thisPath As String) As String
        If String.IsNullOrEmpty(thisPath) Then Return Nothing

        Return thisPath.Replace(String.Format("{0}{1}", My.Settings.DefaultPath, cPathSplitChar(0)), String.Empty)
    End Function
#End Region

#Region "Save Methods"
    Public Enum SaveStyle
        DefaultStyle
        'SaveByConfig
        'SaveByGroup
        'SaveByRoom
    End Enum
    Public Function SaveCfg() As Boolean?
        My.Application.Log.WriteEntry(String.Format("Save using default config: {0}{2}{1}", My.Settings.DefaultPath, My.Settings.DefaultFileName, cPathSplitChar(0)), TraceEventType.Information)

        Return Me.SaveCfg(My.Settings.DefaultSavePath, My.Settings.DefaultFileName)
    End Function
    Public Function SaveCfg(ByVal thisSaveStyle As SaveStyle) As Boolean?
        My.Application.Log.WriteEntry(String.Format("Save using default config: {0}{2}{1}", My.Settings.DefaultPath, My.Settings.DefaultFileName, cPathSplitChar(0)), TraceEventType.Information)

        Return Me.SaveCfg(My.Settings.DefaultSavePath, My.Settings.DefaultFileName, thisSaveStyle)
    End Function

    Public Function SaveCfg(ByVal thisCfgPathAndFileName As String, ByVal thisOverwrite As Boolean) As Boolean?
        If String.IsNullOrEmpty(thisCfgPathAndFileName) Then Return False

        Dim myFilename As String = thisCfgPathAndFileName.Split(cPathSplitChar).Last
        Dim myPath As String = thisCfgPathAndFileName.Remove(thisCfgPathAndFileName.IndexOf(String.Format("{0}{1}", cPathSplitChar(0), myFilename)))

        My.Application.Log.WriteEntry(String.Format("Save using config: {0}{2}{1}", myPath, myFilename, cPathSplitChar(0)), TraceEventType.Information)
        If IO.Directory.Exists(myPath) Then
            If (IO.File.Exists(thisCfgPathAndFileName)) AndAlso (thisOverwrite) Then
                Return Me.SaveCfg(myPath, myFilename)
            ElseIf (Not IO.File.Exists(thisCfgPathAndFileName)) Then
                Return Me.SaveCfg(myPath, myFilename)
            Else
                My.Application.Log.WriteEntry("Abort!", TraceEventType.Information)
            End If
        Else
            My.Application.Log.WriteEntry(String.Format("Directory not found: {0}", myPath), TraceEventType.Error)
            Return False
        End If
        Return False
    End Function
    Public Function SaveCfg(ByVal thisPath As String, ByVal thisCfgFileName As String) As Boolean?
        If String.IsNullOrEmpty(thisPath) Then Return False
        If String.IsNullOrEmpty(thisCfgFileName) Then Return False

        Return SaveCfg(thisPath, thisCfgFileName, SaveStyle.DefaultStyle)
    End Function

    Public Function SaveCfg(ByVal thisPath As String, ByVal thisCfgFileName As String, ByVal thisSaveStyle As SaveStyle) As Boolean?
        If String.IsNullOrEmpty(thisPath) Then Return False
        If String.IsNullOrEmpty(thisCfgFileName) Then Return False

        Dim myFullName As String = String.Format("{0}{2}{1}", thisPath, thisCfgFileName, cPathSplitChar(0))
        If Not IO.Directory.Exists(thisPath) Then
            IO.Directory.CreateDirectory(thisPath)
        End If
        If IO.File.Exists(myFullName) Then
            My.Application.Log.WriteEntry(String.Format("Moving: {0}", thisCfgFileName), TraceEventType.Information)
            IO.File.Move(myFullName, String.Format("{0}{1}", myFullName, DateTime.Now.ToString("yyyyMMdd_HHmmss")))
        End If

        Dim myMemStream As New IO.MemoryStream
        Using myOutput As IO.TextWriter = New IO.StreamWriter(myMemStream)
            Try
                If SaveCfgStream(myOutput, thisSaveStyle) Then
                    myOutput.Flush()
                    Helper.StreamToNewFile(myMemStream, myFullName)
                    Return True
                End If
            Catch ex As Exception
                My.Application.Log.WriteException(ex, TraceEventType.Error, ex.StackTrace)
                Return False
            End Try
        End Using
        If myMemStream IsNot Nothing Then myMemStream.Dispose()
        Return False
    End Function

    Public Function SaveCfgStream(ByRef thisStream As IO.StreamWriter, ByVal thisSaveStyle As SaveStyle) As Boolean?
        If thisStream Is Nothing Then Return False

        Dim myObjects As List(Of FHEMObj) = Me.AllObjectsList.ToList

        If Not SaveDefaultCfgObjects(myObjects, thisStream) Then Return False

        Select Case thisSaveStyle
            Case SaveStyle.DefaultStyle
                If Not SaveCfgStreamStyleDefault(thisStream, myObjects) Then Return False
                'Case SaveStyle.SaveByConfig
                '    If Not SaveCfgStreamStyleConfig(thisStream, myObjects) Then Return False
                'Case SaveStyle.SaveByGroup
                '    If Not SaveCfgStreamStyleGroup(thisStream, myObjects) Then Return False
                'Case SaveStyle.SaveByRoom
                '    If Not SaveCfgStreamStyleRoom(thisStream, myObjects) Then Return False
            Case Else
                Return False
        End Select

        thisStream.WriteLine("# [{0,-20}{1,-40}]", "BEGIN TYPE:", "other")
        For Each myObj In myObjects.OrderBy(Function(x) x.Name).ToList
            If Not SaveCfgObject(myObj, myObjects, thisStream) Then Return False
            thisStream.WriteLine()
        Next
        thisStream.WriteLine("# [{0,-20}{1,-40}]", "END TYPE:", "other")
        thisStream.WriteLine()
        thisStream.WriteLine("# [{0,-20}{1,-40}]", "BEGIN TYPE:", "AUTOCREATE")
        thisStream.WriteLine()

        Return True
    End Function

    Private Shared Function SaveCfgStreamStyleDefault(ByRef thisStream As IO.StreamWriter, ByRef thisObjects As List(Of FHEMObj)) As Boolean?
        If thisStream Is Nothing Then Return False
        If thisObjects Is Nothing Then Return False

        Dim myGroups = thisObjects.GroupBy(Function(x) New With {Key x.FHEMType}).Select(Function(x) x.Key.FHEMType).Distinct.ToList
        For Each myGroup In myGroups.Where(Function(x) Not (x = "FileLog" Or x = "SVG")).ToList
            Dim mySubObjects = thisObjects.Where(Function(x) x.FHEMType = myGroup).OrderBy(Function(x) x.Name).ToList
            If mySubObjects.Count > 0 Then
                thisStream.WriteLine()
                thisStream.WriteLine("# [{0,-20}{1,-40}]", "BEGIN TYPE:", myGroup)

                If Not SaveCfgStreamSubObjects(thisStream, thisObjects, mySubObjects) Then Return False

                thisStream.WriteLine("# [{0,-20}{1,-40}]", "END TYPE:", myGroup)
                thisStream.WriteLine()
            End If
        Next
        Return True
    End Function

    'Private Shared Function SaveCfgStreamStyleConfig(ByRef thisStream As IO.StreamWriter, ByRef thisObjects As List(Of FHEMObj)) As Boolean?
    '    Dim myGroups = thisObjects.GroupBy(Function(x) New With {Key x.ConfigFileName}).Select(Function(x) x.Key.ConfigFileName).Distinct().OrderBy(Function(x) x).ToList
    '    thisStream.WriteLine()
    '    For Each myGroup In myGroups
    '        Dim myStream As IO.StreamWriter
    '        If Not String.IsNullOrEmpty(myGroup) Then
    '            thisStream.WriteLine("# [{0,-20}{1,-40}]", "INCLUDE_BY_CONFIG:", myGroup)

    '            Dim myIncludeFileString As String = String.Format("./{0}/{1}.cfg", My.Settings.DefaultIncludeSaveDirectoryName, myGroup)
    '            thisStream.WriteLine("include {0}", myIncludeFileString)

    '            Dim myIncludeFileName As String = String.Format("{0}{3}{1}{3}{2}.cfg", My.Settings.DefaultSavePath, My.Settings.DefaultIncludeSaveDirectoryName, myGroup, cPathSplitChar(0))
    '            myStream = IO.File.CreateText(myIncludeFileName)
    '        Else
    '            myStream = thisStream
    '        End If

    '        Dim mySubObjects = thisObjects.Where(Function(x) x.ConfigFileName = myGroup).OrderBy(Function(x) x.Name).ToList
    '        If Not SaveCfgStreamSubObjects(myStream, thisObjects, mySubObjects) Then Return False
    '        thisStream.WriteLine()
    '    Next
    '    Return True
    'End Function

    'Private Shared Function SaveCfgStreamStyleGroup(ByRef thisStream As IO.StreamWriter, ByRef thisObjects As List(Of FHEMObj)) As Boolean?
    '    Dim myGroups = thisObjects.GroupBy(Function(x) New With {Key x.Group}).Select(Function(x) x.Key.Group).Distinct().OrderBy(Function(x) x).ToList
    '    thisStream.WriteLine()
    '    For Each myGroup In myGroups
    '        Dim myStream As IO.StreamWriter
    '        If Not String.IsNullOrEmpty(myGroup) Then
    '            thisStream.WriteLine("# [{0,-20}{1,-40}]", "INCLUDE_BY_GROUP:", myGroup)

    '            Dim myIncludeFileString As String = String.Format("./{0}/{1}.cfg", My.Settings.DefaultIncludeSaveDirectoryName, myGroup)
    '            thisStream.WriteLine("include {0}", myIncludeFileString)

    '            Dim myIncludeFileName As String = String.Format("{0}{3}{1}{3}{2}.cfg", My.Settings.DefaultSavePath, My.Settings.DefaultIncludeSaveDirectoryName, myGroup, cPathSplitChar(0))
    '            myStream = IO.File.CreateText(myIncludeFileName)
    '        Else
    '            myStream = thisStream
    '        End If

    '        Dim mySubObjects = thisObjects.Where(Function(x) x.Group = myGroup).OrderBy(Function(x) x.Name).ToList
    '        If Not SaveCfgStreamSubObjects(myStream, thisObjects, mySubObjects) Then Return False
    '        thisStream.WriteLine()
    '    Next
    '    Return True
    'End Function

    'Private Shared Function SaveCfgStreamStyleRoom(ByRef thisStream As IO.StreamWriter, ByRef thisObjects As List(Of FHEMObj)) As Boolean?
    '    Dim myGroups = thisObjects.GroupBy(Function(x) New With {Key x.Room}).Select(Function(x) x.Key.Room).Distinct().OrderBy(Function(x) x).ToList
    '    thisStream.WriteLine()
    '    For Each myGroup In myGroups
    '        Dim myStream As IO.StreamWriter
    '        If Not String.IsNullOrEmpty(myGroup) Then
    '            thisStream.WriteLine("# [{0,-20}{1,-40}]", "INCLUDE_BY_ROOM:", myGroup)

    '            Dim myIncludeFileString As String = String.Format("./{0}/{1}.cfg", My.Settings.DefaultIncludeSaveDirectoryName, myGroup)
    '            thisStream.WriteLine("include {0}", myIncludeFileString)

    '            Dim myIncludeFileName As String = String.Format("{0}{3}{1}{3}{2}.cfg", My.Settings.DefaultSavePath, My.Settings.DefaultIncludeSaveDirectoryName, myGroup, cPathSplitChar(0))
    '            myStream = IO.File.CreateText(myIncludeFileName)
    '        Else
    '            myStream = thisStream
    '        End If

    '        Dim mySubObjects = thisObjects.Where(Function(x) x.Room = myGroup).OrderBy(Function(x) x.Name).ToList
    '        If Not SaveCfgStreamSubObjects(myStream, thisObjects, mySubObjects) Then Return False
    '        thisStream.WriteLine()
    '    Next
    '    Return True
    'End Function

    Private Shared Function SaveDefaultCfgObjects(ByRef thisObjects As List(Of FHEMObj), ByRef thisStream As IO.StreamWriter) As Boolean?
        If thisStream Is Nothing Then Return False
        If thisObjects Is Nothing Then Return False

        If Not SaveCfgObjectByFHEMType("global", thisObjects, thisStream) Then Return False

        thisStream.WriteLine()
        thisStream.WriteLine("# [{0,-20}{1,-40}]", "BEGIN CONFIG:", "DEFAULT")

        If Not SaveCfgObjectByName("Logfile", thisObjects, thisStream) Then Return False
        thisStream.WriteLine()
        If Not SaveCfgObjectByFHEMType("telnet", thisObjects, thisStream) Then Return False
        thisStream.WriteLine()
        If Not SaveCfgObjectByFHEMType("autocreate", thisObjects, thisStream) Then Return False
        thisStream.WriteLine()
        If Not SaveCfgObjectByName("initialUsbCheck", thisObjects, thisStream) Then Return False
        thisStream.WriteLine()

        Dim myObjects = thisObjects.Where(Function(x) x.FHEMType = "FHEMWEB").OrderBy(Function(x) x.Name).ToList
        For Each myObj In myObjects
            If Not SaveCfgObject(myObj, thisObjects, thisStream) Then Return False
            If Not myObj.Equals(myObjects.Last) Then thisStream.WriteLine()
        Next

        thisStream.WriteLine("# [{0,-20}{1,-40}]", "END CONFIG:", "DEFAULT")
        thisStream.WriteLine()

        Return True
    End Function

    Private Shared Function SaveCfgObjectByName(ByVal thisObjectName As String, ByRef thisObjects As List(Of FHEMObj), ByRef thisStream As IO.StreamWriter) As Boolean?
        If String.IsNullOrEmpty(thisObjectName) Then Return False
        If thisObjects Is Nothing Then Return False
        If thisStream Is Nothing Then Return False

        Dim funcFindByName = Function(x As FHEMObj) x.Name = thisObjectName
        If thisObjects.Exists(funcFindByName) Then
            Return SaveCfgObject(thisObjects.Find(funcFindByName), thisObjects, thisStream)
        End If

        Return False
    End Function

    Private Shared Function SaveCfgObjectByFHEMType(ByVal thisFhemType As String, ByRef thisObjects As List(Of FHEMObj), ByRef thisStream As IO.StreamWriter) As Boolean?
        If String.IsNullOrEmpty(thisFhemType) Then Return False
        If thisObjects Is Nothing Then Return False
        If thisStream Is Nothing Then Return False

        Dim funcFindByFhemType = Function(x As FHEMObj) x.FHEMType = thisFhemType
        If thisObjects.Exists(funcFindByFhemType) Then
            Return SaveCfgObject(thisObjects.Find(funcFindByFhemType), thisObjects, thisStream)
        End If

        Return False
    End Function

    Private Shared Function SaveCfgStreamSubObjects(ByRef thisStream As IO.StreamWriter, ByRef thisObjects As List(Of FHEMObj), ByVal thisSubObjects As List(Of FHEMObj)) As Boolean?
        If thisStream Is Nothing Then Return False
        If thisObjects Is Nothing Then Return False
        If thisSubObjects Is Nothing Then Return False

        For Each myObj In thisSubObjects
            If Not SaveCfgObject(myObj, thisObjects, thisStream) Then Return False
            If Not myObj.Equals(thisSubObjects.Last) Then thisStream.WriteLine()
        Next
        Return True
    End Function

    Private Shared Function SaveCfgObject(ByVal thisObject As FHEMObj, ByRef thisObjects As List(Of FHEMObj), ByRef thisStream As IO.StreamWriter) As Boolean?
        If thisObject Is Nothing Then Return False
        If thisObjects Is Nothing Then Return False
        If thisStream Is Nothing Then Return False

        If thisObjects.Contains(thisObject) Then
            thisStream.WriteLine(thisObject.GetCfgDefineString)

            If thisObject.Logs.Count > 0 Then
                thisStream.WriteLine("# -LogFile")
                For Each myLog In thisObject.Logs.OrderBy(Function(x) x.Name).ToList
                    thisStream.WriteLine(myLog.GetCfgDefineString)
                    If myLog.SVGPlots.Count > 0 Then
                        thisStream.WriteLine("# -SVGPlots")
                        For Each mySVGPlot In myLog.SVGPlots.OrderBy(Function(x) x.Name).ToList
                            thisStream.WriteLine(mySVGPlot.GetCfgDefineString)
                            If Not thisObjects.Remove(mySVGPlot) Then Return False
                        Next
                    End If
                    If Not thisObjects.Remove(myLog) Then Return False
                Next
            End If

            Return thisObjects.Remove(thisObject)
        End If

        Return False
    End Function
#End Region
End Class
