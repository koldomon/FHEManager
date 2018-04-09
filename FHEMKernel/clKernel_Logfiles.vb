Imports MM.FHEMManager.FHEMObjects
Partial Class FHEMKernel

#Region "Backup Logfiles"
    Public Shared Function BackupLogs() As Boolean
        My.Application.Log.WriteEntry(String.Format("BackupLogs using default config: {0}, {1}", My.Settings.DefaultLogPath, My.Settings.DefaultLogBackupPath), TraceEventType.Information)

        Return BackupLogs(My.Settings.DefaultLogPath, My.Settings.DefaultLogBackupPath)
    End Function
    Public Shared Function BackupLog(ByVal thisSearchMask As String) As Boolean
        If String.IsNullOrEmpty(thisSearchMask) Then Return False

        My.Application.Log.WriteEntry(String.Format("BackupLog using default config: {0}, {1}, {2}", My.Settings.DefaultLogPath, My.Settings.DefaultLogBackupPath, thisSearchMask), TraceEventType.Information)

        Return BackupLogs(My.Settings.DefaultLogPath, My.Settings.DefaultLogBackupPath, thisSearchMask)
    End Function
    Public Shared Function BackupLogs(ByVal thisSourceFolder As String, ByVal thisBackupFolder As String) As Boolean
        If String.IsNullOrEmpty(thisSourceFolder) Then Return False
        If String.IsNullOrEmpty(thisBackupFolder) Then Return False

        Return BackupLogs(thisSourceFolder, thisBackupFolder, String.Empty)
    End Function
    Public Shared Function BackupLogs(ByVal thisSourceFolder As String, ByVal thisBackupFolder As String, ByVal thisSearchMask As String) As Boolean
        If String.IsNullOrEmpty(thisSourceFolder) Then Return False
        If String.IsNullOrEmpty(thisBackupFolder) Then Return False

        If Not (IO.Directory.Exists(thisSourceFolder)) Then
            Return False
        End If

        If Not IO.Directory.Exists(thisBackupFolder) Then
            My.Application.Log.WriteEntry(String.Format("Create Directory: {0}", thisBackupFolder), TraceEventType.Information)
            IO.Directory.CreateDirectory(thisBackupFolder)
        End If

        Dim myDirFileNames As List(Of String)
        If String.IsNullOrEmpty(thisSearchMask) Then
            myDirFileNames = IO.Directory.GetFiles(thisSourceFolder, "*.log").OrderBy(Function(x) x).ToList
        Else
            myDirFileNames = IO.Directory.GetFiles(thisSourceFolder, String.Format("{0}.log", thisSearchMask)).OrderBy(Function(x) x).ToList
        End If

        For Each myDirFileName As String In myDirFileNames
            Dim myReturn As Boolean = True
            myReturn = BackupLog(myDirFileName, thisBackupFolder)
            If Not myReturn Then Return False
        Next

        Return True
    End Function

    Public Shared Function BackupLog(ByVal thisLogFileName As String, ByVal thisBackupFolder As String) As Boolean
        If Not IO.File.Exists(thisLogFileName) Then Return False
        If Not IO.Directory.Exists(thisBackupFolder) Then Return False

        Dim myLogObjects As New List(Of FHEMLogEntry)
        Dim myFileName As String = ExtractFileName(thisLogFileName)

        My.Application.Log.WriteEntry(String.Format("Processing Log: {0}", myFileName), TraceEventType.Information)
        If ParseLogFile(thisLogFileName, myLogObjects) Then

            If Not LogToArchive(myLogObjects, myFileName, thisBackupFolder) Then Return False

            IO.File.Copy(thisLogFileName, String.Format("{0}\{1}_{2}.old", thisBackupFolder, myFileName, DateTime.Now.ToString("yyyyMMdd_HHmmss")))

            If Not LogToCurrentLog(myLogObjects, thisLogFileName) Then Return False
        Else
            My.Application.Log.WriteEntry(String.Format("Error Processing Log: {0}", thisLogFileName), TraceEventType.Warning)
            Return False
        End If

        Return True
    End Function

    Private Shared Function LogToCurrentLog(ByRef thisLogObjects As List(Of FHEMLogEntry), ByVal thisLogFileName As String) As Boolean
        If thisLogObjects Is Nothing Then Return False
        If String.IsNullOrEmpty(thisLogFileName) Then Return False

        Dim myMemStream = New IO.MemoryStream
        Dim myStopwatch As New Stopwatch

        Using myLogWriter As IO.TextWriter = New IO.StreamWriter(myMemStream)
            Try
                myStopwatch.Start()
                For Each myLogObject In thisLogObjects.OrderBy(Function(x) x.LogTime).ToList
                    myLogWriter.Write(myLogObject.Save)
                Next
                myLogWriter.Flush()
                thisLogObjects.Clear()

                myStopwatch.Stop()
                My.Application.Log.WriteEntry(String.Format("Preparing Logs took: {0}", myStopwatch.Elapsed.ToString("mm\:ss")), TraceEventType.Verbose)


                myStopwatch.Reset()
                myStopwatch.Start()

                Helper.StreamToNewFile(myMemStream, thisLogFileName)

                myStopwatch.Stop()
                My.Application.Log.WriteEntry(String.Format("Create LogFile: {0}", myStopwatch.Elapsed.ToString("mm\:ss")), TraceEventType.Verbose)
            Catch ex As Exception
                My.Application.Log.WriteException(ex, TraceEventType.Error, ex.StackTrace)
                Return False
            Finally
                myLogWriter.Flush()
            End Try
        End Using

        If myMemStream IsNot Nothing Then myMemStream.Dispose()

        Return True
    End Function
    Private Shared Function LogToArchive(ByRef thisLogObjects As List(Of FHEMLogEntry), ByVal myFileName As String, ByVal thisBackupFolder As String) As Boolean
        If thisLogObjects Is Nothing Then Return False
        If String.IsNullOrEmpty(myFileName) Then Return False
        If String.IsNullOrEmpty(thisBackupFolder) Then Return False

        Dim myBackupDirFileName As String = String.Format("{0}\{1}_backup.log", thisBackupFolder, myFileName)
        If Not IO.File.Exists(myBackupDirFileName) Then
            My.Application.Log.WriteEntry(String.Format("Creating Backup Log: {0}", myBackupDirFileName), TraceEventType.Information)

            Dim myTempFile = IO.File.CreateText(myBackupDirFileName)
            myTempFile.Dispose() 'or u cannot access the file later
        End If

        Dim myMemStream = New IO.MemoryStream
        Dim myStopwatch As New Stopwatch

        Using myBackupWriter As IO.TextWriter = New IO.StreamWriter(myMemStream)
            Try
                Dim myRemoveObjects As New List(Of FHEMLogEntry)

                myStopwatch.Reset()
                myStopwatch.Start()
                myRemoveObjects.Clear()
                For Each myLogObject In thisLogObjects.Where(Function(x As FHEMLogEntry) _
                                                                       (x.LogTime <= DateTime.Today.AddMonths(-1)) Or
                                                                       (x.LogLevel.HasValue AndAlso x.LogLevel.Value >= 4)
                                                                       ).OrderBy(Function(x) x.LogTime).ToList
                    myBackupWriter.Write(myLogObject.Save)
                    myRemoveObjects.Add(myLogObject)
                Next
                thisLogObjects = thisLogObjects.Except(myRemoveObjects).ToList
                myBackupWriter.Flush()


                myRemoveObjects.Clear()
                For Each myLogObject In thisLogObjects.Where(Function(x As FHEMLogEntry) _
                                                                       (x.LogText.ToString.StartsWith("PERL WARNING:") = True) Or
                                                                       (x.LogText.ToString.StartsWith("stacktrace:") = True) Or
                                                                       (x.LogText.ToString.Contains("main::") = True) Or
                                                                       (x.LogText.ToString.Contains("return value: syntax error at") = True) Or
                                                                       (x.LogText.ToString.StartsWith("eval:") = True) Or
                                                                       (x.LogText.ToString.StartsWith("192.168.1.205:2323 disconnected") = True) Or
                                                                       (x.LogText.ToString.StartsWith("192.168.1.205:2323 reappeared") = True) Or
                                                                       (x.LogText.ToString.StartsWith("CUNO_FS20: Possible commands:") = True) Or
                                                                       (x.LogText.ToString.StartsWith("Discard Player.OnSeek") = True)
                                                                       ).OrderBy(Function(x) x.LogTime).ToList
                    myBackupWriter.Write(myLogObject.Save)
                    myRemoveObjects.Add(myLogObject)
                Next
                thisLogObjects = thisLogObjects.Except(myRemoveObjects).ToList
                myBackupWriter.Flush()

                myRemoveObjects.Clear()

                myStopwatch.Stop()
                My.Application.Log.WriteEntry(String.Format("Prepare BackupLogs took: {0}", myStopwatch.Elapsed.ToString("mm\:ss")), TraceEventType.Verbose)

                myStopwatch.Reset()
                myStopwatch.Start()

                Helper.StreamAppendToFile(myMemStream, myBackupDirFileName)

                myStopwatch.Stop()
                My.Application.Log.WriteEntry(String.Format("Append to Backup took: {0}", myStopwatch.Elapsed.ToString("mm\:ss")), TraceEventType.Verbose)

            Catch ex As Exception
                My.Application.Log.WriteException(ex, TraceEventType.Error, ex.StackTrace)
                Return False
            End Try
        End Using

        If myMemStream IsNot Nothing Then myMemStream.Dispose()

        Return True
    End Function
#End Region

#Region "Join Logfiles"
    Public Shared Function JoinLog(ByVal thisLog As String) As Boolean
        If String.IsNullOrEmpty(thisLog) Then Return False

        My.Application.Log.WriteEntry(String.Format("JoinLogs using default config: {0}, {1}, {2}", My.Settings.DefaultLogPath, My.Settings.DefaultLogJoinPath, My.Settings.DefaultLogPath), TraceEventType.Information)

        Return JoinLogs(My.Settings.DefaultLogPath, My.Settings.DefaultLogJoinPath, My.Settings.DefaultLogPath, thisLog)
    End Function

    Public Shared Function JoinLogs() As Boolean
        My.Application.Log.WriteEntry(String.Format("JoinLogs using default config: {0}, {1}, {2}", My.Settings.DefaultLogPath, My.Settings.DefaultLogJoinPath, My.Settings.DefaultLogPath), TraceEventType.Information)

        Return JoinLogs(My.Settings.DefaultLogPath, My.Settings.DefaultLogJoinPath, My.Settings.DefaultLogPath)
    End Function

    Private Shared Function JoinLogs(ByVal thisSourceFolder As String, ByVal thisJoinFolder As String, ByVal thisOutFolder As String, Optional ByVal thisLogName As String = "") As Boolean
        If String.IsNullOrEmpty(thisSourceFolder) Then Return False
        If String.IsNullOrEmpty(thisJoinFolder) Then Return False
        If String.IsNullOrEmpty(thisOutFolder) Then Return False

        If Not (IO.Directory.Exists(thisSourceFolder) Or IO.Directory.Exists(thisJoinFolder)) Then
            Return False
        End If

        If Not IO.Directory.Exists(thisJoinFolder & "\joined") Then
            My.Application.Log.WriteEntry(String.Format("Create Directory: {0}\joined", thisJoinFolder), TraceEventType.Information)
            IO.Directory.CreateDirectory(thisJoinFolder & "\joined")
        End If


        If Not IO.Directory.Exists(thisOutFolder) Then
            My.Application.Log.WriteEntry(String.Format("Create Directory: {0}", thisOutFolder), TraceEventType.Information)
            IO.Directory.CreateDirectory(thisOutFolder)
        End If

        Dim myOutFolder As New IO.DirectoryInfo(thisOutFolder)

        Dim myDirFileNames As List(Of String)
        If String.IsNullOrEmpty(thisLogName) Then
            myDirFileNames = IO.Directory.GetFiles(thisSourceFolder, "*.log").OrderBy(Function(x) x).ToList
        Else
            myDirFileNames = IO.Directory.GetFiles(thisSourceFolder, String.Format("{0}.log", thisLogName)).OrderBy(Function(x) x).ToList
        End If

        For Each myDirFileName As String In myDirFileNames
            Dim myLogObjects As New List(Of FHEMLogEntry)
            Dim myFileName As String = ExtractFileName(myDirFileName)
            Dim myJoinFiles = IO.Directory.GetFiles(thisJoinFolder, String.Format("*{0}*.log", myFileName), IO.SearchOption.TopDirectoryOnly).ToList
            Dim myOutFileName As String = String.Format("{0}{2}{1}.log", myOutFolder, myFileName, cPathSplitChar(0))

            My.Application.Log.WriteEntry(String.Format("Processing Log: {0}", myFileName), TraceEventType.Information)
            If ParseLogFile(myDirFileName, myLogObjects) Then
                Dim myJoinLogError As Boolean = False
                For Each myJoinDirFileName In myJoinFiles
                    My.Application.Log.WriteEntry(String.Format("Joining Log: {0}", myJoinDirFileName), TraceEventType.Information)
                    If Not ParseLogFile(myJoinDirFileName, myLogObjects) Then
                        myJoinLogError = True
                        My.Application.Log.WriteEntry(String.Format("Error Processing JoinLog: {0}", myJoinDirFileName), TraceEventType.Warning)
                        Exit For
                    End If
                Next
                If Not myJoinLogError Then
                    For Each myJoinDirFileName In myJoinFiles
                        Dim myJoinFileName As String = ExtractFileName(myJoinDirFileName)
                        IO.File.Move(myJoinDirFileName, String.Format("{0}\joined\{1}.log", thisJoinFolder, myJoinFileName))
                    Next

                    If IO.File.Exists(myOutFileName) Then
                        My.Application.Log.WriteEntry(String.Format("Moving old OutputLog: {0}", myOutFileName), TraceEventType.Information)
                        Dim myOldFileName As String = myOutFileName
                        myOldFileName = myOldFileName.Replace(".log", "")
                        myOldFileName = myOldFileName.Replace(".txt", "")
                        IO.File.Move(myOutFileName, String.Format("{0}_{1}.old", myOldFileName, DateTime.Now.ToString("yyyyMMdd_HHmmss")))
                    End If
                    My.Application.Log.WriteEntry(String.Format("Create OutputLog: {0}", myOutFileName), TraceEventType.Information)
                    Using myOutStream As IO.TextWriter = IO.File.CreateText(myOutFileName)
                        Try
                            myLogObjects = myLogObjects.Distinct().OrderBy(Function(x As FHEMLogEntry) x.LogTime).ToList
                            My.Application.Log.WriteEntry(String.Format("Writing LogEntries: {0:00,00#}", myLogObjects.Count), TraceEventType.Information)
                            For Each myLogObj In myLogObjects
                                myOutStream.Write(myLogObj.Save)
                            Next
                        Catch ex As Exception
                            My.Application.Log.WriteException(ex, TraceEventType.Error, ex.StackTrace)
                            Return False
                        Finally
                            myOutStream.Flush()
                        End Try
                    End Using
                End If
            Else
                My.Application.Log.WriteEntry(String.Format("Error Processing Log: {0}", myDirFileName), TraceEventType.Warning)
            End If
        Next

        Return True
    End Function
#End Region

#Region "Parse LogFiles"
    Private Shared Function ParseLogFile(ByVal thisFileName As String, ByRef thisLogObjects As List(Of FHEMLogEntry)) As Boolean
        If String.IsNullOrEmpty(thisFileName) Then Return False
        If thisLogObjects Is Nothing Then Return False

        Dim myMemStream As IO.MemoryStream = Helper.FileToMemStream(thisFileName)
        Using myReader As New IO.StreamReader(myMemStream)
            Try
                If myReader.Peek > -1 Then
                    If Not ParseLogStream(myReader, thisLogObjects) Then Return False
                End If
            Catch ex As Exception
                My.Application.Log.WriteException(ex, TraceEventType.Error, ex.StackTrace)
                Return False
            End Try
        End Using

        If myMemStream IsNot Nothing Then myMemStream.Dispose()

        Return True
    End Function

    Private Shared Function ParseLogStream(ByRef thisStream As IO.StreamReader, ByRef thisLogObjects As List(Of FHEMLogEntry)) As Boolean
        If thisStream Is Nothing Then Return False
        If thisLogObjects Is Nothing Then Return False

        Dim myLine As String = thisStream.ReadLine()
        Dim myLogObj As FHEMLogEntry = Nothing

        While thisStream.Peek > -1
            If IsNumeric(Left(myLine, 4)) Then
                If ParseLogLine(myLine, myLogObj) Then
                    thisLogObjects.Add(myLogObj)
                Else
                    My.Application.Log.WriteEntry(String.Format("Parsing Error: {0}", myLine))
                End If
                myLine = thisStream.ReadLine
            ElseIf myLine.StartsWith("Use of uninitialized value") _
                Or myLine.StartsWith("Scalar found where operator expected") _
                Or myLine.Contains("(Missing operator") _
                Or myLine.StartsWith("syswrite() on closed filehandle") Then
                myLine = thisStream.ReadLine
            Else
                If myLogObj IsNot Nothing Then myLogObj.Logline.AppendLine(myLine)

                myLine = thisStream.ReadLine
            End If
        End While
        Return True
    End Function

    Private Shared Function ParseLogLine(ByVal thisLine As String, ByRef thisObj As FHEMLogEntry) As Boolean
        If String.IsNullOrEmpty(thisLine) Then Return False

        Dim myWords() As String = thisLine.Split(New Char() {" "}, StringSplitOptions.RemoveEmptyEntries)

        If myWords.Count > 0 Then
            Dim myLogDate As DateTime
            If DateTime.TryParseExact(myWords(0), "yyyy\-MM\-dd\_HH:mm:ss", Nothing, Globalization.DateTimeStyles.AssumeLocal, myLogDate) Then
                thisObj = New FHEMLogEntry With {.LogTime = myLogDate,
                                                 .LogObj = myWords(1)
                                                }
                thisObj.Logline.AppendLine(thisLine)
                thisObj.AppendLogText(Join(myWords.ToList.GetRange(1, myWords.Count - 1).ToArray, " "))
                Return True
            ElseIf DateTime.TryParseExact(Join({myWords(0), myWords(1)}, " "), "yyyy\.MM\.dd HH:mm:ss", Nothing, Globalization.DateTimeStyles.AssumeLocal, myLogDate) Then
                Dim myLogLevel As Integer = -1
                If Integer.TryParse(myWords(2).Trim(New Char() {":"}), myLogLevel) Then
                    thisObj = New FHEMLogEntry With {.LogTime = myLogDate,
                                                     .LogLevel = myLogLevel
                                                    }
                    thisObj.Logline.AppendLine(thisLine)
                    thisObj.AppendLogText(Join(myWords.ToList.GetRange(3, myWords.Count - 3).ToArray, " "))
                    Return True
                End If
            End If
        End If
        Return False
    End Function
#End Region

    Private Shared Function ExtractFileName(ByVal thisDirectoryFileName As String)
        If String.IsNullOrEmpty(thisDirectoryFileName) Then Return False

        Dim myReturn As String = thisDirectoryFileName
        myReturn = myReturn.Split(cPathSplitChar).Last
        myReturn = myReturn.Split(".")(0)
        Return myReturn
    End Function
End Class
