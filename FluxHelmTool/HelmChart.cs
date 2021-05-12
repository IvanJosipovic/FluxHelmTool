using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace FluxHelmTool
{
    public class HelmChart
    {
        //public string apiVersion { get; set; }
        //public string appVersion { get; set; }
        //public string description { get; set; }
        public string name { get; set; }
        public string version { get; set; }
        //public string kubeVersion { get; set; }
        //public string[] keywords { get; set; }
        //public string home { get; set; }
        //public string icon { get; set; }
        //public string[] sources { get; set; }
        //public Maintainer[] maintainers { get; set; }
        public HelmChart[] dependencies { get; set; }
    }
}
