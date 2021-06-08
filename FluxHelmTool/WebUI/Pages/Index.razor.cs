using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using BlazorMonaco;

namespace FluxHelmTool.WebUI.Pages
{
    public partial class Index
    {
        private MonacoDiffEditor YamlDiffEditor;

        private HelmTool HelmTool = new HelmTool();

        private HelmRelease SelectedHelmRelease;

        private string SelectedVersion;

        private List<string> ChartVersions = new List<string>();

        private bool ShowDependencies;

        private async Task ReadFiles(InputFileChangeEventArgs e)
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
            await YamlDiffEditor.ModifiedEditor.SetValue(helmRelease.YamlString);
            await UpdateOriginal();
        }

        private async Task SetSelectedVersion(string version)
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

            await YamlDiffEditor.OriginalEditor.SetValue(resultyaml);
        }

        private string GenerateHeader()
        {
            var lines = SelectedHelmRelease.YamlString.Split(Environment.NewLine);
            var header = new StringBuilder();

            foreach (var line in lines)
            {
                if (line.StartsWith("      version: "))
                {
                    header.AppendLine($"      version: {SelectedVersion}");
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
        private DiffEditorConstructionOptions DiffEditorConstructionOptions(MonacoDiffEditor editor)
        {
            return new DiffEditorConstructionOptions
            {
                OriginalEditable = false
            };
        }
        private async Task EditorOnDidInit(MonacoEditorBase editor)
        {
            // Get or create the original model
            TextModel original_model = await MonacoEditorBase.GetModel("sample-diff-editor-originalModel");
            if (original_model == null)
            {
                var original_value = "\"use strict\";\n" +
                                "function Person(age) {\n" +
                                "	if (age) {\n" +
                                "		this.age = age;\n" +
                                "	}\n" +
                                "}\n" +
                                "Person.prototype.getAge = function () {\n" +
                                "	return this.age;\n" +
                                "};\n";
                original_model = await MonacoEditorBase.CreateModel(original_value, "javascript", "sample-diff-editor-originalModel");
            }

            // Get or create the modified model
            TextModel modified_model = await MonacoEditorBase.GetModel("sample-diff-editor-modifiedModel");
            if (modified_model == null)
            {
                var modified_value = "\"don't use strict\";\n" +
                                "furction Person(age_is_just_a_number) {\n" +
                                "	if (age_is_just_a_number) {\n" +
                                "		this.age_is_just_a_number = age_is_just_a_number;\n" +
                                "	}\n" +
                                "}\n" +
                                "Person.prototype.getAge = function () {\n" +
                                "	return this.age_is_just_a_number;\n" +
                                "};\n" +
                                "//Really, it is just a number people!";
                modified_model = await MonacoEditorBase.CreateModel(modified_value, "javascript", "sample-diff-editor-modifiedModel");
            }
        }
    }
}