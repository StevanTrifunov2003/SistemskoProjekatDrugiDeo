using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemskoProgramiranjeZad11
{
    public class Kes
    {
        // Dictionary za kesiranje slika
        private static readonly Dictionary<string, CachedImage> imageCache = new Dictionary<string, CachedImage>();

        // Struktura koja predstavlja kesiranu sliku sa vremenom isteka
        private struct CachedImage
        {
            public Bitmap Image;
            public DateTime ExpirationTime;
        }

        public static Bitmap GetCachedImage(string filePath)
        {
            lock (imageCache)
            {
                if (imageCache.ContainsKey(filePath))
                {
                    if (DateTime.Now <= imageCache[filePath].ExpirationTime)
                    {
                        return new Bitmap(imageCache[filePath].Image);
                    }
                    else
                    {
                        imageCache.Remove(filePath);
                    }
                }
                return null;
            }
        }

        public static void AddImageToCache(string filePath, Bitmap image, TimeSpan expirationTime)
        {
            lock (imageCache)
            {
                imageCache[filePath] = new CachedImage
                {
                    Image = new Bitmap(image),
                    ExpirationTime = DateTime.Now + expirationTime
                };
            }
        }
    }
}
