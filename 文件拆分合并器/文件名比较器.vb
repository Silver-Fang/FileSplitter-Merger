Imports Windows.Storage

Public Class 文件名比较器
	Implements IComparer(Of StorageFile)

	Public Function Compare(x As StorageFile, y As StorageFile) As Integer Implements IComparer(Of StorageFile).Compare
		Return SByte.Parse(x.Name.Split(".").Last) - SByte.Parse(y.Name.Split(".").Last)
	End Function
End Class
