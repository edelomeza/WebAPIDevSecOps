using System.Net;

namespace WebAPIDevSecOps.Middleware
{
    public class RequestTimeoutMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TimeSpan _timeout;

        public RequestTimeoutMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            var seconds = configuration.GetValue<int>("RequestTimeoutSeconds", 60);
            _timeout = TimeSpan.FromSeconds(seconds);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using var cts = new CancellationTokenSource(_timeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, context.RequestAborted);

            try
            {
                await _next(context).WaitAsync(linkedCts.Token);
            }
            catch (OperationCanceledException) when (!context.Response.HasStarted)
            {
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    """{"statusCode":503,"error":"La solicitud excedió el tiempo máximo de espera.","type":"https://httpstatuses.io/503"}""");
            }
        }
    }
}
