using System.Text;
using Microsoft.AspNetCore.Http;
using UglyToad.PdfPig;

namespace PCShop.Helpers
{
    public static class PdfExtractor
    {
        public static string ExtractText(IFormFile pdfFile)
        {
            if (pdfFile == null || pdfFile.Length == 0) return string.Empty;

            var textBuilder = new StringBuilder();

            try
            {
                using (var stream = pdfFile.OpenReadStream())
                using (var document = PdfDocument.Open(stream))
                {
                    foreach (var page in document.GetPages())
                    {
                        textBuilder.Append(page.Text).Append(" ");
                    }
                }
            }
            catch
            {
                return string.Empty;
            }

            return textBuilder.ToString();
        }
    }
}