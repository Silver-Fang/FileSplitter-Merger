' https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板
Imports Windows.Storage
''' <summary>
''' 可用于自身或导航至 Frame 内部的空白页。
''' </summary>
Public NotInheritable Class MainPage
    Inherits Page
	Private 选定文件 As StorageFile, 选定目录 As StorageFolder, 输入文件列表 As IReadOnlyList(Of StorageFile), 文件保存器 As New Pickers.FileSavePicker, 输出文件SF As StorageFile
	Shared ReadOnly 目录打开器 As New Pickers.FolderPicker, 文件打开器 As New Pickers.FileOpenPicker, 文件比较器 As New 文件名比较器

	Private Async Sub 开始合并_Click(sender As Object, e As RoutedEventArgs) Handles 开始合并.Click
		If 输入文件列表 Is Nothing OrElse 输入文件列表.Count < 2 Then
			提示文本.Text = "请至少选择2个文件"
			错误提示.ShowAt(sender)
			Return
		End If
		If 输出文件SF Is Nothing Then
			提示文本.Text = "未选择输出文件"
			错误提示.ShowAt(sender)
			Return
		End If
		Dim b As Task(Of Stream) = 输出文件SF.OpenStreamForWriteAsync, c As List(Of StorageFile) = 输入文件列表.ToList
		合并输入浏览.IsEnabled = False
		合并输出浏览.IsEnabled = False
		开始合并.IsEnabled = False
		c.Sort(文件比较器)
		合并进度条.Maximum = c.Count
		合并进度条.Value = 0
		Dim d As Stream = Await b
		For Each a As StorageFile In c
			Await (Await a.OpenStreamForReadAsync).CopyToAsync(d)
			合并进度条.Value += 1
		Next
		d.Close()
		合并输入浏览.IsEnabled = True
		合并输出浏览.IsEnabled = True
		开始合并.IsEnabled = True
		提示文本.Text = "合并完成"
		错误提示.ShowAt(sender)
	End Sub

	Private Async Sub 输入浏览_Click(sender As Object, e As RoutedEventArgs) Handles 输入浏览.Click
		Dim a As StorageFile = Await 文件打开器.PickSingleFileAsync()
		If a IsNot Nothing Then
			选定文件 = a
			输入路径.Text = a.Path
		End If
	End Sub

	Private Async Sub 合并输出浏览_Click(sender As Object, e As RoutedEventArgs) Handles 合并输出浏览.Click
		Dim a As StorageFile = Await 文件保存器.PickSaveFileAsync
		If a IsNot Nothing Then
			输出文件SF = a
			输出文件.Text = a.Path
		End If
	End Sub

	Private Async Sub 合并输入浏览_Click(sender As Object, e As RoutedEventArgs) Handles 合并输入浏览.Click
		Dim a As IReadOnlyList(Of StorageFile) = Await 文件打开器.PickMultipleFilesAsync
		If a.Count > 0 Then
			输入文件列表 = a
			输入文件数.Text = a.Count
			Dim b As String() = a(0).Name.Split(".")
			文件保存器.FileTypeChoices.Clear()
			文件保存器.FileTypeChoices.Add("选定文件", {"." & If(b.Count > 2, b(b.GetUpperBound(0) - 1), "合并")})
		End If
	End Sub

	Private Async Sub 开始拆分_Click(sender As Object, e As RoutedEventArgs) Handles 开始拆分.Click
		Dim 拆分数 As Byte
		Try
			拆分数 = 拆分个数.Text
		Catch ex As Exception
			拆分数 = 1
		End Try
		If 拆分数 < 2 Then
			提示文本.Text = "无效的拆分数"
			错误提示.ShowAt(sender)
			Return
		End If
		If 选定文件 Is Nothing Then
			提示文本.Text = "输入文件无效"
			错误提示.ShowAt(sender)
			Return
		End If
		If 选定目录 Is Nothing Then
			提示文本.Text = "输出目录无效"
			错误提示.ShowAt(sender)
			Return
		End If
		Dim a As Task(Of Stream) = 选定文件.OpenStreamForReadAsync, b As IAsyncOperation(Of FileProperties.BasicProperties) = 选定文件.GetBasicPropertiesAsync
		输入浏览.IsEnabled = False
		输出浏览.IsEnabled = False
		开始拆分.IsEnabled = False
		进度条.Value = 0
		进度条.Maximum = 拆分数 * 2
		Dim c As ULong = (Await b).Size, d As Stream = Await a, g(c / 拆分数) As Byte, h As ULong
		For f As Byte = 0 To 拆分数 - 1
			h = (c / (拆分数 - f))
			Await d.ReadAsync(g, 0, h)
			进度条.Value += 1
			Await (Await (Await 选定目录.CreateFileAsync(选定文件.Name & "." & f)).OpenStreamForWriteAsync).WriteAsync(g, 0, h)
			进度条.Value += 1
			c -= h
		Next
		d.Close()
		输入浏览.IsEnabled = True
		输出浏览.IsEnabled = True
		开始拆分.IsEnabled = True
		提示文本.Text = "拆分完成"
		错误提示.ShowAt(sender)
	End Sub

	Private Async Sub 输出浏览_Click(sender As Object, e As RoutedEventArgs) Handles 输出浏览.Click
		Dim a As StorageFolder = Await 目录打开器.PickSingleFolderAsync
		If a IsNot Nothing Then
			选定目录 = a
			输出路径.Text = a.Path
		End If
	End Sub

	Private Sub MainPage_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
		文件打开器.FileTypeFilter.Add("*")
		目录打开器.FileTypeFilter.Add("*")
		文件保存器.FileTypeChoices.Add("合并文件", {".合并"})
	End Sub
End Class
