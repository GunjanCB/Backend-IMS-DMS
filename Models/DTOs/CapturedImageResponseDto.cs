public class CapturedImageResponseDto
{
    public int Id { get; set; }
    public string Filename { get; set; } = string.Empty;
    public string Filetype { get; set; } = "invoice";
    public bool Multipage { get; set; }
    public string Image { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}