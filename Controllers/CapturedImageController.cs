using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Bcpg.Attr;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/[Controller]")]



public class CapturedImageController : ControllerBase
{
    private readonly ICapturedImageRepository _repository;

    public CapturedImageController(ICapturedImageRepository repository)
    {
        _repository = repository;
    }

  [HttpPost("upload")]
public async Task<IActionResult> UploadImage([FromBody] CapturedImageDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.Image) || string.IsNullOrWhiteSpace(dto.FileName))
        return BadRequest("Missing image or filename");

    var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
    {
        return Unauthorized();
    }

    var imageEntity = new CapturedImage
    {
        FileName = dto.FileName,
        FileType = dto.FileType,
        IsMultipage = dto.Multipage,
        ImageBase64 = dto.Image,
        UserId = userId,
        UploadedAt = DateTime.UtcNow
    };

    await _repository.AddCapturedImageAsync(imageEntity);
    return Ok(new { message = "Image uploaded successfully" });
}

[Authorize]
[HttpGet]
public async Task<IActionResult> GetAllImages()
{
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int userId))
        {
            return Unauthorized();
        }

    var images = await _repository.GetCapturedImagesByUserIdAsync(userId);

    var response = images.Select(img => new CapturedImageResponseDto
    {
        Id = img.Id,
        Filename = img.FileName,
        Filetype = img.FileType,
        Multipage = img.IsMultipage,
        Image = img.ImageBase64.StartsWith("data:")
            ? img.ImageBase64
            : $"data:image/jpeg;base64,{img.ImageBase64}",
        Date = img.UploadedAt
    });

    return Ok(response);
}

   [HttpDelete("{id}")]
public async Task<IActionResult> DeleteCapturedImage(int id)
{
    var deleted = await _repository.DeleteCapturedImageAsync(id);
    if (!deleted)
    {
        return NotFound();
    }
    return NoContent();
}

}