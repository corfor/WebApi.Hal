using System.Collections.Generic;
using WebApi.Hal.Tests.Representations;

namespace WebApi.Hal.Tests.HypermediaAppenders
{
    public class CategoryRepresentationHypermediaAppender : IHypermediaAppender<CategoryRepresentation>
    {
        public void Append(CategoryRepresentation resource, IEnumerable<Link> configured)
        {
            foreach (var link in configured)
            {
                switch (link.Rel)
                {
                    case Link.RelForSelf:
                        resource.AddLink(link.CreateLink(new {id = resource.Id}));
                        break;
                    default:
                        resource.AddLink(link); // append untouched ...
                        break;
                }
            }
        }
    }
}