Module modMain
    Friend glbKernel As FHEMManager.FHEMKernel
    ''' <summary>
    ''' Main Routine for the CommandLine interface
    ''' </summary>
    Sub Main()
        glbKernel = New FHEMManager.FHEMKernel

        If My.Application.CommandLineArgs.Count = 0 Then
            ManualMode()
        Else
            Select Case My.Application.CommandLineArgs(0).ToLower
                Case "backuplogs"
                    FHEMKernel.BackupLogs()
                Case "packoldfiles"
                    FHEMKernel.PackOldFiles()
                Case Else
                    Console.WriteLine("dont understand {0}! Exiting!", My.Application.CommandLineArgs(0))
            End Select
        End If

    End Sub

    ''' <summary>
    ''' commandline
    ''' </summary>
    ''' <remarks>
    ''' load [filename] Load the default config. Supply a filename to load
    ''' save [path filename|pathandfilename] Save the loaded config
    ''' backuplog "logname" Archive old LogEntries from the logname (without extension, no .log nessecary)
    ''' backuplogs Archive all .log Files in the Log Directory
    ''' packoldfiles Packs the paranoia-safty-backups of LogFiles to zip by year_month
    ''' stop|exit What you expect
    ''' </remarks>
    <CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")>
    Private Sub ManualMode()
        Console.WriteLine("Please give me commands!")

        Dim myInput As String = Console.ReadLine
        While Not String.IsNullOrEmpty(myInput)
            Dim myReturn As String = Nothing

            Dim myInputs As List(Of String) = FindParts(myInput)

            'Dim myInputs As List(Of String) = myInput.Split(FHEMManager.FHEMKernel.cWordSplitChar, StringSplitOptions.RemoveEmptyEntries).ToList
            Select Case myInputs(0).ToLower
                Case "command", "commands", "cmds"
                    'TODO: Implement commandline help
                Case "load", "loadcfg"
                    If myInputs.Count = 2 Then
                        myReturn = glbKernel.LoadCfg(myInputs(1))
                    Else
                        myReturn = glbKernel.LoadCfg()
                    End If
                Case "save"
                    If myInputs.Count = 3 Then
                        myReturn = glbKernel.SaveCfg(myInputs(1), myInputs(2))
                    ElseIf myInputs.Count = 2 Then
                        Dim myOverwrite As Boolean = False
                        If IO.File.Exists(myInput(1)) Then
                            Console.WriteLine("Overwrite? y/n")
                            Select Case Console.ReadKey(True).Key
                                Case ConsoleKey.Y
                                    myOverwrite = True
                                Case ConsoleKey.N
                                    My.Application.Log.WriteEntry("Abort!", TraceEventType.Information)
                                    myOverwrite = False
                                Case Else
                                    My.Application.Log.WriteEntry("Unknown? Abort!", TraceEventType.Information)
                                    myOverwrite = False
                            End Select
                        End If
                        myReturn = glbKernel.SaveCfg(myInputs(1), myOverwrite)
                    Else
                        myReturn = glbKernel.SaveCfg()
                    End If
                Case "inspect"
                    myReturn = glbKernel.ToString()
                Case "joinlog"
                    If myInputs.Count = 2 Then
                        myReturn = FHEMKernel.JoinLog(myInputs(1))
                    Else
                        Console.WriteLine("wrong argument count: 2 - {0}!", myInput)
                    End If
                Case "joinlogs"
                    myReturn = FHEMKernel.JoinLogs()
                Case "backuplog"
                    If myInputs.Count = 2 Then
                        myReturn = FHEMKernel.BackupLog(myInputs(1))
                    Else
                        Console.WriteLine("wrong argument count: 2 - {0}!", myInput)
                    End If
                Case "backuplogs"
                    myReturn = FHEMKernel.BackupLogs()
                Case "packoldfiles"
                    If myInputs.Count = 1 Then
                        myReturn = FHEMKernel.PackOldFiles()
                    ElseIf myInputs.Count = 2 Then
                        myReturn = FHEMKernel.PackOldFiles(myInputs(1))
                    Else
                        Console.WriteLine("wrong argument count: 2 - {0}!", myInput)
                    End If
                Case "stop", "exit"
                    Exit While
                Case Else
                    Console.WriteLine("dont understand {0}!", myInput)
            End Select
            If Not myReturn Is Nothing Then
                My.Application.Log.WriteEntry(String.Format("returned: {0}", myReturn), TraceEventType.Information)
                Console.WriteLine("Please give me commands!")
            End If
            myInput = Console.ReadLine
        End While
        Console.WriteLine("Bye Bye")
    End Sub

    Private Function FindParts(myInput As String) As List(Of String)
        Dim myReturn As New List(Of String)

        Dim mySB As New Text.StringBuilder

        Dim mySpaceString As Boolean = False
        For Each myChar In myInput.ToCharArray
            If myChar = """" Then
                If mySpaceString = False Then
                    mySpaceString = True
                Else
                    mySpaceString = False
                End If
            ElseIf myChar = " " Then
                If mySpaceString = False Then
                    myReturn.Add(mySB.ToString)
                    mySB.Clear()
                Else
                    mySB.Append(myChar)
                End If
            Else
                mySB.Append(myChar)
            End If
        Next
        myReturn.Add(mySB.ToString)

        Return myReturn
    End Function
End Module
