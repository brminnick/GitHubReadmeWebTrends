using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace GitHubReadmeWebTrends.Common
{
    public class CloudAdvocateYamlModel
    {
        [YamlMember(Alias = "uid")]
        public string Uid { get; set; } = string.Empty;

        [YamlMember(Alias = "name")]
        public string Name { get; set; } = string.Empty;

        [YamlMember(Alias = "metadata")]
        public Metadata Metadata { get; set; } = new Metadata();

        [YamlMember(Alias = "remarks")]
        public string Remarks { get; set; } = string.Empty;

        [YamlMember(Alias = "tagline")]
        public string Tagline { get; set; } = string.Empty;

        [YamlMember(Alias = "image")]
        public Image Image { get; set; } = new Image();

        [YamlMember(Alias = "connect")]
        public List<Connect> Connect { get; set; } = new List<Connect>();

        [YamlMember(Alias = "location")]
        public Location Location { get; set; } = new Location();
    }

    public class Connect
    {
        [YamlMember(Alias = "title")]
        public string Title { get; set; } = string.Empty;

        [YamlMember(Alias = "url")]
        public Uri? Url { get; set; }
    }

    public class Image
    {
        [YamlMember(Alias = "alt")]
        public string Alt { get; set; } = string.Empty;

        [YamlMember(Alias = "src")]
        public string Src { get; set; } = string.Empty;
    }

    public class Location
    {
        [YamlMember(Alias = "display")]
        public string Display { get; set; } = string.Empty;

        [YamlMember(Alias = "lat")]
        public double Lat { get; set; }

        [YamlMember(Alias = "long")]
        public double Long { get; set; }
    }

    public class Metadata
    {
        [YamlMember(Alias = "title")]
        public string Title { get; set; } = string.Empty;

        [YamlMember(Alias = "description")]
        public string Description { get; set; } = string.Empty;

        [YamlMember(Alias = "ms.author")]
        public string Alias { get; set; } = string.Empty;
    }
}
