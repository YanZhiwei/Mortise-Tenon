using Microsoft.AspNetCore.Mvc;
using Tenon.AspNetCore.Filters;

namespace FileUploadSample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly ILogger<FileController> _logger;
    private readonly IWebHostEnvironment _environment;

    public FileController(ILogger<FileController> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// 上传单个文件
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns>上传结果</returns>
    [HttpPost("upload")]
    [ServiceFilter(typeof(FileUploadValidationFilter))]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            // 创建上传目录
            var uploadPath = Path.Combine(_environment.ContentRootPath, "uploads");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            // 生成文件名
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);

            // 保存文件
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("File uploaded successfully: {FileName}", fileName);

            return Ok(new { fileName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// 上传多个文件
    /// </summary>
    /// <param name="files">文件列表</param>
    /// <returns>上传结果</returns>
    [HttpPost("upload-multiple")]
    [ServiceFilter(typeof(FileUploadValidationFilter))]
    public async Task<IActionResult> UploadMultiple(IFormFileCollection files)
    {
        try
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded");

            var uploadPath = Path.Combine(_environment.ContentRootPath, "uploads");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var uploadedFiles = new List<string>();

            foreach (var file in files)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                uploadedFiles.Add(fileName);
                _logger.LogInformation("File uploaded successfully: {FileName}", fileName);
            }

            return Ok(new { files = uploadedFiles });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading files");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// 上传外部资源文件
    /// </summary>
    /// <param name="url">外部资源URL</param>
    /// <returns>上传结果</returns>
    [HttpPost("upload-external")]
    [ServiceFilter(typeof(FileUploadValidationFilter))]
    public async Task<IActionResult> UploadExternal([FromForm] string url)
    {
        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return BadRequest("Failed to download external resource");

            var contentType = response.Content.Headers.ContentType?.MediaType;
            var extension = contentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "application/pdf" => ".pdf",
                _ => Path.GetExtension(url)
            };

            var uploadPath = Path.Combine(_environment.ContentRootPath, "uploads");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await response.Content.CopyToAsync(stream);
            }

            _logger.LogInformation("External file uploaded successfully: {FileName}", fileName);

            return Ok(new { fileName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading external file");
            return StatusCode(500, "Internal server error");
        }
    }
} 