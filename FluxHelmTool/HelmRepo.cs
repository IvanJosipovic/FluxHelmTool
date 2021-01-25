using System;
using System.Collections.Generic;
using System.Text;

namespace FluxHelmTool
{
    public class HelmRepo
    {
        public string apiVersion { get; set; }
        public Dictionary<string, Entry> entries { get; set; }
        public string generated { get; set; }
    }

    public class Entry
    {
        public Annotations annotations { get; set; }
        public string apiVersion { get; set; }
        public string appVersion { get; set; }
        public object created { get; set; }
        public Dependency[] dependencies { get; set; }
        public string description { get; set; }
        public string digest { get; set; }
        public string home { get; set; }
        public string icon { get; set; }
        public string[] keywords { get; set; }
        public Maintainer[] maintainers { get; set; }
        public string name { get; set; }
        public string[] sources { get; set; }
        public string[] urls { get; set; }
        public string version { get; set; }
    }

    public class Annotations
    {
        public string category { get; set; }
    }

    public class Dependency
    {
        public string name { get; set; }
        public string repository { get; set; }
        public string[] tags { get; set; }
        public string version { get; set; }
        public string condition { get; set; }
    }

    public class Maintainer
    {
        public string email { get; set; }
        public string name { get; set; }
    }
}
