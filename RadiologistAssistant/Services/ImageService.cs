using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RadiologistAssistant.Services
{
    public class ImageService
    {
        public List<string> GetImagePaths(string directory)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"Images directory not found: {directory}");
            }
            return Directory.GetFiles(directory).ToList();
        }

        public string EncodeImageToBase64(string imagePath)
        {
            var imageBytes = File.ReadAllBytes(imagePath);
            return Convert.ToBase64String(imageBytes);
        }
    }
}
