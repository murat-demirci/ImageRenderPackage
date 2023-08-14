using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using SkiaSharp;

namespace ImageRenderPackage
{
    public class ImageHelper : IImageHelper
    {
        private readonly IMemoryCache memoryCache;
        public ImageHelper(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }
        public async Task<(bool, string)> SaveImageToLocalAsync(IFormFile file, string path, string name, int quality = 90, SKEncodedImageFormat format = SKEncodedImageFormat.Webp, int width = 540, int height = 317)
        {
            try
            {
                int fCountOld = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly).Length;

                var extension = format.ToString().ToLower();
                var imageName = $"{width}x{height}_{name}_{DateTime.UtcNow.ToLongTimeString().Replace(":", "_")}_{Guid.NewGuid():N}.{extension}"; //file name


                using (var input = file.OpenReadStream())
                using (var inputStream = new SKManagedStream(input))
                using (var original = SKBitmap.Decode(inputStream))
                {
                    using (var resized = original.Resize(new SKImageInfo(width, height), SKBitmapResizeMethod.Lanczos3))
                    {
                        if (resized == null) return (false, string.Empty);
                        using (var image = SKImage.FromBitmap(resized))
                        using (var output = File.OpenWrite(Path.Combine(path, imageName)))
                        {
                            await Task.Run(() => image.Encode(format, quality).SaveTo(output));
                        }
                    }
                }

                int fCountNew = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly).Length;

                if (!(fCountNew - fCountOld == 1))
                    return (false, string.Empty);

                return (true, Path.Combine(path, imageName).ToString());
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }


        public async Task<(bool, string)> DeleteImageFromLocalAsync(string absolutePath, bool cache = false)
        {
            try
            {

                if (!File.Exists(absolutePath)) return (false, "The image not found");

                memoryCache.Remove("deleted-image");

                if (cache)
                    await CacheImage(absolutePath);

                int fCountOld = Directory.GetFiles(Directory.GetParent(absolutePath).FullName, "*", SearchOption.TopDirectoryOnly).Length;

                File.Delete(absolutePath);

                int fCountNew = Directory.GetFiles(Directory.GetParent(absolutePath).FullName, "*", SearchOption.TopDirectoryOnly).Length;

                if (!(fCountOld - fCountNew == 1))
                {
                    memoryCache.Remove("deleted-image");

                    return (false, "The image could not be deleted");
                }

                return (true, "The image was deleted");

            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// If image cached, return image from cache with same properties
        /// </summary>
        public async Task<bool> LoadDeletedImageFromCache(string imageFullPath)
        {
            try
            {
                var imageFromCache = memoryCache.Get<byte[]>("deleted-image");

                if (imageFromCache is null) return false;

                await File.Create(imageFullPath).WriteAsync(imageFromCache);
                //await File.WriteAllBytesAsync(imageFullPath, imageFromCache);

                memoryCache.Remove("deleted-image");

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Cache image for 5 (default) minutes.
        /// </summary>
        private async Task CacheImage(string absolutePath)
        {
            try
            {
                var img = await File.ReadAllBytesAsync(absolutePath);

                var imgInCache = memoryCache.Get<byte[]>("deleted-image");

                if (imgInCache is not null) memoryCache.Remove("deleted-image");

                memoryCache.Set("deleted-image", img, new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.UtcNow.AddMinutes(5),
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)

                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
