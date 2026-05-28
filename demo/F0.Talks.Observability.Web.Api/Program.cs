using System.Text.Json.Serialization;
using F0.Talks.Observability.Web.Api.Routes;
using Sentry.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.WebHost.UseSentry(static (SentryAspNetCoreOptions options) =>
{
	options.Dsn = null;
	options.Debug = true;
	options.SampleRate = 1.0f;
	options.TracesSampleRate = 1.0d;
	options.EnableLogs = true;
	options.EnableMetrics = true;
});

builder.AddServiceDefaults();

builder.Services.ConfigureHttpJsonOptions(options =>
{
	options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddOpenApi();

builder.Services.AddSingleton<TaskListContextFactory>();

builder.Services.AddHostedService<MetricsService>();
builder.Services.AddHostedService<DatabaseService>();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.MapRoutes();

app.Run();

[JsonSerializable(typeof(Todo[]))]
internal sealed partial class AppJsonSerializerContext : JsonSerializerContext;
