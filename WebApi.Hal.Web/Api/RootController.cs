using System;
using System.Web.Http;

namespace WebApi.Hal.Web.Api
{
    public class RootController : ApiController
    {
        [HttpGet]
        public Representation Get()
        {
            var root = new RootRepresentation("Sample HAL API")
            {
                Description = "Playground for exploring HAL",
                Contact =
                    new Contact
                    {
                        Name = "Corey Ford",
                        Photo = new Uri("http://www.gravatar.com/avatar/a74132a50d1e81af0621ad7525383aff.png"),
                        Email = new Uri("Corey.Ford@ltcg.com", UriKind.Relative),
                        Role = "Architect"
                    },
                HypermediaCreator = rootRepresentation =>
                {
                    rootRepresentation.Href = LinkTemplates.Root.Self.Href;

                    rootRepresentation.AddLink(LinkTemplates.Beers.GetBeers);
                    rootRepresentation.AddLink(LinkTemplates.Breweries.GetBreweries);
                    rootRepresentation.AddLink(LinkTemplates.BeerStyles.GetStyles);
                }
            };
            return root;
        }
    }
}