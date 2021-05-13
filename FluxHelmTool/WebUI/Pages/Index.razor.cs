using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agno.BlazorInputFile;
using System.IO;
using BlazorMonacoYaml;
using YamlDotNet.RepresentationModel;
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

        private bool ShowDependencies = false;

        private async Task HandleSelection(IFileListEntry[] files)
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
            await YamlDiffEditor.SetModifiedValue(helmRelease.YamlString);
            await UpdateOriginal();
        }

        public async Task SetSelectedVersion(string version)
        {
            SelectedVersion = version;

            await UpdateOriginal();
        }

        private async Task UpdateOriginal()
        {
            string resultyaml = GenerateHeader();

            resultyaml += Environment.NewLine + await HelmTool.GetValues(SelectedHelmRelease, SelectedVersion);

            if (ShowDependencies)
            {
                var dependencies = await HelmTool.GetDependencies(SelectedHelmRelease, SelectedVersion);

                //foreach (var item in dependencies)
                //{
                //    resultyaml += Environment.NewLine + await HelmTool.GetValues(SelectedHelmRelease, item.version);
                //}
            }

            await YamlDiffEditor.SetOriginalValue(resultyaml);
        }

        private string GenerateHeader()
        {
            var lines = SelectedHelmRelease.YamlString.Split(Environment.NewLine);
            var header = new StringBuilder();

            foreach (var line in lines)
            {
                if (line.StartsWith("      version: "))
                {
                    header.AppendLine("      version: " + SelectedVersion);
                }
                else
                {
                    header.AppendLine(line);
                }

                if (line == "  values:")
                {
                    break;
                }
            }

            return header.ToString();
        }
    }
}