public class CapturedImage
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = "invoice";
    public bool IsMultipage { get; set; }
    public string ImageBase64 { get; set; } = string.Empty;
    public string ImageData { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}