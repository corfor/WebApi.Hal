using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.Hal.Web.Api.Resources;

namespace WebApi.Hal.Web.Api
{
    public class RootController : ApiController
    {
        [HttpGet]
        public Representation Get()
        {
            return new RootRepresentation();
        }
    }
}
