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
}