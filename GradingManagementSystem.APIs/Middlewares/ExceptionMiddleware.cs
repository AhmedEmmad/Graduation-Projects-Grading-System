using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;

namespace GradingManagementSystem.APIs.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, IHostEnvironment environment, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _environment = environment;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                //var response = _environment.IsDevelopment()
                //    ? new ApiResponse((int)HttpStatusCode.InternalServerError, ex.Message)
                //    : new ApiResponse((int)HttpStatusCode.InternalServerError, "An unexpected error occurred.");
                var response = new
                {
                    statusCode = (int)HttpStatusCode.InternalServerError,
                    message = ex.Message,  // Always show the error message
                    stackTrace = ex.StackTrace
                };

                var json = JsonConvert.SerializeObject(response, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });

                await context.Response.WriteAsync(json);
            }
        }
    }

}
