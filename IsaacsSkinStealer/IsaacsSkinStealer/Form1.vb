Imports System.IO
Imports System.Net
Imports Newtonsoft.Json.Linq

Public Class Form1
    Dim request As HttpWebRequest
    Dim response As HttpWebResponse
    Dim reader As StreamReader
    Dim rawresp As String
    Dim uid As String
    Dim base64 As String
    Dim base64Decoded As String
    Dim data() As Byte
    Dim SkinLink As String
    Dim reqlink As String

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If HaveInternetConnection() Then
            If TextBox1.Text = "" Then
                MsgBox("Please Enter A Username In The Text Box", MsgBoxStyle.Critical, "Error")
            Else
                Try
                    MakeRequest("https://api.mojang.com/users/profiles/minecraft/" & TextBox1.Text)
                    uid = JObject.Parse(rawresp)("id")
                    MakeRequest("https://sessionserver.mojang.com/session/minecraft/profile/" & uid)
                    base64 = JObject.Parse(rawresp)("properties")(0)("value")
                    data = Convert.FromBase64String(base64)
                    base64Decoded = System.Text.Encoding.ASCII.GetString(data)
                    SkinLink = JObject.Parse(base64Decoded)("textures")("SKIN")("url")
                    PictureBox1.ImageLocation = SkinLink
                Catch wex As WebException
                    Try
                        response = DirectCast(wex.Response, HttpWebResponse)
                        MsgBox("HTTP Code " & response.StatusCode & ". Description: " & wex.Message & " You can look up this code online for more info.", MsgBoxStyle.Information, "Web Request Error")
                    Catch ex As Exception
                        MsgBox("Timed out, the program will restart after this", MsgBoxStyle.Information, "Error")
                        Application.Restart()
                    End Try
                Catch ex As Exception
                    MsgBox("User does not exist or the Minecraft API is not returning a link to the skin image", MsgBoxStyle.Exclamation, "Error")
                End Try
            End If
        Else
            MsgBox("Could not connect to the Minecraft servers", MsgBoxStyle.Critical, "Error")
        End If
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        If PictureBox1.Image Is Nothing Then
            MsgBox("Please find a skin first", MsgBoxStyle.Critical, "Error")
        Else
            SaveFileDialog1.ShowDialog()
        End If
    End Sub

    Private Sub SaveFileDialog1_FileOk(ByVal sender As System.Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles SaveFileDialog1.FileOk
        If HaveInternetConnection() Then
            If File.Exists(SaveFileDialog1.FileName) Then
                File.Delete(SaveFileDialog1.FileName)
            End If
            My.Computer.Network.DownloadFile(PictureBox1.ImageLocation, SaveFileDialog1.FileName)
            SaveFileDialog1.FileName = ""
        Else
            MsgBox("Could not connect to the Minecraft servers", MsgBoxStyle.Critical, "Error")
        End If
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.Checked Then
            PictureBox1.SizeMode = PictureBoxSizeMode.Zoom
        Else
            PictureBox1.SizeMode = PictureBoxSizeMode.CenterImage
        End If
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim parts() As String = Split(My.User.Name, "\")
        Dim username As String = parts(1)
        SaveFileDialog1.InitialDirectory = ("C:\users\" + username + "\Pictures\")
        PictureBox1.SizeMode = PictureBoxSizeMode.CenterImage
    End Sub

    Public Function HaveInternetConnection() As Boolean
        Try
            Return My.Computer.Network.Ping("www.minecraft.net")
        Catch
            Return False
        End Try
    End Function

    Public Sub MakeRequest(reqlink)
        request = DirectCast(WebRequest.Create(reqlink), HttpWebRequest)
        response = DirectCast(request.GetResponse(), HttpWebResponse)
        reader = New StreamReader(response.GetResponseStream())
        rawresp = reader.ReadToEnd()
    End Sub
End Class
