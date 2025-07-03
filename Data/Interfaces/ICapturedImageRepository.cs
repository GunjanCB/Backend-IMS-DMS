public interface ICapturedImageRepository
{
    Task AddCapturedImageAsync(CapturedImage image);
    Task<List<CapturedImage>> GetAllCapturedImagesAsync();
}