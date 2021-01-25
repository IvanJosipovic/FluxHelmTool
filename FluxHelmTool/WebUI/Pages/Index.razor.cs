using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.MobileBlazorBindings;
using Microsoft.MobileBlazorBindings.Elements;
using Xamarin.Essentials;
using Xamarin.Forms;
using Agno.BlazorInputFile;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using FluxHelmTool.WebUI.Shared;
using System.IO;
using System.Net.Http;
using System.Net;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using FluxHelmTool;
using BlazorMonacoYaml;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace FluxHelmTool.WebUI.Pages
{
    public partial class Index
    {
        public string status;
        private MonacoDiffEditorYaml _yamlDiffEditor { get; set; }

        public YamlNode left;

        public YamlNode right;

        public string chartRepo;

        public string chartVersion;

        public string chartName;

        async Task HandleSelection(IFileListEntry[] files)
        {
            var file = files.FirstOrDefault();
            if (file != null)
            {
                var ms = new MemoryStream();
                await file.Data.CopyToAsync(ms);
                ms.Position = 0;

                TextReader stringStream = new StreamReader(ms);
                var yaml = new YamlStream();
                yaml.Load(stringStream);

                left = yaml.Documents[0].RootNode;

                var serializer = new SerializerBuilder().Build();

                var yamlStr = new StringWriter();

                serializer.Serialize(yamlStr, left);

                await _yamlDiffEditor.SetOriginalValue(yamlStr.ToString());

                GetChartInfo();
            }
        }

        public void GetChartInfo()
        {
            if (left == null) return;

            YamlMappingNode spec = ((YamlMappingNode)left).Children[new YamlScalarNode("spec")] as YamlMappingNode;
            YamlMappingNode chart = spec.Children[new YamlScalarNode("chart")] as YamlMappingNode;

            var repo = chart.Children[new YamlScalarNode("repository")] as YamlScalarNode;
            chartRepo = repo.Value;

            var name = chart.Children[new YamlScalarNode("name")] as YamlScalarNode;
            chartName = name.Value;

            var version = chart.Children[new YamlScalarNode("version")] as YamlScalarNode;
            chartVersion = version.Value;
        }

        public async Task GetRemoteChart()
        {
            //"https://charts.bitnami.com/bitnami/external-dns-4.5.4.tgz"

            var client = new HttpClient();

            var stream = await client.GetStreamAsync(chartRepo.TrimEnd('/') + "/" + chartName + "-" + chartVersion + ".tgz");

            Stream gzipStream = new GZipInputStream(stream);

            using (var tarInputStream = new TarInputStream(gzipStream))
            {
                TarEntry entry;
                while ((entry = tarInputStream.GetNextEntry()) != null)
                {
                    if (entry.Name.EndsWith("values.yaml"))
                    {
                        using (var fileContents = new MemoryStream())
                        {
                            tarInputStream.CopyEntryContents(fileContents);
                            fileContents.Position = 0;
                            TextReader stringStream = new StreamReader(fileContents);
                            
                            await _yamlDiffEditor.SetModifiedValue(stringStream.ReadToEnd());
                        }
                        break;
                    }
                }
            }
        }

        async Task myButton()
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://charts.bitnami.com/bitnami/index.yaml");
            HttpResponseMessage response = await new HttpClient().SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
            var stream = await response.Content.ReadAsStreamAsync();
            Console.WriteLine("Response Length: " + response.Content.Headers.ContentLength);
            var streamReader = new StreamReader(stream);
            var yaml = new YamlStream();
            yaml.Load(streamReader);

            Console.WriteLine(yaml.Documents.Count);

            // Examine the stream
            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

            foreach (KeyValuePair<YamlNode, YamlNode> item in ((YamlMappingNode)mapping.Children[new YamlScalarNode("entries")]).Children)
            {
                //status += " - " + item.Children[new YamlScalarNode("name")];
            }
        }
    }
}