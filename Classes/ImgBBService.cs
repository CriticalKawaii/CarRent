using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace WpfApp.Classes
{
    public class ImgBBService
    {
        private const string ApiBaseUrl = "https://api.imgbb.com/1/upload";
        private readonly string _apiKey;

        public ImgBBService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<string> UploadImageAsync(string imagePath)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    byte[] imageBytes = File.ReadAllBytes(imagePath);
                    string base64Image = Convert.ToBase64String(imageBytes);

                    var formContent = new MultipartFormDataContent();
                    formContent.Add(new StringContent(_apiKey), "key");
                    formContent.Add(new StringContent(base64Image), "image");

                    var response = await client.PostAsync(ApiBaseUrl, formContent);
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            JObject jsonObj = JObject.Parse(jsonResponse);

                            bool success = jsonObj["success"]?.Value<bool>() ?? false;
                            if (success)
                            {
                                string imageUrl = jsonObj["data"]?["url"]?.Value<string>();
                                if (!string.IsNullOrEmpty(imageUrl))
                                {
                                    return imageUrl;
                                }
                                else
                                {
                                    throw new Exception("Image URL not found in response");
                                }
                            }
                            else
                            {
                                throw new Exception($"Upload was not successful: {jsonResponse}");
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Failed to parse API response: {ex.Message}");
                        }
                    }
                    else
                    {
                        throw new Exception($"ImgBB API error ({response.StatusCode}): {jsonResponse}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error uploading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"Upload exception: {ex}");
                return null;
            }
        }

        public async Task<List<string>> UploadImagesAsync(List<string> imagePaths)
        {
            List<string> uploadedUrls = new List<string>();

            foreach (var imagePath in imagePaths)
            {
                string url = await UploadImageAsync(imagePath);
                if (!string.IsNullOrEmpty(url))
                {
                    uploadedUrls.Add(url);
                }
            }

            return uploadedUrls;
        }
    }
}