using FSO.Content.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;

namespace FSO.Server.Utils
{
    /// <summary>
    /// Helper class with a function for fetching images into textures
    /// </summary>
    public class CoreImageLoader
    {
        /// <summary>
        /// Fetch image from stream into a texture
        /// </summary>
        /// <param name="stream">Stream containing</param>
        /// <param name="texRef">Unused</param>
        /// <returns>New texture</returns>
        public static TexBitmap SoftImageFetch(Stream stream, AbstractTextureRef texRef)
        {
            Image<Rgba32> image = null;
            try
            {
                using (var img = Image.Load(stream))
                {
                    image = img.CloneAs<Rgba32>();
                }
            }
            catch (Exception)
            {
                return new TexBitmap() { Data = new byte[0] };
            }
            stream.Close();
            
            if (image == null)
                return null;

            byte[] pixelArray = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixelArray);
            image.Dispose();
            return new TexBitmap
            {
                Data = pixelArray,
                Width = image.Width,
                Height = image.Height,
                PixelSize = 4
            };
        }
    }
}
