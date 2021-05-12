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
        private MonacoDiffEditorYaml YamlDiffEditor;

        private HelmTool HelmTool = new HelmTool();

        private HelmRelease SelectedHelmRelease;

        private string SelectedVersion;

        private List<string> ChartVersions = new List<string>();

        bool ShowDependencies = true;

        async Task HandleSelection(IFileListEntry[] files)
        {
            foreach (var file in files)
            {
                var ms = new MemoryStream();
                await file.Data.CopyToAsync(ms);
                ms.Position = 0;
                HelmTool.LoadYaml(ms);
            }
        }

        private async Task SetSelectedRelease(string helmReleaseName)
        {
            var helmRelease = HelmTool.HelmReleases.First(x => x.Name == helmReleaseName);
            SelectedHelmRelease = helmRelease;
            ChartVersions = await HelmTool.GetChartVersions(helmRelease);

            SelectedVersion = helmRelease.ChartVersion;

            Serializer serializer = new Serializer();
            await YamlDiffEditor.SetModifiedValue(serializer.Serialize(helmRelease.Yaml.RootNode));

        }

        public void SetSelectedVersion(string version)
        {
            SelectedVersion = version;
        }

        //async Task UpdateOriginal()
        //{
        //    string resultyaml = GenerateHeader();

        //    resultyaml += Environment.NewLine + await GetValues(chartRepo, chartName, selectedVersion);

        //    if (showDependencies)
        //    {
        //        var dependencies = await GetDependencies(chartRepo, chartName, selectedVersion);

        //        foreach (var item in dependencies)
        //        {
        //            resultyaml += Environment.NewLine + await GetValues(item.repository, item.name, item.version);
        //        }
        //    }

        //    await _yamlDiffEditor.SetOriginalValue(resultyaml);
        //}

        //void GetChartInfo()
        //{
        //    var spec = ((YamlMappingNode)yaml.RootNode).Children[new YamlScalarNode("spec")] as YamlMappingNode;
        //    var chart = spec.Children[new YamlScalarNode("chart")] as YamlMappingNode;

        //    var repo = chart.Children[new YamlScalarNode("repository")] as YamlScalarNode;
        //    chartRepo = repo.Value;

        //    var name = chart.Children[new YamlScalarNode("name")] as YamlScalarNode;
        //    chartName = name.Value;

        //    var version = chart.Children[new YamlScalarNode("version")] as YamlScalarNode;
        //    chartVersion = version.Value;

        //    selectedVersion = chartVersion;
        //}

        //string GenerateHeader()
        //{
        //    //var spec = ((YamlMappingNode)yaml.RootNode).Children[new YamlScalarNode("spec")] as YamlMappingNode;
        //    //var chart = spec.Children[new YamlScalarNode("chart")] as YamlMappingNode;

        //    //spec.Children[new YamlScalarNode("values")] = new YamlScalarNode();
        //    //chart.Children[new YamlScalarNode("version")] = selectedVersion;

        //    var ys = new YamlStream(yaml);

        //    var header = new StringBuilder();
        //    ys.Save(new StringWriter(header), false);

        //    string cleanedHeader = header.ToString();

        //    cleanedHeader = cleanedHeader[0..^10];

        //    return "---" + Environment.NewLine + cleanedHeader;
        //}

    }
}