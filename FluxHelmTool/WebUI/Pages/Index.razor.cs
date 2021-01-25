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

namespace FluxHelmTool.WebUI.Pages
{
    public partial class Index
    {
        public string status;
        private MonacoDiffEditorYaml _yamlDiffEditor { get; set; }

        public YamlNode left;

        public string leftValue;

        public YamlNode right;

        public string rightValue;

        void HandleSelection(IFileListEntry[] files)
        {
            var file = files.FirstOrDefault();
            if (file != null)
            {
                var streamReader = new StreamReader(file.Data);
                var yaml = new YamlStream();
                yaml.Load(streamReader);

                left = yaml.Documents[0].RootNode;

                var serializer = new SerializerBuilder().Build();

                var yamlStr = new StringWriter();

                serializer.Serialize(yamlStr, left);

                rightValue = yamlStr.ToString();
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