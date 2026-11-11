using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace Monetra.UnitTests.TestSupport;

internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
    private readonly ConcurrentQueue<Uri> _requests = new();

    public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responder = responder;
    }

    public IReadOnlyCollection<Uri> Requests => _requests;

    public static StubHttpMessageHandler Json(string json) => new(_ => JsonResponse(json));

    public static StubHttpMessageHandler Status(HttpStatusCode statusCode) => new(_ => new HttpResponseMessage(statusCode));

    public static StubHttpMessageHandler Unreachable() => new(_ => throw new HttpRequestException("Sin conexión con el proveedor."));

    public static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _requests.Enqueue(request.RequestUri!);
        return Task.FromResult(_responder(request));
    }
}
