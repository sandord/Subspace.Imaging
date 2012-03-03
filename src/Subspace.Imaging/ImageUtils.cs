// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Subspace" file="ImageUtils.cs">
//   Copyright (c) Subspace. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Subspace.Imaging
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    /// <summary>
    ///     Provides imaging utility methods.
    /// </summary>
    public class ImageUtils
    {
        private const string ArgumentException_CannotBeNullOrEmptyOrWhitespaceOnlyString = "Cannot be null, nor empty nor a string consisting only of whitespace characters.";
        private const string ArgumentException_PathDoesNotExist = "The path does not exist.";

        /// <summary>
        ///     Returns the MIME type of the specified image.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>A mime type string.</returns>
        public static string GetMimeType(Image image)
        {
            Check.Require<ArgumentNullException>(image != null, "image");

            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageDecoders())
            {
                if (codec.FormatID == image.RawFormat.Guid)
                {
                    return codec.MimeType;
                }
            }

            return "image/unknown";
        }

        /// <summary>
        ///     Gets the image from the specified stream without retaining a file lock on the
        ///     stream source.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>An image.</returns>
        public static Image GetNonLockingImageFromStream(Stream stream)
        {
            Check.Require<ArgumentNullException>(stream != null, "stream");

            // After copying the file stream to a memory stream, the file stream can be closed
            // without disposing the image first, which would be necessary when using the file
            // stream directly.
            MemoryStream memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            return Image.FromStream(memoryStream);
        }

        /// <summary>
        ///     Gets the image from the specified file without retaining a lock on the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>An image.</returns>
        public static Image GetNonLockingImageFromFile(string path)
        {
            Check.Require<ArgumentException>(!string.IsNullOrWhiteSpace(path), "path", ArgumentException_CannotBeNullOrEmptyOrWhitespaceOnlyString);

            if (!File.Exists(path))
            {
                throw new ArgumentException(ArgumentException_PathDoesNotExist, "path");
            }

            using (FileStream fileStream = File.OpenRead(path))
            {
                MemoryStream memoryStream = new MemoryStream();
                fileStream.CopyTo(memoryStream);

                return Image.FromStream(memoryStream);
            }
        }
    }
}
