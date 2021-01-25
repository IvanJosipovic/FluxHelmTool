using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agno.BlazorInputFile;
using System.IO;
using System.Net.Http;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
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

        public Dictionary<string, string> chartVersions = new Dictionary<string, string>();

        public string selectedVersion;

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

                await GetChartVersions();

                await GetRemoteChart();
            }
        }

        public void GetChartInfo()
        {
            YamlMappingNode spec = ((YamlMappingNode)left).Children[new YamlScalarNode("spec")] as YamlMappingNode;
            YamlMappingNode chart = spec.Children[new YamlScalarNode("chart")] as YamlMappingNode;

            var repo = chart.Children[new YamlScalarNode("repository")] as YamlScalarNode;
            chartRepo = repo.Value;

            var name = chart.Children[new YamlScalarNode("name")] as YamlScalarNode;
            chartName = name.Value;

            var version = chart.Children[new YamlScalarNode("version")] as YamlScalarNode;
            chartVersion = version.Value;
            selectedVersion = chartVersion;
        }

        async Task GetChartVersions()
        {
            var stream = await new HttpClient().GetStreamAsync(chartRepo.TrimEnd('/') + "/index.yaml");
            var streamReader = new StreamReader(stream);
            var yaml = new YamlStream();
            yaml.Load(streamReader);

            var mapping = yaml.Documents[0].RootNode as YamlMappingNode;

            var items = (mapping.Children[new YamlScalarNode("entries")] as YamlMappingNode).Children[new YamlScalarNode(chartName)] as YamlSequenceNode;

            chartVersions.Clear();

            foreach (YamlNode item in items)
            {
                var version = (item[new YamlScalarNode("version")] as YamlScalarNode).Value;

                var url = ((item[new YamlScalarNode("urls")] as YamlSequenceNode).Children.First() as YamlScalarNode).Value;
                chartVersions.Add(version, url);
            }
        }

        public async Task GetRemoteChart()
        {
            var client = new HttpClient();

            var stream = await client.GetStreamAsync(chartVersions.GetValueOrDefault(selectedVersion));

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

                            var output = new StringWriter();
                            string line = null;
                            while ((line = stringStream.ReadLine()) != null)
                            {
                                output.WriteLine("    " + line);
                            }

                            await _yamlDiffEditor.SetModifiedValue(output.ToString());
                        }
                        break;
                    }
                }
            }
        }
    }
}