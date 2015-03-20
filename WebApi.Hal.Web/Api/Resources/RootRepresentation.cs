using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace WebApi.Hal.Web.Api.Resources
{
    public class RootRepresentation : Representation
    {
        static readonly string AssemblyVersion;
        static readonly DateTimeOffset CreationTime;

        static RootRepresentation()
        {
            var assembly = Assembly.GetExecutingAssembly();
            AssemblyVersion = assembly.GetName().Version.ToString();
            CreationTime = File.GetCreationTime(assembly.Location);
        }

        [JsonProperty("version")]
        public string Version
        {
            get { return AssemblyVersion; }
        }

        [JsonProperty("built")]
        public DateTimeOffset Built
        {
            get { return CreationTime; }
        }

        protected override void CreateHypermedia()
        {
            Href = LinkTemplates.Root.Self.Href;

            AddLink(LinkTemplates.Beers.GetBeers);
            AddLink(LinkTemplates.Breweries.GetBreweries);
            AddLink(LinkTemplates.BeerStyles.GetStyles);
        }

    }
}