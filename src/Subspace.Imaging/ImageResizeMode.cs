// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Subspace" file="ImageResizeMode.cs">
//   Copyright (c) Subspace. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Subspace.Imaging
{
    /// <summary>
    ///     An enumeration used to specify an image resize mode.
    /// </summary>
    public enum ImageResizeMode
    {
        /// <summary>
        ///     Resizes the image using the <see cref="ImageResizeMode.ExtendConstrained"/> mode.
        /// </summary>
        Default = ExtendConstrained,

        /// <summary>
        ///     Constrains the image to fit within the specified dimensions. Images that are larger
        ///     than the specified dimensions are shrunk while images that are smaller are left
        ///     unchanged. The aspect ratio of the image is maintained. The output dimensions are
        ///     equal to the specified dimensions and bars may be added in order to extend the
        ///     image when the aspect ratio of the specified dimensions does not match the aspect
        ///     ratio of the image.
        /// </summary>
        ExtendConstrained = 0,

        /// <summary>
        ///     Scales the image to fit within the specified dimensions. Images that are larger
        ///     than the specified dimensions are shrunk while images that are smaller are
        ///     enlarged. The aspect ratio of the image is maintained. The output dimensions are
        ///     equal to the specified dimensions and bars may be added in order to extend the
        ///     image when the aspect ratio of the specified dimensions does not match the aspect
        ///     ratio of the image.
        /// </summary>
        ExtendScaled,

        /// <summary>
        ///     Constrains the image to fit within the specified dimensions. Images that are larger
        ///     than the specified dimensions are shrunk while images that are smaller are left
        ///     unchanged. The aspect ratio of the image is maintained. The output dimensions may
        ///     be smaller than the specified dimensions in effect.
        /// </summary>
        Constrain,

        /// <summary>
        ///     Scales the image to fit within the specified dimensions. Images that are larger
        ///     than the specified dimensions are shrunk while images that are smaller are
        ///     enlarged. The aspect ratio of the image is maintained. The output dimensions may
        ///     be smaller than the specified dimensions in effect.
        /// </summary>
        Scale,

        /// <summary>
        ///     Stretches the image to exactly fit the specified dimensions. The aspact ratio of
        ///     the image may change.
        /// </summary>
        Stretch,

        /// <summary>
        ///     Resizes the image ensuring that the output width equals the specified width. The
        ///     height of the image varies depending on the aspect ratio of the image. The aspect
        ///     ratio of the image is maintained.
        /// </summary>
        FixWidth,

        /// <summary>
        ///     Resizes the image ensuring that the output height equals the specified height. The
        ///     width of the image varies depending on the aspect ratio of the image. The aspect
        ///     ratio of the image is maintained.
        /// </summary>
        FixHeight,

        /// <summary>
        ///     Resizes the image to match the specified dimensions while maintaining the aspect
        ///     ratio of the image. As a result, some areas of the image may be cropped.
        /// </summary>
        Fill
    }
}