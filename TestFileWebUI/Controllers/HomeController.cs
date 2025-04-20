using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TestFileWebUI.Models;

namespace TestFileWebUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _client;
        public HomeController(ILogger<HomeController> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _client = clientFactory.CreateClient("FileUploadAPI");
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile? dosya)
        {
            if (dosya == null || dosya.Length == 0)
            {
                return BadRequest("Dosya bulunamadý.");
            }

            using var content = new MultipartFormDataContent();
            using var stream = dosya.OpenReadStream();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(dosya.ContentType);

            content.Add(streamContent, "file", dosya.FileName);

            var result = await _client.PostAsync("FileUpload/upload", content);

            if (!result.IsSuccessStatusCode)
            {
                return StatusCode((int)result.StatusCode, await result.Content.ReadAsStringAsync());
            }

            var response = await result.Content.ReadFromJsonAsync<ResponseViewModel>();
            return RedirectToAction("ListFile");
        }

        [HttpGet]
        public async Task<IActionResult> ListFile()
        {
            var result = await _client.GetFromJsonAsync<List<string>>("FileUpload/list");

            if (result == null)
                return NotFound();

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return BadRequest("Dosya yolu boþ olamaz.");

            // API'den dosya verisini almak için HttpClient kullanýyoruz
            var result = await _client.GetAsync($"FileUpload/download?filePath={filePath}");

            if (!result.IsSuccessStatusCode)
            {
                return NotFound("Dosya bulunamadý.");
            }

            // Dosya içeriðini almak
            var fileBytes = await result.Content.ReadAsByteArrayAsync();

            // Ýçeriði ve dosya adýný alýyoruz
            var contentType = "application/octet-stream"; // Ýhtiyaca göre daha uygun bir contentType seçilebilir
            var fileName = Path.GetFileName(filePath);

            // Dosyayý yanýt olarak döndürüyoruz
            return File(fileBytes, contentType, fileName);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteFile(string filePath)
        {
            var result = await _client.DeleteAsync("FileUpload/delete?filePath=" + filePath);

            if (result == null)
                return NotFound();

            return RedirectToAction("ListFile");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

