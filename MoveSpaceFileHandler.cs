using System.Runtime.InteropServices;

#pragma warning disable CS8618
public class MoveSpaceFileHandler : IDisposable
{
    public string FilePath;
    public IntPtr FileContent = IntPtr.Zero;
    public long Length;

    private MoveSpaceFileHandler() { }

    public bool IsValid => FileContent != IntPtr.Zero;

    public static MoveSpaceFileHandler GetFile(string _FilePath)
    {
        if (string.IsNullOrEmpty(_FilePath) || !File.Exists(_FilePath)) throw new ArgumentException("Invalid file path", nameof(_FilePath));
        MoveSpaceFileHandler file = new();
        file.FilePath = _FilePath;
        using (BinaryReader binaryReader = new BinaryReader((Stream)new FileStream(_FilePath, FileMode.Open, FileAccess.Read)))
        {
            file.Length = binaryReader.BaseStream.Length;
            byte[] numArray = new byte[file.Length];
            binaryReader.Read(numArray, 0, numArray.Length);
            file.FileContent = Marshal.AllocHGlobal(numArray.Length);
            Marshal.Copy(numArray, 0, file.FileContent, numArray.Length);
        }
        return file;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Dispose(bool _Disposing)
    {
        if (_Disposing) Length = 0L;
        if (IsValid) Marshal.FreeHGlobal(FileContent);
        FileContent = IntPtr.Zero;
    }

    ~MoveSpaceFileHandler() => Dispose(false);
}