namespace Tests.AndroidApp.Helpers;

internal sealed class FileReader(string directoryPath)
{
    public ArraySegment<byte> ReadFile(string fileName)
    {
        using Stream stream = FileSystem.OpenAppPackageFileAsync(Path.Combine(directoryPath, fileName)).Result;

        using MemoryStream memoryStream = new();
        stream.CopyTo(memoryStream);

        return new ArraySegment<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
    }
}
