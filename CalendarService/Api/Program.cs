using Api.Exceptions;
using Api.Features.CalendarEvents;
using Azure.Identity;
using FluentValidation;
using ZiggyCreatures.Caching.Fusion;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(new Uri(builder.Configuration.GetRequiredSection("Endpoints:AppConfig").Value!), new DefaultAzureCredential())
    .ConfigureKeyVault(kv =>
    {
        kv.SetCredential(new DefaultAzureCredential());
    });
});

builder.Services.AddOpenApi();

builder.Services.AddHttpClient();

builder.Services.AddValidatorsFromAssemblyContaining<FetchMultiple.Validator>();

builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddFusionCache()
    .WithDefaultEntryOptions(opt =>
    {
        opt.SetDuration(TimeSpan.FromHours(2));
        opt.SetFailSafe(isEnabled: true, maxDuration: TimeSpan.FromDays(1), throttleDuration: TimeSpan.FromMinutes(1));
    });

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "PoliceService API V1");
    });
}

app.UseHttpsRedirection();


FetchMultiple.Endpoint.Map(app);

app.Run();

public partial class Program {}