Subspace.Imaging is a .NET imaging library, currently aimed at resizing images.

It has elaborate image resizing options, especially useful for creating thumbnail and preview images that must their retain aspect ratios:

**Extend Constrained**

Constrains the image to fit within the specified dimensions. Images that are larger
than the specified dimensions are shrunk while images that are smaller are left
unchanged. The aspect ratio of the image is maintained. The output dimensions are
equal to the specified dimensions and bars may be added in order to extend the
image when the aspect ratio of the specified dimensions does not match the aspect
ratio of the image.

**ExtendScaled**
   
Scales the image to fit within the specified dimensions. Images that are larger
than the specified dimensions are shrunk while images that are smaller are
enlarged. The aspect ratio of the image is maintained. The output dimensions are
equal to the specified dimensions and bars may be added in order to extend the
image when the aspect ratio of the specified dimensions does not match the aspect
ratio of the image.

**Constrain**

Constrains the image to fit within the specified dimensions. Images that are larger
than the specified dimensions are shrunk while images that are smaller are left
unchanged. The aspect ratio of the image is maintained. The output dimensions may
be smaller than the specified dimensions in effect.

**Scale**

Scales the image to fit within the specified dimensions. Images that are larger
than the specified dimensions are shrunk while images that are smaller are
enlarged. The aspect ratio of the image is maintained. The output dimensions may
be smaller than the specified dimensions in effect.

**Stretch**

Stretches the image to exactly fit the specified dimensions. The aspact ratio of
the image may change.

**Fix Width**

Resizes the image ensuring that the output width equals the specified width. The
height of the image varies depending on the aspect ratio of the image. The aspect
ratio of the image is maintained.

**FixHeight**

Resizes the image ensuring that the output height equals the specified height. The
width of the image varies depending on the aspect ratio of the image. The aspect
ratio of the image is maintained.

**Fill**

Resizes the image to match the specified dimensions while maintaining the aspect
ratio of the image. As a result, some areas of the image may be cropped.
