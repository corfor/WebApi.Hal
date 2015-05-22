using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using WebApi.Hal.Interfaces;
using WebApi.Hal.JsonConverters;

namespace WebApi.Hal
{
    public abstract class Representation : IResource
    {
        [JsonIgnore] readonly IDictionary<PropertyInfo, object> embeddedResourceProperties = new Dictionary<PropertyInfo, object>();
        [JsonIgnore] IList<Link> links;
        string reasonPhrase;
        HttpStatusCode statusCode;

        protected Representation()
        {
            links = new List<Link>();
        }

        [JsonProperty("_embedded")]
        IList<EmbeddedResource> Embedded { get; set; }

        /// <summary>
        ///     Gets or sets the status code of the HTTP response.
        /// </summary>
        /// <returns>
        ///     Returns <see cref="T:System.Net.HttpStatusCode" />.The status code of the HTTP response.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException" accessor="set">Must be between 0 and 999, inclusive.</exception>
        [JsonIgnore]
        public HttpStatusCode StatusCode
        {
            get { return statusCode; }
            set
            {
                if (value < 0 || value > (HttpStatusCode) 999)
                    throw new ArgumentOutOfRangeException("value");
                statusCode = value;
            }
        }

        /// <summary>
        ///     Gets or sets the reason phrase which typically is sent by servers together with the status code.
        /// </summary>
        /// <returns>
        ///     Returns <see cref="T:System.String" />.The reason phrase sent by the server.
        /// </returns>
        /// <exception cref="FormatException" accessor="set">Cannot contain NewLine character</exception>
        [JsonIgnore]
        public string ReasonPhrase
        {
            get
            {
                if (reasonPhrase != null)
                    return reasonPhrase;

                return HttpStatusDescription.Get(StatusCode);
            }
            set
            {
                if (value != null && ContainsNewLineCharacter(value))
                    throw new FormatException("ReasonPhrase cannot contain NewLine character.");
                reasonPhrase = value;
            }
        }

        [JsonIgnore]
        public virtual string Rel { get; set; }

        [JsonIgnore]
        public virtual string Href { get; set; }

        [JsonIgnore]
        public string LinkName { get; set; }

        public IList<Link> Links
        {
            get { return links.Count > 0 ? links : null; }
            set { links = value; }
        }

        public void AddLink(Link link)
        {
            links.Add(link);
        }

        static bool ContainsNewLineCharacter(string value)
        {
            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '\r':
                    case '\n':
                        return true;
                    default:
                        goto default;
                }
            }
            return false;
        }

        [OnSerializing]
        void OnSerialize(StreamingContext context)
        {
            // Clear the embeddedResourceProperties in order to make this object re-serializable.
            embeddedResourceProperties.Clear();

            if (!ResourceConverter.IsResourceConverterContext(context))
                return;

            var ctx = (HalJsonConverterContext) context.Context;

            if (!ctx.IsRoot)
                return;

            var curies = new List<CuriesLink>();

            RepopulateRecursively(ctx.HypermediaResolver, curies);

            links = curies
                .Distinct(CuriesLink.EqualityComparer)
                .Select(x => x.ToLink())
                .Union(links)
                .ToList();

            ctx.IsRoot = false;
        }

        [OnSerialized]
        void OnSerialized(StreamingContext context)
        {
            if (!ResourceConverter.IsResourceConverterContext(context)) return;
            // restore embedded resource properties
            foreach (var prop in embeddedResourceProperties.Keys)
                prop.SetValue(this, embeddedResourceProperties[prop], null);
        }

        Link ToLink(IHypermediaResolver resolver)
        {
            Link link = null;

            if (resolver != null)
            {
                link = resolver.ResolveSelf(this);

                if (link != null)
                {
                    link = link.Clone();
                    link.Rel = resolver.ResolveRel(this);
                }
            }

            if ((resolver != null) && (link != null)) return link;

            link = links.SingleOrDefault(x => x.Rel == "self");

            if (link == null) return null;

            link = link.Clone();
            link.Rel = Rel;

            return link;
        }

        void RepopulateRecursively(IHypermediaResolver resolver, List<CuriesLink> curies)
        {
            var type = GetType();

            if (resolver == null)
                RepopulateHyperMedia();
            else
                ResolveAndAppend(resolver, type);

            // put all embedded resources and lists of resources into Embedded for the _embedded serializer
            Embedded = new List<EmbeddedResource>();

            foreach (var property in type.GetProperties().Where(p => IsEmbeddedResourceType(p.PropertyType)))
            {
                var value = property.GetValue(this, null);

                if (value == null)
                    continue; // nothing to serialize for this property ...

                // remember embedded resource property for restoring after serialization
                embeddedResourceProperties.Add(property, value);

                var resource = value as IResource;

                if (resource != null)
                    ProcessPropertyValue(resolver, curies, resource);
                else
                    ProcessPropertyValue(resolver, curies, (IEnumerable<IResource>) value);

                // null out the embedded property so it doesn't serialize separately as a property
                property.SetValue(this, null, null);
            }

            curies.AddRange(links.Where(l => l != null && l.Curie != null).Select(l => l.Curie));

            if (Embedded.Count == 0)
                Embedded = null; // avoid the property from being serialized ...
        }

        void ProcessPropertyValue(IHypermediaResolver resolver, List<CuriesLink> curies, IEnumerable<IResource> resources)
        {
            var resourceList = resources.ToList();

            if (!resourceList.Any())
                return;

            var embeddedResource = new EmbeddedResource {IsSourceAnArray = true};

            foreach (var resourceItem in resourceList)
            {
                embeddedResource.Resources.Add(resourceItem);

                var representation = resourceItem as Representation;

                if (representation == null)
                    continue;

                representation.RepopulateRecursively(resolver, curies); // traverse ...
                links.Add(representation.ToLink(resolver)); // add a link to embedded to the container ...
            }

            Embedded.Add(embeddedResource);
        }

        void ProcessPropertyValue(IHypermediaResolver resolver, List<CuriesLink> curies, IResource resource)
        {
            var embeddedResource = new EmbeddedResource {IsSourceAnArray = false};
            embeddedResource.Resources.Add(resource);

            Embedded.Add(embeddedResource);

            var representation = resource as Representation;

            if (representation == null)
                return;

            representation.RepopulateRecursively(resolver, curies); // traverse ...
            links.Add(representation.ToLink(resolver)); // add a link to embedded to the container ...
        }

        void ResolveAndAppend(IHypermediaResolver resolver, Type type)
        {
            // We need reflection here, because appenders are of type IHypermediaAppender<T> whilst we define this logic in the base class of T

            var methodInfo = type.GetMethod("Append", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.NonPublic);
            var genericMethod = methodInfo.MakeGenericMethod(type);

            genericMethod.Invoke(this, new object[] {this, resolver});
        }

        protected static void Append<T>(IResource resource, IHypermediaResolver resolver) where T : class, IResource // called using reflection ...
        {
            var typed = resource as T;

            var appender = resolver.ResolveAppender(typed);
            var configured = resolver.ResolveLinks(typed).ToList();

            configured.Insert(0, resolver.ResolveSelf(typed));

            appender.Append(typed, configured);
        }

        internal static bool IsEmbeddedResourceType(Type type)
        {
            return typeof (IResource).IsAssignableFrom(type) ||
                   typeof (IEnumerable<IResource>).IsAssignableFrom(type);
        }

        public void RepopulateHyperMedia()
        {
            CreateHypermedia();

            if (!string.IsNullOrEmpty(Href) && links.Count(l => l.Rel == "self") == 0)
                links.Insert(0, new Link {Rel = "self", Href = Href});
        }

        protected virtual void CreateHypermedia()
        {
        }

        internal static class HttpStatusDescription
        {
            static readonly string[][] httpStatusDescriptions = new string[6][]
            {
                null,
                new string[3]
                {
                    "Continue",
                    "Switching Protocols",
                    "Processing"
                },
                new string[8]
                {
                    "OK",
                    "Created",
                    "Accepted",
                    "Non-Authoritative Information",
                    "No Content",
                    "Reset Content",
                    "Partial Content",
                    "Multi-Status"
                },
                new string[8]
                {
                    "Multiple Choices",
                    "Moved Permanently",
                    "Found",
                    "See Other",
                    "Not Modified",
                    "Use Proxy",
                    null,
                    "Temporary Redirect"
                },
                new string[27]
                {
                    "Bad Request",
                    "Unauthorized",
                    "Payment Required",
                    "Forbidden",
                    "Not Found",
                    "Method Not Allowed",
                    "Not Acceptable",
                    "Proxy Authentication Required",
                    "Request Timeout",
                    "Conflict",
                    "Gone",
                    "Length Required",
                    "Precondition Failed",
                    "Request Entity Too Large",
                    "Request-Uri Too Long",
                    "Unsupported Media Type",
                    "Requested Range Not Satisfiable",
                    "Expectation Failed",
                    null,
                    null,
                    null,
                    null,
                    "Unprocessable Entity",
                    "Locked",
                    "Failed Dependency",
                    null,
                    "Upgrade Required"
                },
                new string[8]
                {
                    "Internal Server Error",
                    "Not Implemented",
                    "Bad Gateway",
                    "Service Unavailable",
                    "Gateway Timeout",
                    "Http Version Not Supported",
                    null,
                    "Insufficient Storage"
                }
            };

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            internal static string Get(HttpStatusCode code)
            {
                return Get((int) code);
            }

            internal static string Get(int code)
            {
                if (code >= 100 && code < 600)
                {
                    var index1 = code/100;
                    var index2 = code%100;
                    if (index2 < httpStatusDescriptions[index1].Length)
                        return httpStatusDescriptions[index1][index2];
                }
                return null;
            }
        }
    }
}