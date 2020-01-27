//  --------------------------------------------------------------------------------------------------------------------
//  <summary>
//    Defines the RequestLoggerMiddleware type.
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

namespace IdentityProvider.Common.Middlewares
{
    using System;
    using System.Diagnostics;
    using System.Linq; 
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;

    using Serilog;
    using Serilog.Events;
    using Helpers;

    /// <summary>
    /// The Request Logger Middleware.
    /// </summary>
    public class RequestLoggerMiddleware
    {
        /// <summary>
        /// The message template
        /// </summary>
        public const string MessageTemplate = @"HTTP {RequestMethod} {RequestPath} responded 
{StatusCode} in {Elapsed:0.0000} ms. 
RequestHeaders = {RequestHeaders}; 
RequestHost = {RequestHost}; 
RequestProtocol = {RequestProtocol}; 
RequestForm = {RequestForm}";

        /// <summary>
        /// The log
        /// </summary>
        private static readonly ILogger Log = Serilog.Log.ForContext<RequestLoggerMiddleware>();

        /// <summary>
        /// The next middleware
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestLoggerMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware.</param>
        public RequestLoggerMiddleware(RequestDelegate next)
        {
            this._next = next ?? throw new ArgumentNullException(nameof(next));
        }

        /// <summary>
        /// Invokes the Request Logger Middleware.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentNullException">httpContext</exception>
        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            var start = Stopwatch.GetTimestamp();
            try
            {
                await this._next(httpContext);
                var elapsedMs = TimeHelper.GetElapsedMilliseconds(start, Stopwatch.GetTimestamp());

                var statusCode = httpContext.Response?.StatusCode;
                var level = statusCode > 499 ? LogEventLevel.Error : LogEventLevel.Information;
                LogForContext(httpContext, elapsedMs).Write(level, MessageTemplate);
            }
            catch (Exception ex)
            {
                var elapsedMs = TimeHelper.GetElapsedMilliseconds(start, Stopwatch.GetTimestamp());
                LogForContext(httpContext, elapsedMs).Error(ex, MessageTemplate);
            }
        }


        /// <summary>
        /// Logs for HTTP context.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="elapsedMs">Elapsed time (in ms).</param>
        /// <returns>The Logger.</returns>
        private static ILogger LogForContext(HttpContext httpContext, double elapsedMs)
        {
            var request = httpContext.Request;

            var result = Log.ForContext("RequestMethod", request.Method)
                .ForContext("RequestPath", request.Path)
                .ForContext("StatusCode", httpContext.Response?.StatusCode)
                .ForContext("Elapsed", elapsedMs)
                .ForContext("RequestHeaders", request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), destructureObjects: true)
                .ForContext("RequestHost", request.Host)
                .ForContext("RequestProtocol", request.Protocol);

            if (request.HasFormContentType)
            {
                result = result.ForContext(
                    "RequestForm",
                    request.Form.ToDictionary(v => v.Key, v => v.Value.ToString()));
            }

            return result;
        }
    }
}
