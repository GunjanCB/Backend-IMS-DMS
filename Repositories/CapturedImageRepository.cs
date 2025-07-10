using DocumentManagementBackend.Data;
using Microsoft.EntityFrameworkCore;

public class CapturedImageRepository : ICapturedImageRepository
{
    private readonly DataContext _context;

    public CapturedImageRepository(DataContext context)
    {
        _context = context;
    }
    public async Task AddCapturedImageAsync(CapturedImage image)
    {
        _context.CapturedImages.Add(image);
        await _context.SaveChangesAsync();

    }

    public async Task<List<CapturedImage>> GetAllCapturedImagesAsync()
    {
        return await _context.CapturedImages.ToListAsync();
    }
   public async Task<List<CapturedImage>> GetCapturedImagesByUserIdAsync(int userId)
{
    return await _context.CapturedImages
        .Where(img => img.UserId == userId)
        .OrderByDescending(img => img.UploadedAt)
        .ToListAsync();
}

    public async Task<bool> DeleteCapturedImageAsync(int id)
    {
        var image = await _context.CapturedImages.FindAsync(id);
        if (image == null)
            return false;

        _context.CapturedImages.Remove(image);
        await _context.SaveChangesAsync();
        return true;
    }
}
