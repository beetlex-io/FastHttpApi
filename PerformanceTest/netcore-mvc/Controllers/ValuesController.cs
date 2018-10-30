using Microsoft.AspNetCore.Mvc;
using System;

namespace netcore_mvc.Controllers
{
    // ValuesController is the equivalent
    // `ValuesController` of the Iris 8.3 mvc application.
    [Route("api/Values")]
    public class ValuesController : ControllerBase
    {
        // Get handles "GET" requests to "api/values/{id}".
        [HttpGet("{id}")]
        public string Get(string id)
        {
            return id;
        }

        // Put handles "PUT" requests to "api/values/{id}".
        [HttpPost("{id}")]
        public void Post()
        {
        }

        // Delete handles "DELETE" requests to "api/values/{id}".
        [HttpDelete("{id}")]
        public void Delete()
        {
        }
    }
}