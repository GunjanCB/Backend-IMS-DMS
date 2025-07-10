public interface ICapturedImageRepository
{
    Task AddCapturedImageAsync(CapturedImage image);
    Task<List<CapturedImage>> GetAllCapturedImagesAsync();
    Task<bool> DeleteCapturedImageAsync(int id);
    Task<List<CapturedImage>> GetCapturedImagesByUserIdAsync(int userId);

}