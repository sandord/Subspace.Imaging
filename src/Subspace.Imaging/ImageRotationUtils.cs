namespace Subspace.Imaging
{
    using System;
    using System.Drawing;

    /// <summary>
    ///     Contains image rotation utility methods.
    /// </summary>
    public static class ImageRotationUtils
    {
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
            Check.Require<ArgumentNullException>(image != null, "image");
            Check.Require<ArgumentOutOfRangeException>(angle >= 0.0f && angle < 360.0f, "angle");

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
    }
}
