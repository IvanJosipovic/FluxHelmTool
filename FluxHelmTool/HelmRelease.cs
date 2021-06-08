using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.RepresentationModel;

namespace FluxHelmTool
{
    public class HelmRelease
    {
        public string YamlString { get; set; }

        public YamlDocument Yaml { get; set; }

        public string Name => (((Yaml.RootNode as YamlMappingNode)
                                .Children[new YamlScalarNode("metadata")] as YamlMappingNode)
                                .Children[new YamlScalarNode("name")] as YamlScalarNode).Value;

        public string Namespace => (((Yaml.RootNode as YamlMappingNode)
                        .Children[new YamlScalarNode("metadata")] as YamlMappingNode)
                        .Children[new YamlScalarNode("namespace")] as YamlScalarNode).Value;

        public string ChartName => (((((Yaml.RootNode as YamlMappingNode)
                                .Children[new YamlScalarNode("spec")] as YamlMappingNode)
                                .Children[new YamlScalarNode("chart")] as YamlMappingNode)
                                .Children[new YamlScalarNode("spec")] as YamlMappingNode)
                                .Children[new YamlScalarNode("chart")] as YamlScalarNode).Value;

        public string ChartVersion => (((((Yaml.RootNode as YamlMappingNode)
                                .Children[new YamlScalarNode("spec")] as YamlMappingNode)
                                .Children[new YamlScalarNode("chart")] as YamlMappingNode)
                                .Children[new YamlScalarNode("spec")] as YamlMappingNode)
                                .Children[new YamlScalarNode("version")] as YamlScalarNode).Value;

        public string RepositoryName => ((((((Yaml.RootNode as YamlMappingNode)
                                .Children[new YamlScalarNode("spec")] as YamlMappingNode)
                                .Children[new YamlScalarNode("chart")] as YamlMappingNode)
                                .Children[new YamlScalarNode("spec")] as YamlMappingNode)
                                .Children[new YamlScalarNode("sourceRef")] as YamlMappingNode)
                                .Children[new YamlScalarNode("name")] as YamlScalarNode).Value;
    }
}
