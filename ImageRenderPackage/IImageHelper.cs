using Microsoft.AspNetCore.Http;
using SkiaSharp;

namespace ImageRenderPackage
{
    public interface IImageHelper
    {
        Task<(bool, string)> SaveImageToLocalAsync(IFormFile file, string path, string name, int quality = 90, SKEncodedImageFormat format = SKEncodedImageFormat.Webp, int width = 540, int height = 317);
        Task<(bool, string)> DeleteImageFromLocalAsync(string absolutePath, bool cache = false);
        Task<bool> LoadDeletedImageFromCache(string imageFullPath);
    }
}
