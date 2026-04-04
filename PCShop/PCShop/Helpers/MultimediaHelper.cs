using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace PCShop.Helpers
{
    public static class MultimediaHelper
    {
        // --- 1. PROCESARE IMAGINE ---

        public static byte[] ProcessImage(byte[] imageBytes, List<string> operations, float blurRadius = 5f, float rotationDegrees = 90f, int scalePercent = 50, int cropWidth = 200, int cropHeight = 200)
        {
            // Daca nu a fost bifat nici un filtru, returnam imaginea originala
            if (operations == null || operations.Count == 0)
                return imageBytes;

            using var ms = new MemoryStream(imageBytes);
            using var image = Image.Load(ms);
            foreach (var operation in operations)
            {
                switch (operation.ToLower())
                {
                    case "blur":
                        image.Mutate(x => x.GaussianBlur(blurRadius > 0 ? blurRadius : 5f));
                        break;
                    case "rotate":
                        image.Mutate(x => x.Rotate(rotationDegrees));
                        break;
                    case "scale":
                        int newWidth = (int)(image.Width * (scalePercent / 100));
                        int newHeight = (int)(image.Height * (scalePercent / 100));

                        if (newWidth > 0 && newHeight > 0)
                        {
                            image.Mutate(x => x.Resize(image.Width / 2, image.Height / 2));
                        }
                        break;
                    case "grayscale":
                        image.Mutate(x => x.Grayscale());
                        break;
                    case "crop":
                        int cw = Math.Min(cropWidth > 0 ? cropWidth : 200, image.Width);
                        int ch = Math.Min(cropHeight > 0 ? cropHeight : 200, image.Height);

                        int cropX = (image.Width - cw) / 2;
                        int cropY = (image.Height - ch) / 2;

                        image.Mutate(x => x.Crop(new Rectangle(cropX, cropY, cw, ch)));
                        break;
                }

            }

            using var outStream = new MemoryStream();
            image.SaveAsJpeg(outStream);
            return outStream.ToArray();
        }

        // --- 2. ALGORITM COMPRESIE LZW ---

        public static List<int> CompressLZW(string uncompressed)
        {
            if (string.IsNullOrEmpty(uncompressed)) return new List<int>();

            int dictSize = 256;
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            for (int i = 0; i < 256; i++)
                dictionary.Add(((char)i).ToString(), i);

            string w = "";
            List<int> compressed = new List<int>();

            foreach (char c in uncompressed)
            {
                string wc = w + c;
                if (dictionary.ContainsKey(wc))
                {
                    w = wc;
                }
                else
                {
                    compressed.Add(dictionary[w]);
                    dictionary.Add(wc, dictSize++);
                    w = c.ToString();
                }
            }

            if (!string.IsNullOrEmpty(w))
                compressed.Add(dictionary[w]);

            return compressed;
        }
    }
}