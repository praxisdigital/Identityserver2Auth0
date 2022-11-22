// See https://aka.ms/new-console-template for more information
internal class FileStreamResult
{
    private MemoryStream memoryStream;
    private string v;

    public FileStreamResult(MemoryStream memoryStream, string v)
    {
        this.memoryStream = memoryStream;
        this.v = v;
    }
}