Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports MM.FHEMManager.FHEMObjects
Imports MM.FHEMManager

<TestClass()> Public Class UnitTestKernel
    Private Shared CurrentFHEM As FHEMKernel
    Private Shared SyncObj As New Object()
    Public Sub Initialize()
        If CurrentFHEM Is Nothing Then
            SyncLock SyncObj
                If CurrentFHEM Is Nothing Then CurrentFHEM = GetFhem()
            End SyncLock
        End If
    End Sub
    Private Function GetFhem() As FHEMKernel
        Dim myReturn As FHEMKernel
        myReturn = New FHEMKernel

        Dim myCfgFileName As String = String.Format("{0}\TestFiles\fhem.cfg", My.Application.Info.DirectoryPath)
        Assert.IsTrue(IO.File.Exists(myCfgFileName))
        Assert.IsTrue(myReturn.LoadCfg(myCfgFileName))

        Return myReturn
    End Function

    <TestMethod()> Public Sub TestKernelLoadCfg()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)

        Dim myCfgFileName As String = String.Format("{0}\TestFiles\fhem.cfg", My.Application.Info.DirectoryPath)
        Assert.IsTrue(IO.File.Exists(myCfgFileName))
        SyncLock SyncObj
            Assert.IsTrue(CurrentFHEM.LoadCfg(myCfgFileName))
            Assert.IsTrue(CurrentFHEM.CurrentObjects.Count > 0)
            Debug.WriteLine("Total Objects: {0}", New Object() {CurrentFHEM.CurrentObjects.Count})
        End SyncLock
    End Sub
    <TestMethod()> Public Sub TestKernelSaveCfg()
        If CurrentFHEM Is Nothing Then Initialize()
        Assert.IsNotNull(CurrentFHEM)

        Assert.IsTrue(CurrentFHEM.CurrentObjects.Count > 0)

        Dim myOutFileName As String = String.Format("{0}\TestFiles\fhem_save.cfg", My.Application.Info.DirectoryPath)
        SyncLock SyncObj
            Assert.IsTrue(CurrentFHEM.SaveCfg(myOutFileName, True))
            Assert.IsTrue(IO.File.Exists(myOutFileName))
            Assert.IsTrue(DateTime.Now.Subtract(IO.File.GetLastWriteTime(myOutFileName)).TotalSeconds < 3)
            Assert.IsTrue((New IO.FileInfo(myOutFileName)).Length > 0)
            Debug.WriteLine("File written to: {0}", New Object() {myOutFileName})
        End SyncLock
    End Sub

    <TestMethod> Public Sub TestBackupLogs()
        Dim myLogFolder As String = String.Format("{0}\TestFiles\Logfiles", My.Application.Info.DirectoryPath)
        Assert.IsTrue(IO.Directory.Exists(myLogFolder))

        Dim myBackupFolder As String = String.Format("{0}\TestFiles\Logfiles\Backup", My.Application.Info.DirectoryPath)
        If IO.Directory.Exists(myBackupFolder) Then
            IO.Directory.Delete(myBackupFolder, True)
            Threading.Thread.Sleep(100)
        End If

        Assert.IsFalse(IO.Directory.Exists(myBackupFolder))

        IO.Directory.CreateDirectory(myBackupFolder)
        Assert.IsTrue(IO.Directory.Exists(myBackupFolder))

        Assert.IsTrue(FHEMKernel.BackupLogs(myLogFolder, myBackupFolder))

        Dim myOldFolder As String = String.Format("{0}\TestFiles\Logfiles\Backup", My.Application.Info.DirectoryPath)
        Assert.IsTrue(IO.Directory.Exists(myOldFolder))

        Assert.IsTrue(FHEMKernel.PackOldFiles(myOldFolder))
    End Sub
    <TestMethod> Public Sub TestPackOldFiles()
        Dim myOldFolder As String = String.Format("{0}\TestFiles\Logfiles\Oldfiles", My.Application.Info.DirectoryPath)
        Assert.IsTrue(IO.Directory.Exists(myOldFolder))

        Assert.IsTrue(FHEMKernel.PackOldFiles(myOldFolder))
    End Sub
End Class