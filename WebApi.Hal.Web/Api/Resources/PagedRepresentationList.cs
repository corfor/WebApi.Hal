using System.Collections.Generic;
using Newtonsoft.Json;

namespace WebApi.Hal.Web.Api.Resources
{
    public abstract class PagedRepresentationList<TRepresentation> : SimpleListRepresentation<TRepresentation> where TRepresentation : Representation
    {
        readonly Link uriTemplate;
        protected readonly object UriTemplateSubstitutionParams;

        protected PagedRepresentationList(IList<TRepresentation> res, int totalResults, int totalPages, int page, Link uriTemplate,
            object uriTemplateSubstitutionParams)
            : base(res)
        {
            this.uriTemplate = uriTemplate;
            TotalResults = totalResults;
            TotalPages = totalPages;
            Page = page;
            UriTemplateSubstitutionParams = uriTemplateSubstitutionParams;
        }

        [JsonProperty("totalResults")]
        public int TotalResults { get; set; }
        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }
        [JsonProperty("pages")]
        public int Page { get; set; }

        protected override void CreateHypermedia()
        {
            var prms = new List<object> {new {page = Page}};
            if (UriTemplateSubstitutionParams != null)
                prms.Add(UriTemplateSubstitutionParams);

            Href = Href ?? uriTemplate.CreateLink(prms.ToArray()).Href;

            AddLink(new Link {Href = Href, Rel = "self"});

            if (Page > 1)
            {
                var item = UriTemplateSubstitutionParams == null
                    ? uriTemplate.CreateLink("prev", new {page = Page - 1})
                    : uriTemplate.CreateLink("prev", UriTemplateSubstitutionParams, new {page = Page - 1}); // page overrides UriTemplateSubstitutionParams
                AddLink(item);
            }
            if (Page < TotalPages)
            {
                var link = UriTemplateSubstitutionParams == null // kbr
                    ? uriTemplate.CreateLink("next", new {page = Page + 1})
                    : uriTemplate.CreateLink("next", UriTemplateSubstitutionParams, new {page = Page + 1}); // page overrides UriTemplateSubstitutionParams
                AddLink(link);
            }
            AddLink(new Link("page", uriTemplate.Href));
        }
    }
}