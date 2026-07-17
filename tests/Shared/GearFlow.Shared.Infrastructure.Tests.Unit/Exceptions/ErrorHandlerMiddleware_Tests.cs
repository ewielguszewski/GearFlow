using GearFlow.Shared.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Text.Json;

namespace GearFlow.Shared.Infrastructure.Tests.Unit.Exceptions;

public class ErrorHandlerMiddleware_Tests
{
    [Fact]
    public async Task error_response_should_expose_current_w3c_trace_id()
    {
        using var activity = new Activity("test-request");
        activity.SetIdFormat(ActivityIdFormat.W3C);
        activity.Start();

        var context = new DefaultHttpContext();
        context.TraceIdentifier = "request-id";
        context.Response.Body = new MemoryStream();
        var logger = new TestLogger<ErrorHandlerMiddleware>();
        var middleware = new ErrorHandlerMiddleware(logger);

        await middleware.InvokeAsync(
            context,
            _ => throw new InvalidOperationException("Failure"));

        context.Response.Body.Position = 0;
        using var response = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(
            activity.TraceId.ToString(),
            response.RootElement.GetProperty("traceId").GetString());
    }
}
