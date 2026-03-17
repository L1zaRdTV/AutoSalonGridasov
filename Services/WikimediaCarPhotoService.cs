using Microsoft.Extensions.Caching.Memory;

namespace AutoSalonGrida.Services;

public sealed class WikimediaCarPhotoService : ICarPhotoService
{
    private static readonly IReadOnlyList<string> SchematicPhotos =
    [
        "/assets/images/car-1.svg",
        "/assets/images/car-2.svg",
        "/assets/images/car-3.svg",
        "/assets/images/car-4.svg",
        "/assets/images/car-5.svg",
        "/assets/images/car-6.svg",
        "/assets/images/car-7.svg",
        "/assets/images/car-8.svg",
        "/assets/images/car-9.svg",
        "/assets/images/car-10.svg",
        "/assets/images/default-car.svg"
    ];

    private readonly IMemoryCache _cache;

    public WikimediaCarPhotoService(IMemoryCache cache)
    {
        _cache = cache;
    }


    public Task<IReadOnlyList<string>> GetPhotosAsync(string brand, string model, int count = 4, CancellationToken cancellationToken = default)
    {
        var key = $"{brand} {model}".Trim().ToLowerInvariant();

        if (!_cache.TryGetValue(key, out IReadOnlyList<string>? cached) || cached is null)
        {
            cached = BuildDeterministicSet(key, Math.Max(count, 4));
            _cache.Set(key, cached, TimeSpan.FromHours(12));
        }

        return Task.FromResult<IReadOnlyList<string>>(cached.Take(Math.Max(1, count)).ToList());
    }

    private static IReadOnlyList<string> BuildDeterministicSet(string key, int count)
    {
        var startIndex = Math.Abs(key.GetHashCode()) % SchematicPhotos.Count;
        var result = new List<string>(count);

        for (var i = 0; i < count; i++)
        {
            result.Add(SchematicPhotos[(startIndex + i) % SchematicPhotos.Count]);
        }

        return result;
    }
}
