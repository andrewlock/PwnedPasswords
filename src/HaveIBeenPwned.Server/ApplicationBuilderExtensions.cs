using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace HaveIBeenPwned.Server
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseHaveIBeenPwnedServer(this IApplicationBuilder app)
        {
            return app.UseMiddleware<HaveIBeenPwnedMiddleware>();
        }

        public static IApplicationBuilder UseHaveIBeenPwnedServer(this IApplicationBuilder app, HaveIBeenPwnedMiddlewareSettings settings)
        {
            if (app == null) { throw new ArgumentNullException(nameof(app)); }
            if (settings == null) { throw new ArgumentNullException(nameof(settings)); }

            return app.UseMiddleware<HaveIBeenPwnedMiddleware>(Options.Create(settings));
        }
    }
}
