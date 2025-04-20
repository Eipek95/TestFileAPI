using Microsoft.AspNetCore.Mvc;

namespace FileUploadAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly string _baseUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Dosya bulunamadı.");

            var dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
            var uploadPath = Path.Combine(_baseUploadPath, dateFolder);

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var extension = Path.GetExtension(file.FileName);
            var newFileName = $"emre_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, newFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { FileName = newFileName, FilePath = $"{dateFolder}/{newFileName}" });
        }


        [HttpGet("list")]
        public IActionResult ListFiles()
        {
            var dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
            var todayPath = Path.Combine(_baseUploadPath, dateFolder);

            if (!Directory.Exists(todayPath))
                return Ok(new List<string>());

            var files = Directory.GetFiles(todayPath)
                .Select(path => Path.Combine(dateFolder, Path.GetFileName(path)).Replace("\\", "/"))
                .ToList();

            return Ok(files);
        }

        [HttpDelete("delete")]
        public IActionResult DeleteFile([FromQuery] string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return BadRequest("Dosya yolu boş olamaz.");

            var fullPath = Path.Combine(_baseUploadPath, filePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (!System.IO.File.Exists(fullPath))
                return NotFound("Dosya bulunamadı.");

            System.IO.File.Delete(fullPath);
            return Ok("Dosya silindi.");
        }


        [HttpGet("download")]
        public IActionResult DownloadFile([FromQuery] string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return BadRequest("Dosya yolu boş olamaz.");

            var fullPath = Path.Combine(_baseUploadPath, filePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (!System.IO.File.Exists(fullPath))
                return NotFound("Dosya bulunamadı.");

            var fileBytes = System.IO.File.ReadAllBytes(fullPath);
            var contentType = "application/octet-stream";
            var fileName = Path.GetFileName(fullPath);

            return File(fileBytes, contentType, fileName);
        }
    }
}