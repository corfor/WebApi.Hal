﻿using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace WebApi.Hal.Web.Api.Resources
{
    public class BeerRepresentation : Representation
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int? BreweryId { get; set; }
        public string BreweryName { get; set; }

        public int? StyleId { get; set; }
        public string StyleName { get; set; }

        [JsonIgnore]
        public List<int> ReviewIds { get; set; }

        public override string Rel
        {
            get { return LinkTemplates.Beers.Beer.Rel; }
            set { }
        }

        public override string Href
        {
            get { return LinkTemplates.Beers.Beer.CreateLink(new { id = Id }).Href; }
            set { }
        }

        protected override void CreateHypermedia()
        {
            if (StyleId != null)
                AddLink(LinkTemplates.BeerStyles.Style.CreateLink(new { id = StyleId }));
            if (BreweryId != null)
                AddLink(LinkTemplates.Breweries.Brewery.CreateLink(new { id = BreweryId }));

            if (ReviewIds == null || !ReviewIds.Any() ) return;

            foreach (var rid in ReviewIds)
                AddLink(LinkTemplates.Reviews.GetBeerReview.CreateLink(new { id = Id, rid }));
        }
    }
}
