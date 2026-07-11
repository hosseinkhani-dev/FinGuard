namespace FinGuard.UI.Models.TransactionFiles;

public class FileDownloadResult
{
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
    public string FileName { get; set; } = "template.xlsx";
}
