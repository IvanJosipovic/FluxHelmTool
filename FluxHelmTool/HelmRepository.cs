﻿using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.RepresentationModel;

namespace FluxHelmTool
{
    public class HelmRepository
    {
        public YamlDocument Yaml { get; set; }

        public string Name => (((Yaml.RootNode as YamlMappingNode)
                        .Children[new YamlScalarNode("metadata")] as YamlMappingNode)
                        .Children[new YamlScalarNode("name")] as YamlScalarNode).Value;
        
        public string Url => (((Yaml.RootNode as YamlMappingNode)
                        .Children[new YamlScalarNode("spec")] as YamlMappingNode)
                        .Children[new YamlScalarNode("url")] as YamlScalarNode).Value;
    }
}