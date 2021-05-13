using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace FluxHelmTool
{
    public class HelmTool
    {
        public List<HelmRelease> HelmReleases { get; set; } = new List<HelmRelease>();

        public List<HelmRepository> HelmRepositories { get; set; } = new List<HelmRepository>();

        public void LoadYaml(Stream yamlStream)
        {
            foreach (var yamlString in Regex.Split(new StreamReader(yamlStream).ReadToEnd(), @"---\r\n"))
            {
                var yaml = new YamlStream();
                yaml.Load(new StringReader(yamlString));

                foreach (var item in yaml.Documents)
                {
                    switch (((item.RootNode as YamlMappingNode).Children[new YamlScalarNode("kind")] as YamlScalarNode).Value)
                    {
                        case "HelmRepository":
                            HelmRepositories.Add(new HelmRepository() { Yaml = item, YamlString = yamlString });
                            break;
                        case "HelmRelease":
                            HelmReleases.Add(new HelmRelease() { Yaml = item, YamlString = yamlString });
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public async Task<List<HelmChart>> GetDependencies(HelmRelease helmRelease, string version)
        {
            using var client = new HttpClient();

            string url = await GetChartUrl(helmRelease, version);

            var stream = await client.GetStreamAsync(url);

            Stream gzipStream = new GZipInputStream(stream);

            using (var tarInputStream = new TarInputStream(gzipStream, Encoding.UTF8))
            {
                TarEntry entry;
                while ((entry = tarInputStream.GetNextEntry()) != null)
                {
                    if (entry.Name.EndsWith("chart.yaml", StringComparison.InvariantCultureIgnoreCase))
                    {
                        using var fileContents = new MemoryStream();
                        tarInputStream.CopyEntryContents(fileContents);
                        fileContents.Position = 0;
                        var stringStream = new StreamReader(fileContents);

                        var deserializer = new DeserializerBuilder()
                            .IgnoreUnmatchedProperties()
                            .Build();

                        var p = deserializer.Deserialize<HelmChart>(stringStream);

                        if (p.dependencies != null)
                        {
                            return p.dependencies.ToList();
                        }
                    }
                }
            }

            return new List<HelmChart>();
        }

        private async Task<string> GetChartUrl(HelmRelease helmRelease, string version)
        {
            var repo = HelmRepositories.First(x => x.Name == helmRelease.RepositoryName).Url;

            using var stream = await new HttpClient().GetStreamAsync(repo.TrimEnd('/') + "/index.yaml");
            using var streamReader = new StreamReader(stream);
            var yaml = new YamlStream();
            yaml.Load(streamReader);

            var mapping = yaml.Documents[0].RootNode as YamlMappingNode;

            var items = (mapping.Children[new YamlScalarNode("entries")] as YamlMappingNode).Children[new YamlScalarNode(helmRelease.ChartName)] as YamlSequenceNode;

            var item = items.First(x => (x[new YamlScalarNode("version")] as YamlScalarNode).Value == version);

            var url = ((item[new YamlScalarNode("urls")] as YamlSequenceNode).Children.First() as YamlScalarNode).Value;

            if (Uri.TryCreate(url, UriKind.Relative, out Uri result))
            {
                url = new Uri(new Uri(repo), url).ToString();
            }

            return url;
        }

        public async Task<string> GetValues(HelmRelease helmRelease, string version)
        {
            using var client = new HttpClient();

            string url = await GetChartUrl(helmRelease, version);

            var stream = await client.GetStreamAsync(url);

            using Stream gzipStream = new GZipInputStream(stream);
            using (var tarInputStream = new TarInputStream(gzipStream, Encoding.UTF8))
            {
                TarEntry entry;
                while ((entry = tarInputStream.GetNextEntry()) != null)
                {
                    if (entry.Name.EndsWith("values.yaml", StringComparison.InvariantCultureIgnoreCase))
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

                        return values.ToString();
                    }
                }
            }

            throw new Exception("Values.yaml not found");
        }

        public async Task<List<string>> GetChartVersions(HelmRelease helmRelease)
        {
            using var stream = await new HttpClient().GetStreamAsync(HelmRepositories.First(x => x.Name == helmRelease.RepositoryName).Url.TrimEnd('/') + "/index.yaml");
            using var streamReader = new StreamReader(stream);
            
            var yaml = new YamlStream();
            yaml.Load(streamReader);

            var items = ((yaml.Documents[0].RootNode as YamlMappingNode)
                        .Children[new YamlScalarNode("entries")] as YamlMappingNode)
                        .Children[new YamlScalarNode(helmRelease.ChartName)] as YamlSequenceNode;

            var versions = new List<string>();

            foreach (YamlNode item in items)
            {
                var version = (item[new YamlScalarNode("version")] as YamlScalarNode).Value;

                versions.Add(version);
            }

            return versions;
        }
    }
}
