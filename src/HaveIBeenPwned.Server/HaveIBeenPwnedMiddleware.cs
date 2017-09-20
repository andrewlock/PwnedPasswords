using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace HaveIBeenPwned.Server
{
    public class HaveIBeenPwnedMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HaveIBeenPwnedMiddlewareSettings _options;
        private readonly IPwnedPasswordService _service;

        public HaveIBeenPwnedMiddleware(RequestDelegate next, IPwnedPasswordService service, IOptions<HaveIBeenPwnedMiddlewareSettings> options)
        {
            _next = next ?? throw new System.ArgumentNullException(nameof(next));
            _service = service ?? throw new System.ArgumentNullException(nameof(service));
            if (options == null) { throw new System.ArgumentNullException(nameof(options)); };
            _options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = $"/api/{_options.VersionString}/pwnedpassword";
            if (!context.Request.Path.StartsWithSegments(path))
            {
                await _next(context);
            }

            if (!context.Request.Headers.ContainsKey("User-Agent"))
            {
                context.Response.StatusCode = 403;
                return;
            }

            var password = context.Request.Form[_options.FormKey];

            if (string.IsNullOrEmpty(password))
            {
                context.Response.StatusCode = 400;
                return;
            }
            var passswordIsAHash = IsPasswordAHash(context, password);

            var hasBeenPwnedTask = passswordIsAHash
                ? _service.HasSha1PasswordBeenPwned(password)
                : _service.HasPasswordBeenPwned(password);

            if (await hasBeenPwnedTask)
            {
                context.Response.StatusCode = 200;
            }
            else
            {
                context.Response.StatusCode = 404;
            }
        }

        private static bool IsPasswordAHash(HttpContext context, string password)
        {
            if (!SHA1Util.IsSHA1String(password))
            {
                return false;
            }

            if (!context.Request.Query.ContainsKey("originalPasswordIsAHash"))
            {
                //no extra parameter and is sha1
                return true;
            }

            // this is a bit odd naming, originalPasswordIsAHash = password is NOT a hash
            var param = context.Request.Query["originalPasswordIsAHash"];
            return !string.Equals(param, "true", System.StringComparison.OrdinalIgnoreCase);
        }
    }

}
