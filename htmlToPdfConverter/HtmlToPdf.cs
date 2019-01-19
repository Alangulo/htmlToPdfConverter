
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;
using DinkToPdf;
using System;
using IPdfConverter = DinkToPdf.Contracts.IConverter;

namespace htmlToPdfConverter
{
    public static class HtmlToPdf
    {
        [FunctionName("HtmlToPdf")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequest req, TraceWriter log)
        {
            string url = req.Query["url"];
            log.Info($"Converting {req} to PDF");

            var html = await FetchHtml(url);
            var pdfBytes = BuildPdf(html);
            var response = BuildResponse(pdfBytes);

            return response;
        }

        private static FileContentResult BuildResponse(byte[] pdfBytes)
        {
            return new FileContentResult(pdfBytes, "application/pdf");
        }

        private static byte[] BuildPdf(string html)
        {
            return pdfConverter.Convert(new HtmlToPdfDocument()
            {
                Objects =
                {
                    new ObjectSettings
                    {
                        HtmlContent = html
                    }
                }
            });
        }

        private static async Task<string> FetchHtml(string url)
        {
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"FetchHtml failed {response.StatusCode} : {response.ReasonPhrase}");
            }
            return await response.Content.ReadAsStringAsync();
        }

        static HttpClient httpClient = new HttpClient();
        static IPdfConverter pdfConverter = new SynchronizedConverter(new PdfTools());
    }
}
