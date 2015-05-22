using System;
using System.Security.Policy;
using Newtonsoft.Json;

namespace WebApi.Hal
{
    /// <summary>
    ///     Based on hCard format
    ///     <see cref="http://microformats.org/wiki/h-card" />
    /// </summary>
    public class Contact
    {
        /// <summary>
        ///     The full/formatted name of the person or organization
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Name { get; set; }

        /// <summary>
        ///     Home page
        /// </summary>
        [JsonProperty("url", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Uri Url { get; set; }

        /// <summary>
        ///     Email address
        /// </summary>
        [JsonProperty("email", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Uri Email { get; set; }

        /// <summary>
        ///     Photo
        /// </summary>
        [JsonProperty("photo", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Uri Photo { get; set; }

        /// <summary>
        ///     Description of role
        /// </summary>
        [JsonProperty("role", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Role { get; set; }
    }
}