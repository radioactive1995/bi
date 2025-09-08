using Api.Features.CalendarEvents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Tests.Shared;

namespace Tests.CalendarEvents;

public class FetchMultipleTests : IntegrationTestBase
{
    public FetchMultipleTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetCalendarEvents_ReturnsOkAndExpectedDataShape()
    {
        // Act
        var response = await _client.GetAsync("/calendar-events");
        var events = await response.Content.ReadFromJsonAsync<FetchMultiple.Response[]>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(events);

        var sample = events?.FirstOrDefault();
        if (sample != null)
        {
            Assert.False(string.IsNullOrWhiteSpace(sample.Id));
            Assert.False(string.IsNullOrWhiteSpace(sample.Title));
            Assert.False(string.IsNullOrWhiteSpace(sample.Location));
            Assert.False(string.IsNullOrWhiteSpace(sample.Language));
            Assert.False(string.IsNullOrWhiteSpace(sample.Url));
            Assert.False(string.IsNullOrWhiteSpace(sample.ImageUrl));
            Assert.False(string.IsNullOrWhiteSpace(sample.ImageText));
            Assert.False(string.IsNullOrWhiteSpace(sample.StartTime));
            Assert.False(string.IsNullOrWhiteSpace(sample.EndTime));
            Assert.True(sample.Start < sample.End);
        }
    }

    [Fact]
    public async Task GetCalendarEvents_WithInvalidLanguage_ReturnsValidationProblem()
    {
        // Act
        var response = await _client.GetAsync("/calendar-events?language=de");
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Contains("Language", problem.Errors.Keys);
    }

    [Fact]
    public async Task GetCalendarEvents_WithNegativeTake_ReturnsValidationProblem()
    {
        // Act
        var response = await _client.GetAsync("/calendar-events?take=-1");
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Contains("Take", problem.Errors.Keys);
    }

    [Fact]
    public async Task GetCalendarEvents_WithValidQueryParams_ReturnsFilteredEvents()
    {
        // Act
        var response = await _client.GetAsync("/calendar-events?take=2&language=en");
        var events = await response.Content.ReadFromJsonAsync<FetchMultiple.Response[]>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(events);
        Assert.True(events.Length <= 2);
        Assert.All(events, e => Assert.Equal("en", e.Language));
    }
}