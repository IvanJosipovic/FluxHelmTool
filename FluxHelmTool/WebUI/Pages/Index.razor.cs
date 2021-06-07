using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using BlazorMonacoYaml;
using System.Text;
using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FluxHelmTool.WebUI.Pages
{
    public partial class Index
    {
        private MonacoDiffEditorYaml YamlDiffEditor;

        private HelmTool HelmTool = new HelmTool();

        private HelmRelease SelectedHelmRelease;

        private string SelectedVersion;

        private List<string> ChartVersions = new List<string>();

        private bool ShowDependencies;

        public async Task ReadFiles(InputFileChangeEventArgs e)
        {
            foreach (var file in e.GetMultipleFiles(100))
            {
                using (Stream stream = file.OpenReadStream())
                {
                    await HelmTool.LoadYaml(stream);
                }
            }
         }

        private async Task SetSelectedRelease(string helmReleaseName)
        {
            var helmRelease = HelmTool.HelmReleases.First(x => x.Name == helmReleaseName);
            SelectedHelmRelease = helmRelease;
            ChartVersions = await HelmTool.GetChartVersions(helmRelease);
            SelectedVersion = helmRelease.ChartVersion;
            StateHasChanged();
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
            if (SelectedVersion == null)
            {
                return;
            }

            string resultyaml = GenerateHeader();

            resultyaml += Environment.NewLine + await HelmTool.GetValues(SelectedHelmRelease.RepositoryName, SelectedHelmRelease.ChartName, SelectedVersion);

            if (ShowDependencies)
            {
                var dependencies = await HelmTool.GetDependencies(SelectedHelmRelease.RepositoryName, SelectedHelmRelease.ChartName, SelectedVersion);

                foreach (var item in dependencies)
                {
                    resultyaml += Environment.NewLine + await HelmTool.GetValues(SelectedHelmRelease.RepositoryName, item.name, item.version);
                }
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