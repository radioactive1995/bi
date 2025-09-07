
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ZiggyCreatures.Caching.Fusion;

namespace Api.Features.CalendarEvents;

public static class FetchMultiple
{
    public record Request(
        [property: FromQuery(Name = "take") ]int? Take, 
        [property: FromQuery(Name = "language")]string? Language, 
        [property: FromQuery(Name = "campus")]string? Campus, 
        [property: FromQuery(Name = "audience")]string? Audience)
    {
        public string CacheKey => $"{Endpoint.ENDPOINT_ROUTE}-" +
            $"{Take}-{Language?.Trim()?.ToLower()}-{Campus?.Trim()?.ToLower()}-{Audience?.Trim()?.ToLower()}";
    }

    public record Response(
        string Id, string Language, string Title, string Location, string FilterList, DateTime Start, DateTime End, 
        string StartTime, string EndTime, string Url, string ImageUrl, string ImageText, bool BothLanguages);
    
    public class Endpoint
    {
        public const string ENDPOINT_ROUTE = "calendar-events";
        public static void Map(WebApplication app)
        {
            app.MapGet(ENDPOINT_ROUTE, async (IHttpClientFactory factory, IFusionCache cache, IValidator<Request> validator, [AsParameters]Request request) =>
            {
                var valideringResultat = validator.Validate(request);
                if (!valideringResultat.IsValid) return Results.ValidationProblem(ProcessErrorCodes(valideringResultat));

                var response = await cache.GetOrSetAsync(request.CacheKey, async _ =>
                {
                    var actualTake = request.Take ?? 5;
                    using var client = factory.CreateClient();

                    var response = await client.GetFromJsonAsync<Response[]>(
                            $"https://bi.no/apii/calendar-events?take={actualTake}&language={request?.Language}&campus={request?.Campus}&audience={request?.Audience}") ?? [];

                    return response;
                }, tags: [ENDPOINT_ROUTE, request.CacheKey]);

                return Results.Ok(response);
            })
            .WithName("GetCalendarEvents")
            .WithTags("CalendarEvents")
            .WithDescription("""
            Returns a list of calendar events.

            Query parameters:
            - take (optional, int): Maximum number of events to return. Must be non-negative.
            - language (optional, string): Language filter. Allowed values: "no", "en", "all".
            - campus (optional, string): Campus filter.
            - audience (optional, string): Audience filter.
            """);
        }
    }

    public class Validator : AbstractValidator<Request>
    {
        private static readonly HashSet<string> _allowedLanguages = ["no", "en", "all"];
        public Validator()
        {
            RuleFor(x => x.Take)
                .GreaterThan(-1)
                .When(x => x.Take.HasValue)
                .WithMessage("Take can't be negative.");

            RuleFor(x => x.Language)
                .Must(_allowedLanguages.Contains!)
                .When(x => !string.IsNullOrWhiteSpace(x.Language))
                .WithMessage("Language must be one of the following: " + string.Join(", ", _allowedLanguages));
        }
    }

    private static Dictionary<string, string[]> ProcessErrorCodes(ValidationResult valideringResultat)
    {
        return valideringResultat.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
    }
}
