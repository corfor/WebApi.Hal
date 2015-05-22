using System;
using System.Collections;
using System.Collections.Generic;
using WebApi.Hal.Interfaces;

namespace WebApi.Hal
{
    public abstract class SimpleListRepresentation<TResource> : Representation, IEnumerable<TResource> where TResource : IResource
    {
        IList<TResource> resourceList;

        protected SimpleListRepresentation()
        {
            ResourceList = new List<TResource>();
        }

        /// <exception cref="ArgumentNullException">The value of 'list' cannot be null. </exception>
        protected SimpleListRepresentation(IList<TResource> list)
        {
            if (list == null) throw new ArgumentNullException("list");
            ResourceList = list;
        }

        /// <exception cref="ArgumentNullException" accessor="set">The value of 'value' cannot be null. </exception>
        public IList<TResource> ResourceList
        {
            get { return resourceList; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");

                resourceList = value;
            }
        }

        public IEnumerator<TResource> GetEnumerator()
        {
            return ResourceList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}