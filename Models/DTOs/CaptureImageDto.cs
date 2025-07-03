public class CapturedImageDto

{
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = "invoice";
    public bool Multipage { get; set; }
    public string Image { get; set; } = string.Empty;
}