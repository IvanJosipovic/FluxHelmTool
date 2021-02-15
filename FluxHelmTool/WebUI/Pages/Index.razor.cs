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
using System.Text;
using System;

namespace FluxHelmTool.WebUI.Pages
{
    public partial class Index
    {
        MonacoDiffEditorYaml _yamlDiffEditor { get; set; }

        YamlDocument yaml;

        string chartRepo;

        string chartVersion;

        string chartName;

        Dictionary<string, string> chartVersions = new Dictionary<string, string>();

        string selectedVersion;

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

                this.yaml = yaml.Documents[0];

                ms.Position = 0;
                TextReader outputStream = new StreamReader(ms);
                await _yamlDiffEditor.SetModifiedValue(outputStream.ReadToEnd());

                GetChartInfo();

                await GetChartVersions();

                await GetRemoteChart();
            }
        }

        void GetChartInfo()
        {
            var spec = ((YamlMappingNode)yaml.RootNode).Children[new YamlScalarNode("spec")] as YamlMappingNode;
            var chart = spec.Children[new YamlScalarNode("chart")] as YamlMappingNode;

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
                var version = (item[new YamlScalarNode("version")] as YamlScalarNode).Value.TrimStart('v');

                var url = ((item[new YamlScalarNode("urls")] as YamlSequenceNode).Children.First() as YamlScalarNode).Value;
                chartVersions.Add(version, url);
            }
        }

        async Task GetRemoteChart()
        {
            using var client = new HttpClient();

            string url = chartVersions.GetValueOrDefault(selectedVersion);

            if (Uri.TryCreate(url, UriKind.Relative, out Uri result))
            {
                url = new Uri(new Uri(chartRepo), url).ToString();
            }

            var stream = await client.GetStreamAsync(url);

            Stream gzipStream = new GZipInputStream(stream);

            using (var tarInputStream = new TarInputStream(gzipStream, Encoding.UTF8))
            {
                TarEntry entry;
                while ((entry = tarInputStream.GetNextEntry()) != null)
                {
                    if (entry.Name.EndsWith("values.yaml"))
                    {
                        using var fileContents = new MemoryStream();
                        tarInputStream.CopyEntryContents(fileContents);
                        fileContents.Position = 0;
                        var stringStream = new StreamReader(fileContents);

                        var values = new StringWriter();
                        string line = null;
                        while ((line = stringStream.ReadLine()) != null)
                        {
                            values.WriteLine("    " + line);
                        }

                        var spec = ((YamlMappingNode)yaml.RootNode).Children[new YamlScalarNode("spec")] as YamlMappingNode;
                        var chart = spec.Children[new YamlScalarNode("chart")] as YamlMappingNode;

                        spec.Children[new YamlScalarNode("values")] = new YamlScalarNode();
                        chart.Children[new YamlScalarNode("version")] = selectedVersion;

                        var ys = new YamlStream(yaml);

                        var header = new StringBuilder();
                        ys.Save(new StringWriter(header), false);

                        string cleanedHeader = header.ToString();

                        cleanedHeader = cleanedHeader[0..^10];

                        await _yamlDiffEditor.SetOriginalValue("---" + Environment.NewLine + cleanedHeader + Environment.NewLine + values.ToString());

                        break;
                    }
                }
            }
        }
    }
}