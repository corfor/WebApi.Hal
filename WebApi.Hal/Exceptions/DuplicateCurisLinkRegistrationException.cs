using System;

namespace WebApi.Hal.Exceptions
{
    [Serializable]
    public class DuplicateCurisLinkRegistrationException : Exception
    {
        public DuplicateCurisLinkRegistrationException(string name)
            : base("Configuration already containes a curies link with name: " + name)
        {
        }
    }
}