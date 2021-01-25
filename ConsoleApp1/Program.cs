using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace ConsoleApp1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://charts.bitnami.com/bitnami/index.yaml");

            HttpResponseMessage response = await new HttpClient().SendAsync(requestMessage);

            var stream = await response.Content.ReadAsStreamAsync();
            Console.WriteLine("Response Length: " + response.Content.Headers.ContentLength);

            var streamReader = new StreamReader(stream);
            var yaml = new YamlStream();
            yaml.Load(streamReader);

            Console.WriteLine(yaml.Documents.Count);
        }
    }
}
