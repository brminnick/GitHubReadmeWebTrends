using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks
{
    class CloudAdvocateYamlModel
    {
        [JsonProperty("uid")]
        public string Uid { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("metadata")]
        public Metadata? Metadata { get; set; }

        [JsonProperty("remarks")]
        public string Remarks { get; set; } = string.Empty;

        [JsonProperty("tagline")]
        public string Tagline { get; set; } = string.Empty;

        [JsonProperty("image")]
        public Image? Image { get; set; }

        [JsonProperty("connect")]
        public List<Connect> Connect { get; set; } = Enumerable.Empty<Connect>().ToList();

        [JsonProperty("location")]
        public Location? Location { get; set; }
    }

    class Connect
    {
        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("url")]
        public Uri? Url { get; set; }
    }

    class Image
    {
        [JsonProperty("alt")]
        public string Alt { get; set; } = string.Empty;

        [JsonProperty("src")]
        public string Src { get; set; } = string.Empty;
    }

    class Location
    {
        [JsonProperty("display")]
        public string Display { get; set; } = string.Empty;

        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("long")]
        public double Long { get; set; }
    }

    class Metadata
    {
        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;
    }
}
