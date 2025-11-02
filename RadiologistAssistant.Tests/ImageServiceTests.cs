using System;
using System.IO;
using RadiologistAssistant.Services;
using Xunit;

namespace RadiologistAssistant.Tests
{
    public class ImageServiceTests
    {
        private readonly ImageService _imageService;

        public ImageServiceTests()
        {
            _imageService = new ImageService();
        }

        [Fact]
        public void GetImagePaths_ThrowsException_WhenDirectoryNotFound()
        {
            // Arrange
            var invalidDirectory = "non_existent_directory";

            // Act & Assert
            Assert.Throws<DirectoryNotFoundException>(() => _imageService.GetImagePaths(invalidDirectory));
        }

        [Fact]
        public void EncodeImageToBase64_ReturnsValidBase64String()
        {
            // Arrange
            var tempImagePath = Path.GetTempFileName();
            File.WriteAllBytes(tempImagePath, new byte[] { 1, 2, 3 });

            // Act
            var base64String = _imageService.EncodeImageToBase64(tempImagePath);

            // Assert
            Assert.False(string.IsNullOrEmpty(base64String));
            var bytes = Convert.FromBase64String(base64String);
            Assert.Equal(new byte[] { 1, 2, 3 }, bytes);

            // Clean up
            File.Delete(tempImagePath);
        }
    }
}
