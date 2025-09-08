using Microsoft.AspNetCore.Mvc.Testing;

namespace Tests.Shared;

public abstract class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    protected readonly HttpClient _client;

    public IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
