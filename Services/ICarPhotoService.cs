namespace AutoSalonGrida.Services;

public interface ICarPhotoService
{
    Task<IReadOnlyList<string>> GetPhotosAsync(string brand, string model, int count = 4, CancellationToken cancellationToken = default);
}
