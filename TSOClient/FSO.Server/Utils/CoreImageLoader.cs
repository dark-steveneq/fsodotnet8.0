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
            Image<Rgba32> result = null;
            try
            {
                result = Image.Load(stream);
            }
            catch (Exception)
            {
                return new TexBitmap() { Data = new byte[0] };
            }
            stream.Close();
            
            if (result == null) return null;

            return new TexBitmap
            {
                Data = result.SavePixelData(),
                Width = result.Width,
                Height = result.Height,
                PixelSize = 4
            };
        }
    }
}
