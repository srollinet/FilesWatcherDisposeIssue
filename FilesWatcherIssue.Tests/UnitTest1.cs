using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace FilesWatcherIssue.Tests;

public class IssueReproFixture : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;

    public IssueReproFixture()
    {
        _factory = new WebApplicationFactory<Program>();
    }

    internal WebApplicationFactory<Program> WithBuilder(Action<IWebHostBuilder> configuration)
    {
        return _factory.WithWebHostBuilder(configuration);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        _factory?.Dispose();
        return Task.CompletedTask;
    }
}

[CollectionDefinition("IssueRepro")]
public class IssueReproFixtureCollection : ICollectionFixture<IssueReproFixture>
{
}

[Collection("IssueRepro")]
public class IssueReproFixtureTest : IAsyncLifetime
{
    private readonly IssueReproFixture _fixture;
    private WebApplicationFactory<Program>? _webAppFactory;

    public IssueReproFixtureTest(IssueReproFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        _webAppFactory = _fixture.WithBuilder(ConfigureBuilder);
        return Task.CompletedTask;
    }

    private void ConfigureBuilder(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(CustomizeServices);
    }

    protected virtual void CustomizeServices(IServiceCollection collection) { }

    public async Task DisposeAsync()
    {
        if (_webAppFactory is not null)
        {
            await _webAppFactory.DisposeAsync();
        }
    }

    [Theory]
    [MemberData(nameof(GetTestData))]
    public void Test(int value)
    {
        _webAppFactory!.Services.GetService<IHostLifetime>();
    }

    public static IEnumerable<object[]> GetTestData => Enumerable.Range(0, 128).Select(i => new object[] {i});
}