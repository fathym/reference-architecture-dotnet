using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Fathym.RefArch.Web.API.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
		#region Fields
		protected readonly ILogger<ValuesController> logger;
		#endregion

		#region Constructors
		public ValuesController(ILoggerFactory loggerFactory)
		{
			logger = loggerFactory.CreateLogger<ValuesController>();
		}
		#endregion

		// GET api/values
		[HttpGet]
        public IEnumerable<string> Get()
        {
            var vals = new string[] { "value1", "value2" };

			logger.LogDebug(vals.ToString());

			return vals;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
