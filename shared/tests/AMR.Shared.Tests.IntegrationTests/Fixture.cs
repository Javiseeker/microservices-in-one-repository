using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace   AMR.Shared.Tests.IntegrationTests;

public class Fixture<TProgram> : IClassFixture<WebApplicationFactory<TProgram>> where TProgram : class
{
    public readonly HttpClient _client;
    public readonly WebApplicationFactory<TProgram> _factory;

    public Fixture(WebApplicationFactory<TProgram> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    #region Initialize mocks and data for all integration tests

    #endregion
}