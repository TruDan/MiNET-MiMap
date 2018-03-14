using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MiMap.Web.Middleware
{
    public class NoCacheMiddleware
	{
		private readonly RequestDelegate _next;
		public NoCacheMiddleware(RequestDelegate next)
		{
			_next = next;
		}

        public Task Invoke(HttpContext context)
        {
            context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            context.Response.Headers["Pragma"] = "no-cache";
            context.Response.Headers["Expires"] = "0";
            return _next.Invoke(context);
        }
    }
}
