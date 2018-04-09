Partial Class FHEMKernel
    Public Shared Function PackOldFiles() As Boolean
        My.Application.Log.WriteEntry(String.Format("PackOldFiles using default config: {0}", My.Settings.DefaultLogBackupPath), TraceEventType.Information)

        Return PackOldFiles(My.Settings.DefaultLogBackupPath)
    End Function

    Public Shared Function PackOldFiles(thisFolderName As String) As Boolean
        My.Application.Log.WriteEntry(String.Format("PackOldFiles in Folder: {0}", thisFolderName), TraceEventType.Information)

        If String.IsNullOrEmpty(thisFolderName) Then Return False
        If Not IO.Directory.Exists(thisFolderName) Then Return False

        Dim myCategories As New Dictionary(Of String, List(Of String))

        Dim myFiles = IO.Directory.EnumerateFiles(thisFolderName, "*.old", IO.SearchOption.AllDirectories).ToList
        If myFiles IsNot Nothing AndAlso myFiles.Count > 0 Then
            For Each myFile In myFiles
                Dim myCreateDateTime = IO.File.GetLastWriteTime(myFile)
                Dim myCatKey As String = String.Format("{0:d4}_{1:d2}", myCreateDateTime.Year, myCreateDateTime.Month)
                If Not myCategories.ContainsKey(myCatKey) Then
                    myCategories.Add(myCatKey, New List(Of String) From {myFile})
                Else
                    DirectCast(myCategories(myCatKey), List(Of String)).Add(myFile)
                End If
            Next
        End If
        myCategories = myCategories.OrderBy(Function(x) x.Key).ToDictionary(Function(x) x.Key, Function(y) y.Value)

        My.Application.Log.WriteEntry(String.Format("Compressing total {0:d4} Files in {1:d2} Groups", myFiles.Count, myCategories.Count), TraceEventType.Information)

        myFiles.Clear()
        myFiles = Nothing
        GC.WaitForFullGCComplete()

        For Each myCategory In myCategories
            PackOldFiles(myCategory, thisFolderName)
        Next
        Return True
    End Function

    Private Shared Function PackOldFiles(thisKeyValue As KeyValuePair(Of String, List(Of String)), thisFolderName As String) As Boolean
        If String.IsNullOrEmpty(thisFolderName) Then Return False
        If Not IO.Directory.Exists(thisFolderName) Then Return False

        Dim my7ZipFileName As String = String.Format("{0}\7-Zip\7z.exe", My.Computer.FileSystem.SpecialDirectories.ProgramFiles)
        If Not IO.File.Exists(my7ZipFileName) Then Return False

        Dim myTempFile As String = String.Format("{0}\{1}.fhem", My.Computer.FileSystem.SpecialDirectories.Temp, thisKeyValue.Key)
        Dim myOutFileName As String = String.Format("{0}\FHEMSaveLogs_Old_{1}.zip", thisFolderName, thisKeyValue.Key)

        Dim myStopWatch As Stopwatch = Stopwatch.StartNew

        Dim myMemStream As New IO.MemoryStream
        Using myWriter As IO.TextWriter = New IO.StreamWriter(myMemStream)
            thisKeyValue.Value.ForEach(Sub(x) myWriter.WriteLine(x))
            myWriter.Flush()
            Helper.StreamToNewFile(myMemStream, myTempFile)
        End Using
        If myMemStream IsNot Nothing Then myMemStream.Dispose()


        Dim myStartInfo As New ProcessStartInfo With {
            .FileName = my7ZipFileName,
            .WorkingDirectory = thisFolderName,
            .Arguments = String.Format("a -tzip -mx=7 -mmt=on -sdel ""{0}"" @""{1}""", myOutFileName, myTempFile),
            .CreateNoWindow = True,
            .WindowStyle = ProcessWindowStyle.Hidden
        }


        Dim my7zip As Process
        my7zip = Process.Start(myStartInfo)
        my7zip.PriorityBoostEnabled = False
        my7zip.PriorityClass = ProcessPriorityClass.BelowNormal

        my7zip.WaitForExit(TimeSpan.FromMinutes(10).Milliseconds)

        While Not my7zip.HasExited
            Threading.Thread.Sleep(100)
            my7zip.Refresh()
        End While

        If my7zip.ExitCode = 0 AndAlso IO.File.Exists(myOutFileName) Then
            'Made with 7zip
            'IO.File.Delete(myTempFile)
            'thisKeyValue.Value.ForEach(Sub(x) IO.File.Delete(x))

            My.Application.Log.WriteEntry(String.Format("Compressing {0} ({1:d3} Files) to {2} took {3} seconds", thisKeyValue.Key, thisKeyValue.Value.Count, myOutFileName.Replace(thisFolderName & "\", String.Empty), myStopWatch.Elapsed.TotalSeconds.ToString("00.00")), TraceEventType.Information)

            Return True
        Else
            Return False
        End If
    End Function
End Class