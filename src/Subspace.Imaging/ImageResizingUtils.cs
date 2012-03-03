// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Subspace" file="ImageResizingUtils.cs">
//   Copyright (c) Subspace. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Subspace.Imaging
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;

    /// <summary>
    ///     Provides imaging utility methods.
    /// </summary>
    public static class ImageResizingUtils
    {
        private const string ArgumentOutOfRangeException_MustEqualOneOrLarger = "Must be equal to or larger than 1.";
        private const string ArgumentException_PathDoesNotExist = "The path does not exist.";
        private const string ArgumentException_CannotBeNullOrEmptyOrWhitespaceOnlyString = "Cannot be null, nor empty nor a string consisting only of whitespace characters.";

        /// <summary>
        ///     Gets a thumbnail image that represents the specified <paramref name="image"/>.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="width">The thumbnail image width.</param>
        /// <param name="height">The thumbnail image height.</param>
        /// <returns>The thumbnail image.</returns>
        public static Image GetThumbnail(Image image, int width, int height)
        {
            Require<ArgumentNullException>(image != null, "image");
            Require<ArgumentOutOfRangeException>(width > 0, "width");
            Require<ArgumentOutOfRangeException>(height > 0, "height");

            return image.GetThumbnailImage(
                width,
                height,
                () => false,
                IntPtr.Zero);
        }

        /// <summary>
        ///     Resizes the specified image.
        /// </summary>
        /// <param name="image">The input image.</param>
        /// <param name="zoomLevel">The zoom level.</param>
        /// <returns>The resized <see cref="Bitmap"/>.</returns>
        public static Bitmap ResizeImage(Image image, float zoomLevel)
        {
            Require<ArgumentNullException>(image != null, "image");
            Require<ArgumentOutOfRangeException>(zoomLevel > 0.0f, "zoomLevel");

            int width = (int)((float)image.Width * zoomLevel);
            int height = (int)((float)image.Height * zoomLevel);

            return ResizeImage(image, width, height, ImageResizeMode.Scale);
        }

        /// <summary>
        ///     Resizes the specified image.
        /// </summary>
        /// <param name="image">The input image.</param>
        /// <param name="width">The output image width.</param>
        /// <param name="height">The output image height.</param>
        /// <param name="resizeMode">The <see cref="ImageResizeMode"/>.</param>
        /// <param name="keepTransparency">A value indicating whether to keep transparency information from the source image.</param>
        /// <returns>The resized <see cref="Bitmap"/>.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height, ImageResizeMode resizeMode = ImageResizeMode.Default, bool keepTransparency = true)
        {
            return ResizeImage(image, width, height, resizeMode, keepTransparency, Color.White, Color.White);
        }

        /// <summary>
        ///     Resizes the specified image.
        /// </summary>
        /// <param name="image">The input image.</param>
        /// <param name="width">The output image width or 0 if the specified resize mode is <see cref="ImageResizeMode.FixHeight"/>.</param>
        /// <param name="height">The output image height or 0 if the specified resize mode is <see cref="ImageResizeMode.FixWidth"/>.</param>
        /// <param name="resizeMode">The <see cref="ImageResizeMode"/>.</param>
        /// <param name="keepTransparency">A value indicating whether to keep transparency information from the source image.</param>
        /// <param name="backgroundColor">The background color.</param>
        /// <param name="barColor">The color of the bars that are used when the aspect ratio of the input image differs from the specified output dimensions.</param>
        /// <returns>The resized <see cref="Bitmap"/>.</returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="image"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     The specified <paramref name="width"/> is zero or negative.
        ///     -or-
        ///     The specified <paramref name="height"/> is zero or negative.
        /// </exception>
        public static Bitmap ResizeImage(Image image, int width, int height, ImageResizeMode resizeMode, bool keepTransparency, Color backgroundColor, Color barColor)
        {
            Require<ArgumentNullException>(barColor != null, "barColor");
            Require<ArgumentNullException>(backgroundColor != null, "backgroundColor");

            using (Brush backgroundBrush = new SolidBrush(backgroundColor))
            using (Brush barBrush = new SolidBrush(barColor))
            {
                return ResizeImage(image, width, height, resizeMode, keepTransparency, backgroundBrush, barBrush);
            }
        }

        /// <summary>
        ///     Resizes the specified image.
        /// </summary>
        /// <param name="image">The input image.</param>
        /// <param name="width">The output image width or 0 if the specified resize mode is <see cref="ImageResizeMode.FixHeight"/>.</param>
        /// <param name="height">The output image height or 0 if the specified resize mode is <see cref="ImageResizeMode.FixWidth"/>.</param>
        /// <param name="resizeMode">The <see cref="ImageResizeMode"/>.</param>
        /// <param name="keepTransparency">A value indicating whether to keep transparency information from the source image.</param>
        /// <param name="backgroundBrush">The background brush.</param>
        /// <param name="barBrush">The brush of the bars that are used when the aspect ratio of the input image differs from the specified output dimensions.</param>
        /// <returns>The resized <see cref="Bitmap"/>.</returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="image"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     The specified <paramref name="width"/> is zero or negative.
        ///     -or-
        ///     The specified <paramref name="height"/> is zero or negative.
        /// </exception>
        public static Bitmap ResizeImage(Image image, int width, int height, ImageResizeMode resizeMode, bool keepTransparency, Brush backgroundBrush, Brush barBrush)
        {
            Require<ArgumentNullException>(image != null, "image");
            Require<ArgumentOutOfRangeException>(resizeMode == ImageResizeMode.FixHeight || width > 0, "width");
            Require<ArgumentOutOfRangeException>(resizeMode != ImageResizeMode.FixHeight || width == 0, "width");
            Require<ArgumentOutOfRangeException>(resizeMode == ImageResizeMode.FixWidth || height > 0, "height");
            Require<ArgumentOutOfRangeException>(resizeMode != ImageResizeMode.FixWidth || height == 0, "height");
            Require<ArgumentNullException>(backgroundBrush != null, "backgroundBrush");
            Require<ArgumentNullException>(barBrush != null, "barBrush");

            // Imply operation flags from the specified resize mode.
            bool maintainAspectRatio = false;
            bool allowUpscaling = false;
            bool fixWidth = false;
            bool fixHeight = false;
            bool fill = false;
            bool bars = false;

            if (resizeMode == ImageResizeMode.Constrain)
            {
                maintainAspectRatio = true;
            }
            else if (resizeMode == ImageResizeMode.ExtendConstrained)
            {
                maintainAspectRatio = true;
                bars = true;
            }
            else if (resizeMode == ImageResizeMode.ExtendScaled)
            {
                maintainAspectRatio = true;
                allowUpscaling = true;
                bars = true;
            }
            else if (resizeMode == ImageResizeMode.Fill)
            {
                maintainAspectRatio = true;
                allowUpscaling = true;
                fill = true;
            }
            else if (resizeMode == ImageResizeMode.FixHeight)
            {
                maintainAspectRatio = true;
                allowUpscaling = true;
                fixHeight = true;
            }
            else if (resizeMode == ImageResizeMode.FixWidth)
            {
                maintainAspectRatio = true;
                allowUpscaling = true;
                fixWidth = true;
            }
            else if (resizeMode == ImageResizeMode.Scale)
            {
                maintainAspectRatio = true;
                allowUpscaling = true;
            }
            else if (resizeMode == ImageResizeMode.Stretch)
            {
                allowUpscaling = true;
            }

            if (width < 1 && !fixHeight)
            {
                throw new ArgumentOutOfRangeException("width", ArgumentOutOfRangeException_MustEqualOneOrLarger);
            }
            else if (height < 1 && !fixWidth)
            {
                throw new ArgumentOutOfRangeException("height", ArgumentOutOfRangeException_MustEqualOneOrLarger);
            }

            float inputWidth = image.Width;
            float inputHeight = image.Height;
            float outputWidth = width;
            float outputHeight = height;

            float renderLeft = 0;
            float renderTop = 0;
            float renderWidth = 0;
            float renderHeight = 0;

            Bitmap outputImage = null;

            if (maintainAspectRatio)
            {
                // Fill the image to completely cover the requested output dimensions,
                // at the cost of some probable cropped image data.
                if (fill)
                {
                    renderWidth = inputWidth * (outputWidth / inputWidth);
                    renderHeight = inputHeight * (outputWidth / inputWidth);

                    if (renderHeight < outputHeight)
                    {
                        renderWidth *= outputHeight / renderHeight;
                        renderHeight *= outputHeight / renderHeight;
                    }
                }
                else
                {
                    // Calculate the render dimensions maintaining the aspect ratio.
                    renderWidth = inputWidth;
                    renderHeight = inputHeight;

                    if (renderWidth < outputWidth && allowUpscaling)
                    {
                        renderHeight *= outputWidth / renderWidth;
                        renderWidth = outputWidth;
                    }

                    if (renderHeight < outputHeight && allowUpscaling)
                    {
                        renderWidth *= outputHeight / renderHeight;
                        renderHeight = outputHeight;
                    }

                    if (renderWidth > outputWidth && !fixHeight)
                    {
                        renderHeight *= outputWidth / renderWidth;
                        renderWidth = outputWidth;
                    }

                    if (renderHeight > outputHeight && !fixWidth)
                    {
                        renderWidth *= outputHeight / renderHeight;
                        renderHeight = outputHeight;
                    }

                    if (fixHeight || !bars)
                    {
                        outputWidth = renderWidth;
                    }

                    if (fixWidth || !bars)
                    {
                        outputHeight = renderHeight;
                    }
                }
            }
            else
            {
                if (allowUpscaling)
                {
                    renderWidth = outputWidth;
                    renderHeight = outputHeight;
                }
                else
                {
                    renderWidth = (inputWidth > outputWidth) ? outputWidth : inputWidth;
                    renderHeight = (inputHeight > outputHeight) ? outputHeight : inputHeight;
                }
            }

            // Center the image.
            renderLeft = (outputWidth / 2) - (renderWidth / 2);
            renderTop = (outputHeight / 2) - (renderHeight / 2);

            outputImage = new Bitmap((int)Math.Round(outputWidth), (int)Math.Round(outputHeight), PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(outputImage))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.High;

                if (!keepTransparency)
                {
                    graphics.FillRectangle(backgroundBrush, new Rectangle(0, 0, outputImage.Width, outputImage.Height));

                    if (bars)
                    {
                        DrawAspectCompensationBars(barBrush, outputWidth, outputHeight, renderLeft, renderTop, renderWidth, renderHeight, graphics);
                    }
                }

                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.DrawImage(image, renderLeft, renderTop, renderWidth, renderHeight);
            }

            return outputImage;
        }

        /// <summary>
        ///     Rotates the specified image, keeping the dimensions of the specified image.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="angle">The number of degrees to rotate.</param>
        /// <param name="restrictDimensions">A value indicating whether to ensure that the dimensions of the resulting image do not exceed those of the specified image.</param>
        /// <param name="backgroundBrush">The background brush to use when the rotation results in an image that has a different aspect ratio or <c>null</c> to refrain from drawing a background at all times.</param>
        /// <param name="borderPen">The border pen to use when the rotation results in an image that has a different aspect ratio or <c>null</c> to refrain from drawing a border at all times.</param>
        /// <returns>The rotated image.</returns>
        public static Image RotateImage(Image image, float angle, bool restrictDimensions = false, Brush backgroundBrush = null, Pen borderPen = null)
        {
            Require<ArgumentNullException>(image != null, "image");
            Require<ArgumentOutOfRangeException>(angle >= 0.0f && angle < 360.0f, "angle");

            if (angle == 0.0f)
            {
                return (Image)image.Clone();
            }

            float w = image.Width;
            float h = image.Height;
            float x = w / 2.0f;
            float y = h / 2.0f;

            Bitmap resultBitmap = new Bitmap((int)w, (int)h);

            if (restrictDimensions)
            {
                using (Graphics graphics = Graphics.FromImage(resultBitmap))
                {
                    if (backgroundBrush != null)
                    {
                        graphics.FillRectangle(backgroundBrush, 0.0f, 0.0f, w, h);
                    }

                    graphics.TranslateTransform(x, y);
                    graphics.RotateTransform(angle);

                    float horizontalZoom = w / graphics.VisibleClipBounds.Width;
                    float verticalZoom = h / graphics.VisibleClipBounds.Height;
                    float zoom = Math.Min(horizontalZoom, verticalZoom);

                    graphics.ScaleTransform(zoom, zoom);
                    graphics.TranslateTransform(-x, -y);
                    graphics.DrawImage(image, 0.0f, 0.0f);

                    if (borderPen != null && zoom != 1.0f)
                    {
                        graphics.DrawRectangle(borderPen, 0.0f, 0.0f, w, h);
                    }
                }
            }
            else
            {
                // Determine the final image size.
                using (Graphics graphics = Graphics.FromImage(resultBitmap))
                {
                    graphics.TranslateTransform(x, y);
                    graphics.RotateTransform(angle);

                    resultBitmap = new Bitmap((int)graphics.VisibleClipBounds.Width, (int)graphics.VisibleClipBounds.Height);
                    resultBitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                }

                using (Graphics graphics = Graphics.FromImage(resultBitmap))
                {
                    float w2 = resultBitmap.Width;
                    float h2 = resultBitmap.Height;
                    float x2 = w2 / 2.0f;
                    float y2 = h2 / 2.0f;

                    bool matchingAspect = w == w2 && h == h2;

                    if (backgroundBrush != null && !matchingAspect)
                    {
                        graphics.FillRectangle(backgroundBrush, 0.0f, 0.0f, w2, h2);
                    }

                    graphics.TranslateTransform(x2, y2);
                    graphics.RotateTransform(angle);
                    graphics.TranslateTransform(-x, -y);
                    graphics.DrawImage(image, 0.0f, 0.0f);

                    if (borderPen != null && !matchingAspect)
                    {
                        graphics.DrawRectangle(borderPen, 0.0f, 0.0f, w, h);
                    }
                }
            }

            return resultBitmap;
        }

        /// <summary>
        ///     Returns the MIME type of the specified image.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>A mime type string.</returns>
        public static string GetMimeType(Image image)
        {
            Require<ArgumentNullException>(image != null, "image");

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
            Require<ArgumentNullException>(stream != null, "stream");

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
            Require<ArgumentException>(!string.IsNullOrWhiteSpace(path), "path", ArgumentException_CannotBeNullOrEmptyOrWhitespaceOnlyString);

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

        /// <summary>
        ///     Draws bars to conceil the empty space that appears after resizing an image to
        ///     dimensions that have a different aspect ratio.
        /// </summary>
        /// <param name="barBrush">The bar brush.</param>
        /// <param name="outputWidth">The output width.</param>
        /// <param name="outputHeight">The output height.</param>
        /// <param name="renderLeft">The render left coordinate.</param>
        /// <param name="renderTop">The render top coordinate.</param>
        /// <param name="renderWidth">The rendering width.</param>
        /// <param name="renderHeight">The rendering height.</param>
        /// <param name="graphics">The graphics surface.</param>
        private static void DrawAspectCompensationBars(Brush barBrush, float outputWidth, float outputHeight, float renderLeft, float renderTop, float renderWidth, float renderHeight, Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.None;

            if (renderTop >= 1)
            {
                graphics.FillRectangle(barBrush, 0, 0, outputWidth, renderTop);
                graphics.FillRectangle(barBrush, 0, renderTop + renderHeight, outputWidth, outputHeight - renderHeight - renderTop);
            }

            if (renderLeft >= 1)
            {
                graphics.FillRectangle(barBrush, 0, 0, renderLeft, outputHeight);
                graphics.FillRectangle(barBrush, renderLeft + renderWidth, 0, outputWidth - renderWidth - renderLeft, outputHeight);
            }
        }

        /// <summary>
        ///     Throws an exception of the specified type if the specified condition is false.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <param name="condition">The condition.</param>
        /// <param name="argumentName">The name of the argument, if applicable for the specified exception.</param>
        /// <param name="message">The message.</param>
        private static void Require<TException>(bool condition, string argumentName = null, string message = null)
            where TException : Exception, new()
        {
            if (!condition)
            {
                if (argumentName == null)
                {
                    throw (Exception)Activator.CreateInstance(typeof(TException), message ?? string.Empty);
                }

                throw (Exception)Activator.CreateInstance(
                    typeof(TException),
                    message ?? string.Empty,
                    argumentName);
            }
        }
    }
}
