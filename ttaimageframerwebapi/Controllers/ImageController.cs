namespace ttaimageframerwebapi.Controllers
{
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return new OkResult();
        }

        [HttpPost]
        public IActionResult UploadImage(IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                using (var uploadedImageStream = imageFile.OpenReadStream())
                {
                    using (var uploadedImage = new Bitmap(uploadedImageStream))
                    {
                        // Check the image's orientation
                        if (uploadedImage.PropertyIdList.Contains(0x112))
                        {
                            var orientation = (int)uploadedImage.GetPropertyItem(0x112).Value[0];
                            switch (orientation)
                            {
                                case 1:
                                    // No rotation needed
                                    break;
                                case 3:
                                    // 180 degrees rotation
                                    uploadedImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                    break;
                                case 6:
                                    // 90 degrees rotation to the right
                                    uploadedImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                    break;
                                case 8:
                                    // 90 degrees rotation to the left
                                    uploadedImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                    break;
                                    // Add more cases if needed for other orientations
                            }
                        }

                        // Create a new Bitmap with the desired dimensions (1667x1667)
                        using (var resizedImage = new Bitmap(1667, 1667))
                        {
                            using (var graphics = Graphics.FromImage(resizedImage))
                            {
                                // Set the interpolation mode for resizing
                                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                graphics.DrawImage(uploadedImage, 0, 0, 1667, 1667);
                            }

                            // Load the predefined frame image
                            using (var frameImage = new Bitmap(@".\Images\frame.png"))
                            {
                                // Overlay the resized user's image with the frame
                                using (var graphics = Graphics.FromImage(resizedImage))
                                {
                                    graphics.DrawImage(frameImage, 0, 0, 1667, 1667);
                                }
                            }

                            // Save the resulting image
                            resizedImage.Save($"processed-{Guid.NewGuid()}.png");
                        }
                    }
                }
            }

            // Redirect to a page where the user can view the result
            return RedirectToAction("Result");
        }
    }

}
