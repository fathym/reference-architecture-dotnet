using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Fathym.Presentation.MVC.Rewrite
{
    public class UsernameRule : IRule
    {
        #region Properties
        public virtual string UsernameQueryParameter { get; set; }
        #endregion

        #region API Methods
        public virtual void ApplyRule(RewriteContext context)
        {
            var request = context.HttpContext.Request;

            var query = QueryHelpers.ParseQuery(request.QueryString.Value).ToDictionary(v => v.Key, v => v.Value.ToString());

            if (!shouldRemove(context))
                query[UsernameQueryParameter] = queryValueLoader(context);
            else
                query.Remove(UsernameQueryParameter); ;

            var currentUri = request.GetFullUrl();

            if (!request.Query.IsNullOrEmpty())
                currentUri = currentUri.Replace(WebUtility.UrlDecode(request.QueryString.Value), string.Empty);

            var newUri = new Uri(QueryHelpers.AddQueryString(currentUri, query));

            var response = context.HttpContext.Response;

            response.StatusCode = (int)HttpStatusCode.Redirect;

            response.Headers["Location"] = newUri.ToString();

            context.Result = RuleResult.EndResponse;
        }
        #endregion

        #region Helpers
        protected virtual string queryValueLoader(RewriteContext context)
        {
            return context.HttpContext.User.Identity.Name;
        }

        protected virtual bool shouldRemove(RewriteContext context)
        {
            return !context.HttpContext.User.Identity.IsAuthenticated;
        }
        #endregion
    }
}
