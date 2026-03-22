using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PCShop.Models;

namespace PCShop.Helpers
{
    public static class TfIdfSearch
    {
        public static List<Product> Search(List<Product> products, string query)
        {
            
            // List<string> queryTerms
            var queryTerms = Tokenize(query);
            if (!queryTerms.Any() || !products.Any()) return products;

            int totalProducts = products.Count;
            var ProductTokens = new Dictionary<int, List<string>>();

           
            foreach (var product in products)
            {
                var textToSearch = $"{product.Name} {product.Description}";
                ProductTokens[product.ProductId] = Tokenize(textToSearch);
            }

            var idfScores = new Dictionary<string, double>();
            foreach (var term in queryTerms.Distinct())
            {
                int productsWithTerm = ProductTokens.Values.Count(tokens => tokens.Contains(term));
                idfScores[term] = productsWithTerm > 0 ? Math.Log((double)totalProducts / productsWithTerm) : 0;
            }

            var productScores = new Dictionary<Product, double>();

            foreach (var product in products)
            {
                // List<string> tokens
                var tokens = ProductTokens[product.ProductId];
                int totalTokens = tokens.Count;
                double documentScore = 0;

                if (totalTokens == 0) continue;

                foreach (var term in queryTerms)
                {
                    int termCount = tokens.Count(t => t == term);
                    double tf = (double)termCount / totalTokens;
                    documentScore += tf * idfScores.GetValueOrDefault(term, 0);
                }

                
                if (documentScore > 0)
                {
                    productScores.Add(product, documentScore);
                }
            }

          
            return productScores
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        private static List<string> Tokenize(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return new List<string>();
            var cleanText = Regex.Replace(text.ToLowerInvariant(), @"[^a-z0-9\s]", "");
            return cleanText.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}
