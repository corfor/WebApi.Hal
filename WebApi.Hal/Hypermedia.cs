using System;
using System.Collections.Generic;
using System.Linq;
using WebApi.Hal.Exceptions;
using WebApi.Hal.Interfaces;

namespace WebApi.Hal
{
    public class Hypermedia : IHypermediaResolver, IHypermediaBuilder
    {
        readonly IDictionary<Type, object> appenders = new Dictionary<Type, object>();
        readonly IDictionary<Type, IList<Link>> hypermedia = new Dictionary<Type, IList<Link>>();
        readonly IDictionary<Type, Link> selfLinks = new Dictionary<Type, Link>();

        Hypermedia()
        {
        }

        public void RegisterAppender<T>(IHypermediaAppender<T> appender) where T : class, IResource
        {
            if (appender == null)
                throw new ArgumentNullException("appender");

            var type = typeof (T);

            if (appenders.ContainsKey(type))
                throw new DuplicateHypermediaResolverRegistrationException(type);

            appenders.Add(type, appender);
        }

        public void RegisterSelf<T>(Link link) where T : IResource
        {
            if (link == null)
                throw new ArgumentNullException("link");

            var type = typeof (T);

            if (selfLinks.ContainsKey(type))
                throw new DuplicateSelfLinkRegistrationException(type);

            selfLinks.Add(type, link);
        }

        public void RegisterSelf<T>(Link<T> link) where T : class, IResource
        {
            if (link == null)
                throw new ArgumentNullException("link");

            var type = typeof (T);

            if (selfLinks.ContainsKey(type))
                throw new DuplicateSelfLinkRegistrationException(type);

            selfLinks.Add(type, link);
        }

        public void RegisterLinks<T>(params Link[] links) where T : class, IResource
        {
            if (links == null)
                throw new ArgumentNullException("links");

            var type = typeof (T);

            if (hypermedia.ContainsKey(type))
                hypermedia[type] = hypermedia[type].Union(links).Distinct(Link.EqualityComparer).ToList();
            else
                hypermedia.Add(type, links.Distinct(Link.EqualityComparer).ToList());
        }

        public IHypermediaResolver Build()
        {
            return this;
        }

        public IHypermediaAppender<T> ResolveAppender<T>(T resource) where T : class, IResource
        {
            var type = resource.GetType();

            if (!appenders.ContainsKey(type))
                return null;

            return (IHypermediaAppender<T>) appenders[type];
        }

        public IEnumerable<Link> ResolveLinks(IResource resource)
        {
            var type = resource.GetType();

            return hypermedia.ContainsKey(type)
                ? hypermedia[type]
                : new Link[0];
        }

        public string ResolveRel(IResource resource)
        {
            var type = resource.GetType();

            return selfLinks.ContainsKey(type)
                ? selfLinks[type].Rel
                : type.Name.ToLowerInvariant();
        }

        public Link ResolveSelf(IResource resource)
        {
            var type = resource.GetType();

            if (!selfLinks.ContainsKey(type))
                return null;

            var clone = selfLinks[type].Clone();

            clone.Rel = Link.RelForSelf;

            return clone;
        }

        public static IHypermediaBuilder CreateBuilder()
        {
            return new Hypermedia();
        }
    }
}