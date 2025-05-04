using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;


namespace WpfApp.Services
{
    internal class ImgBBService
    {
        private const string API_KEY = "f4f86515362892ae57c23e650747804b"; // Replace with your actual ImgBB API key
        private const string UPLOAD_URL = "https://api.imgbb.com/1/upload";

        private static readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Uploads an image to ImgBB from a byte array
        /// </summary>
        /// <param name="imageData">The image as a byte array</param>
        /// <param name="imageName">Optional name for the image</param>
        /// <returns>URL of the uploaded image or null if upload failed</returns>
        public async Task<string> UploadImageAsync(byte[] imageData, string imageName = null)
        {
            try
            {
                // Convert the image data to base64
                string base64Image = Convert.ToBase64String(imageData);

                // Create form content for the request
                var formContent = new MultipartFormDataContent();
                formContent.Add(new StringContent(API_KEY), "key");
                formContent.Add(new StringContent(base64Image), "image");

                if (!string.IsNullOrEmpty(imageName))
                {
                    formContent.Add(new StringContent(imageName), "name");
                }

                // Send request to ImgBB API
                var response = await _httpClient.PostAsync(UPLOAD_URL, formContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ImgBBResponse>(responseContent);

                    if (result?.Data?.Url != null)
                    {
                        return result.Data.Url;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading image to ImgBB: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Uploads an image to ImgBB from a file
        /// </summary>
        /// <param name="filePath">Path to the image file</param>
        /// <returns>URL of the uploaded image or null if upload failed</returns>
        public async Task<string> UploadImageFromFileAsync(string filePath)
        {
            try
            {
                byte[] imageData = File.ReadAllBytes(filePath);
                string fileName = Path.GetFileName(filePath);
                return await UploadImageAsync(imageData, fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading image file: {ex.Message}");
                return null;
            }
        }

        // Response classes for JSON deserialization
        private class ImgBBResponse
        {
            public ImgBBData Data { get; set; }
            public bool Success { get; set; }
            public int Status { get; set; }
        }

        private class ImgBBData
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Url { get; set; }
            public string Display_url { get; set; }
            public int Size { get; set; }
            public string Time { get; set; }
            public string Expiration { get; set; }
            public ImgBBImage Image { get; set; }
            public ImgBBImage Thumb { get; set; }
            public ImgBBImage Medium { get; set; }
            public ImgBBImage Delete_url { get; set; }
        }

        private class ImgBBImage
        {
            public string Filename { get; set; }
            public string Name { get; set; }
            public string Mime { get; set; }
            public string Extension { get; set; }
            public string Url { get; set; }
        }

    }
}
