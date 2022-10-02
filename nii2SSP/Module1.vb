Imports System.IO

Module Module1
    Sub Main(args() As String)

        Dim SourceFilePath As String = Nothing
        If args.Length < 1 Then
            Console.WriteLine("usage hogehoge.exe Source")
            Environment.Exit(1)
        Else
            SourceFilePath = args(0)
        End If

        Dim ImgFilePath As String = Left(SourceFilePath, SourceFilePath.LastIndexOf(".")) & ".img"
        Dim HdrFilePath As String = Left(SourceFilePath, SourceFilePath.LastIndexOf(".")) & ".hdr"

        If System.IO.File.Exists(SourceFilePath) = False Then
            Console.WriteLine(SourceFilePath & "が見つかりません。")
            Environment.Exit(2)
        ElseIf System.IO.File.Exists(ImgFilePath) = True Then
            Console.WriteLine(ImgFilePath & "が存在します。")
            Environment.Exit(2)
        ElseIf System.IO.File.Exists(HdrFilePath) = True Then
            Console.WriteLine(HdrFilePath & "が存在します。")
            Environment.Exit(2)
        End If

        Dim MatrixX As Long = 0
        Dim MatrixY As Long = 0
        Dim SliceNo As Long = 0
        Dim BigEndian As Boolean = False
        Dim RescaleSlope As Single = 1
        Dim RescaleIntercept As Single = 0
        Dim DataType As Short = 0
        Dim SizeX As Single = 0
        Dim SizeY As Single = 0
        Dim SizeZ As Single = 0

        'SourceFileの読み込み
        Using stream As Stream = File.OpenRead(SourceFilePath)
            ' streamから読み込むためのBinaryReaderを作成
            Using reader As New BinaryReader(stream)
                Dim HeaderSize1 As Long = reader.ReadInt32
                If HeaderSize1 = 348 Then
                    BigEndian = False
                ElseIf HeaderSize1 = 1543569408 Then
                    BigEndian = True
                End If
                reader.ReadBytes(38)

                Dim TempBuff() As Byte
                ReDim TempBuff(1)

                TempBuff = reader.ReadBytes(2)
                If BigEndian = True Then
                    Array.Reverse(TempBuff)
                End If
                MatrixX = BitConverter.ToInt16(TempBuff, 0)

                TempBuff = reader.ReadBytes(2)
                If BigEndian = True Then
                    Array.Reverse(TempBuff)
                End If
                MatrixY = BitConverter.ToInt16(TempBuff, 0)

                TempBuff = reader.ReadBytes(2)
                If BigEndian = True Then
                    Array.Reverse(TempBuff)
                End If
                SliceNo = BitConverter.ToInt16(TempBuff, 0)

                reader.ReadBytes(22)

                TempBuff = reader.ReadBytes(2)
                If BigEndian = True Then
                    Array.Reverse(TempBuff)
                End If
                DataType = BitConverter.ToInt16(TempBuff, 0)

                reader.ReadBytes(8)

                ReDim TempBuff(3)
                TempBuff = reader.ReadBytes(4)
                If BigEndian = True Then
                    Array.Reverse(TempBuff)
                End If
                SizeX = BitConverter.ToSingle(TempBuff, 0)

                TempBuff = reader.ReadBytes(4)
                If BigEndian = True Then
                    Array.Reverse(TempBuff)
                End If
                SizeY = BitConverter.ToSingle(TempBuff, 0)

                TempBuff = reader.ReadBytes(4)
                If BigEndian = True Then
                    Array.Reverse(TempBuff)
                End If
                SizeZ = BitConverter.ToSingle(TempBuff, 0)

                reader.ReadBytes(20)

                TempBuff = reader.ReadBytes(4)
                If BigEndian = True Then
                    Array.Reverse(TempBuff)
                End If
                RescaleSlope = BitConverter.ToSingle(TempBuff, 0)

                TempBuff = reader.ReadBytes(4)
                If BigEndian = True Then
                    Array.Reverse(TempBuff)
                End If
                RescaleIntercept = BitConverter.ToSingle(TempBuff, 0)

            End Using
        End Using

        Dim TotalPixelNum As Long = MatrixX * MatrixY * SliceNo

        Dim DestBuff(TotalPixelNum - 1) As Double

        Using stream As Stream = File.OpenRead(SourceFilePath)
            ' streamから読み込むためのBinaryReaderを作成
            Using reader As New BinaryReader(stream)
                '画素値取り込み

                Dim BytesPerPixel As Long

                Select Case DataType
                    Case 1
                        BytesPerPixel = 1
                    Case 2
                        BytesPerPixel = 1
                    Case 4
                        BytesPerPixel = 2
                    Case 8
                        BytesPerPixel = 4
                    Case 16
                        BytesPerPixel = 4
                    Case 64
                        BytesPerPixel = 8
                    Case 512
                        BytesPerPixel = 4
                End Select

                Dim TempBuff(BytesPerPixel) As Byte
                reader.ReadBytes(352)
                For i = 0 To TotalPixelNum - 1
                    Select Case DataType
                        Case 1
                            DestBuff(i) = CDbl(reader.ReadByte) * RescaleSlope + RescaleIntercept
                        Case 2
                            DestBuff(i) = CDbl(reader.ReadByte) * RescaleSlope + RescaleIntercept
                        Case 4
                            TempBuff = reader.ReadBytes(2)
                            If BigEndian = True Then
                                Array.Reverse(TempBuff)
                            End If
                            DestBuff(i) = CDbl(BitConverter.ToInt16(TempBuff, 0)) * RescaleSlope + RescaleIntercept
                        Case 8
                            TempBuff = reader.ReadBytes(4)
                            If BigEndian = True Then
                                Array.Reverse(TempBuff)
                            End If
                            DestBuff(i) = CDbl(BitConverter.ToInt32(TempBuff, 0)) * RescaleSlope + RescaleIntercept
                        Case 16
                            TempBuff = reader.ReadBytes(4)
                            If BigEndian = True Then
                                Array.Reverse(TempBuff)
                            End If
                            DestBuff(i) = CDbl(BitConverter.ToSingle(TempBuff, 0)) * RescaleSlope + RescaleIntercept
                        Case 64
                            TempBuff = reader.ReadBytes(8)
                            If BigEndian = True Then
                                Array.Reverse(TempBuff)
                            End If
                            DestBuff(i) = CDbl(BitConverter.ToDouble(TempBuff, 0)) * RescaleSlope + RescaleIntercept
                        Case 512
                            TempBuff = reader.ReadBytes(4)
                            If BigEndian = True Then
                                Array.Reverse(TempBuff)
                            End If
                            DestBuff(i) = CDbl(BitConverter.ToUInt16(TempBuff, 0)) * RescaleSlope + RescaleIntercept
                    End Select
                Next
            End Using
        End Using

        Using stream As Stream = File.OpenWrite(ImgFilePath)
            ' streamに書き込むためのBinaryWriterを作成
            Using writer As New BinaryWriter(stream)
                For i = 0 To TotalPixelNum - 1
                    writer.Write(CSng(DestBuff(i)))
                Next
            End Using
        End Using

        Using stream As Stream = File.OpenWrite(HdrFilePath)
            ' streamに書き込むためのBinaryWriterを作成
            Using writer As New StreamWriter(stream)
                writer.WriteLine("!data offset in bytes :=0")
                writer.WriteLine("!imagedata byte order :=LITTLEENDIAN")
                writer.WriteLine("!matrix size [1] :=" & CStr(MatrixX))
                writer.WriteLine("!matrix size [2] :=" & CStr(MatrixY))
                writer.WriteLine("!data format :=3")
                writer.WriteLine("!number format :=floating point")
                writer.WriteLine("!number of bytes per pixel :=4")
                writer.WriteLine("scaling factor (mm/pixel) [1] :=" & CStr(SizeX))
                writer.WriteLine("scaling factor (mm/pixel) [2] :=" & CStr(SizeY))
                writer.WriteLine("!pixel scaling value :=32700.000000")
                writer.WriteLine("!number of slices :=" & CStr(SliceNo))
                writer.WriteLine("!slice thickness (mm/pixel) :=" & CStr(SizeZ))
                writer.WriteLine("!the right brain on the left   :=1")
                writer.WriteLine("!the anterior to the posterior :=0")
                writer.WriteLine("!the superior to the inferior  :=0")
                writer.WriteLine("!END OF HEADER:=")
            End Using
        End Using
    End Sub

End Module
