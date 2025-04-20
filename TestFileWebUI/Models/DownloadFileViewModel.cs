namespace TestFileWebUI.Models
{
    public class DownloadFileViewModel
    {
        public byte[]? fileBytes { get; set; }
        public string? contentType { get; set; }
        public string? fileName { get; set; }
    }
}
