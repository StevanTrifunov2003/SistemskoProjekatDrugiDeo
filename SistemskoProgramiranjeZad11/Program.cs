using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SistemskoProgramiranjeZad11
{
    class Program
    {
        static async Task Main(string[] args)
        {
            const string rootFolder = "C:\\Images\\";
            const string baseUrl = "http://localhost:5050/";

            Console.WriteLine("Web server for converting a picture to grayscale is running...");
            Console.WriteLine($"Listening on {baseUrl}");

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(baseUrl);

            try
            {
                listener.Start();

                while (true)
                {
                    var context = await listener.GetContextAsync();
                    _ = HandleRequestAsync(context, rootFolder);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                listener.Close();
            }
        }

        static async Task HandleRequestAsync(HttpListenerContext context, string rootFolder)
        {
            string requestUrl = context.Request.Url.AbsolutePath;

            try
            {
                if (!requestUrl.StartsWith("/"))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.Close();
                    Console.WriteLine($"Bad request: {requestUrl}");
                    return;
                }

                string fileName = requestUrl.Substring(1); // Preskacemo prvi karakter ("/")
                string filePath = Path.Combine(rootFolder, fileName);

                if (!File.Exists(filePath))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.Close();
                    Console.WriteLine($"File not found: {fileName}");
                    return;
                }

                // Provera da li je slika vec kesirana
                Bitmap cachedImage = Kes.GetCachedImage(filePath);

                if (cachedImage != null)
                {
                    await SendImageResponseAsync(context, cachedImage);
                    Console.WriteLine($"Cached image sent: {fileName}");
                    return;
                }
              
                // Konvertovanje slike u crno-beli format
                using (var originalImage = new Bitmap(filePath))
                using (var grayscaleImage = new Bitmap(originalImage.Width, originalImage.Height))
                {
                    for (int y = 0; y < originalImage.Height; y++)
                    {
                        for (int x = 0; x < originalImage.Width; x++)
                        {
                            Color pixelColor = originalImage.GetPixel(x, y);
                            int grayscale = (int)(pixelColor.R * 0.3 + pixelColor.G * 0.59 + pixelColor.B * 0.11);
                            Color newColor = Color.FromArgb(grayscale, grayscale, grayscale);
                            grayscaleImage.SetPixel(x, y, newColor);
                        }
                    }
                    // Cuvanje konvertovane slike u kesu
                    Kes.AddImageToCache(filePath, grayscaleImage, new TimeSpan(0, 1, 0));

                    await SendImageResponseAsync(context, grayscaleImage);
                    Console.WriteLine($"Converted image sent: {fileName}");
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.Close();
                Console.WriteLine($"Internal server error {requestUrl}: {ex.Message}");
            }
        }

        static async Task SendImageResponseAsync(HttpListenerContext context, Bitmap image)
        {
            // Cuvanje konvertovane slike u memoriji
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Jpeg);
                byte[] imageBytes = ms.ToArray();

                // Slanje odgovora
                context.Response.ContentType = "image/jpeg";
                await context.Response.OutputStream.WriteAsync(imageBytes, 0, imageBytes.Length);
                context.Response.Close();
            }
        }

        

        
    }
}
