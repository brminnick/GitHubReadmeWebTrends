using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks
{
    class CloudAdvocateYamlModel
    {
        [JsonProperty("uid")]
        public string Uid { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("remarks")]
        public string Remarks { get; set; }

        [JsonProperty("tagline")]
        public string Tagline { get; set; }

        [JsonProperty("image")]
        public Image Image { get; set; }

        [JsonProperty("connect")]
        public List<Connect> Connect { get; set; }

        [JsonProperty("location")]
        public Location Location { get; set; }
    }

    class Connect
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }
    }

    class Image
    {
        [JsonProperty("alt")]
        public string Alt { get; set; }

        [JsonProperty("src")]
        public string Src { get; set; }
    }

    class Location
    {
        [JsonProperty("display")]
        public string Display { get; set; }

        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("long")]
        public double Long { get; set; }
    }

    class Metadata
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
