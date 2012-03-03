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
    ///     Provides image resizing utility methods.
    /// </summary>
    public static class ImageResizingUtils
    {
        private const string ArgumentOutOfRangeException_MustEqualOneOrLarger = "Must be equal to or larger than 1.";

        /// <summary>
        ///     Gets a thumbnail image that represents the specified <paramref name="image"/>.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="width">The thumbnail image width.</param>
        /// <param name="height">The thumbnail image height.</param>
        /// <returns>The thumbnail image.</returns>
        public static Image GetThumbnail(Image image, int width, int height)
        {
            Check.Require<ArgumentNullException>(image != null, "image");
            Check.Require<ArgumentOutOfRangeException>(width > 0, "width");
            Check.Require<ArgumentOutOfRangeException>(height > 0, "height");

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
            Check.Require<ArgumentNullException>(image != null, "image");
            Check.Require<ArgumentOutOfRangeException>(zoomLevel > 0.0f, "zoomLevel");

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
            Check.Require<ArgumentNullException>(barColor != null, "barColor");
            Check.Require<ArgumentNullException>(backgroundColor != null, "backgroundColor");

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
            Check.Require<ArgumentNullException>(image != null, "image");
            Check.Require<ArgumentOutOfRangeException>(resizeMode == ImageResizeMode.FixHeight || width > 0, "width");
            Check.Require<ArgumentOutOfRangeException>(resizeMode != ImageResizeMode.FixHeight || width == 0, "width");
            Check.Require<ArgumentOutOfRangeException>(resizeMode == ImageResizeMode.FixWidth || height > 0, "height");
            Check.Require<ArgumentOutOfRangeException>(resizeMode != ImageResizeMode.FixWidth || height == 0, "height");
            Check.Require<ArgumentNullException>(backgroundBrush != null, "backgroundBrush");
            Check.Require<ArgumentNullException>(barBrush != null, "barBrush");

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
    }
}
