using System;
using System.IO;
using System.Reflection;
using System.Web;
using Newtonsoft.Json;

namespace WebApi.Hal
{
    /// <summary>
    ///     Acts like a sitemap.xml for this API
    ///     <see cref="http://t.co/nLhtxaJfXl" />
    /// </summary>
    public class RootRepresentation : Representation
    {
        static readonly Version InternalAssemblyVersion;
        static readonly DateTimeOffset CreationTime;
        DateTimeOffset built;
        string version;

        static RootRepresentation()
        {
            var assembly = Assembly.GetEntryAssembly() ?? GetWebEntryAssembly();
            if (assembly != null)
            {
                InternalAssemblyVersion = assembly.GetName().Version;
                CreationTime = File.GetCreationTime(assembly.Location);
            }
        }

        public RootRepresentation(string name)
        {
            Name = name;
        }

        public static Version AssemblyVersion
        {
            get { return InternalAssemblyVersion; }
        }

        /// <summary>
        ///     The name of the API
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Name { get; private set; }

        /// <summary>
        ///     A description of the value this API delivers.
        /// </summary>
        [JsonProperty("description", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Description { get; set; }

        /// <summary>
        ///     An image, logo, or icon that describes this API.
        /// </summary>
        [JsonProperty("image", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Uri Image { get; set; }

        /// <summary>
        ///     A handful of key words and phrases that describe the API itself.
        /// </summary>
        [JsonProperty("tags", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[] Tags { get; set; }

        /// <summary>
        ///     Contact info for further questions, requests, bugs, etc.
        /// </summary>
        [JsonProperty("contact", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Contact Contact { get; set; }

        /// <summary>
        ///     The url any human should visit to learn more about this API.
        /// </summary>
        [JsonProperty("humanURL", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Uri HumanUrl { get; set; }

        /// <summary>
        ///     The base url any machine should follow to start using this API.
        /// </summary>
        [JsonProperty("baseURL", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Uri BaseUrl { get; set; }

        [JsonProperty("specificationVersion", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string SpecificationVersion { get; set; }

        /// <summary>
        ///     API version. Defaults to AssemblyVersion.
        /// </summary>
        [JsonProperty("version", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Version
        {
            get { return version ?? InternalAssemblyVersion.ToString(); }
            set { version = value; }
        }

        /// <summary>
        ///     When this API was last built.
        /// </summary>
        [JsonProperty("built", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTimeOffset Built
        {
            get { return built != default(DateTimeOffset) ? built : CreationTime; }
            set { built = value; }
        }

        /// <summary>
        ///     Used for passing a custom message back to user.
        ///     <example>It is recommended to pass http header Accept: application/hal+json</example>
        /// </summary>
        [JsonProperty("message", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonIgnore]
        public Action<RootRepresentation> HypermediaCreator { get; set; }

        static Assembly GetWebEntryAssembly()
        {
            if (HttpContext.Current == null ||
                HttpContext.Current.ApplicationInstance == null)
            {
                return null;
            }

            var type = HttpContext.Current.ApplicationInstance.GetType();
            while (type != null && type.Namespace == "ASP")
            {
                type = type.BaseType;
            }

            return type == null ? null : type.Assembly;
        }

        protected override void CreateHypermedia()
        {
            base.CreateHypermedia();
            if (HypermediaCreator != null)
                HypermediaCreator(this);
        }
    }
}