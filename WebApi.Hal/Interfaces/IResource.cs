using System.Collections.Generic;
using Newtonsoft.Json;

namespace WebApi.Hal.Interfaces
{
    public interface IResource
    {
        [JsonIgnore]
        string Rel { get; set; }

        [JsonIgnore]
        string Href { get; set; }

        [JsonIgnore]
        string LinkName { get; set; }

        [JsonProperty("_links", DefaultValueHandling = DefaultValueHandling.Ignore)]
        IList<Link> Links { get; set; }

        void AddLink(Link link);
    }
}