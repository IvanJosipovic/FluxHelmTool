using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.RepresentationModel;

namespace FluxHelmTool
{
    public class HelmRepository
    {
        public string YamlString { get; set; }

        public YamlDocument Yaml { get; set; }

        public string Name => (((Yaml.RootNode as YamlMappingNode)
                        .Children[new YamlScalarNode("metadata")] as YamlMappingNode)
                        .Children[new YamlScalarNode("name")] as YamlScalarNode).Value;

        public string Namespace => (((Yaml.RootNode as YamlMappingNode)
                        .Children[new YamlScalarNode("metadata")] as YamlMappingNode)
                        .Children[new YamlScalarNode("namespace")] as YamlScalarNode).Value;

        public string Url => (((Yaml.RootNode as YamlMappingNode)
                        .Children[new YamlScalarNode("spec")] as YamlMappingNode)
                        .Children[new YamlScalarNode("url")] as YamlScalarNode).Value;
    }
}
