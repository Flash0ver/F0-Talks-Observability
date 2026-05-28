using Aspire.Hosting.DevTunnels;
using Aspire.Hosting.Maui;

var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ProjectResource> webApi = builder.AddProject<Projects.F0_Talks_Observability_Web_Api>("webapi")
	.WithHttpHealthCheck("/health");

IResourceBuilder<MauiProjectResource> mauiApp = builder.AddMauiProject("mauiapp", "../F0.Talks.Observability.Maui.App/F0.Talks.Observability.Maui.App.csproj");

IResourceBuilder<DevTunnelResource> publicDevTunnel = builder.AddDevTunnel("devtunnel-public")
	.WithAnonymousAccess()
	.WithReference(webApi.GetEndpoint("http"));

if (OperatingSystem.IsMacOS())
{
	mauiApp.AddMacCatalystDevice("mauiapp-maccatalyst")
		.WithReference(webApi);

	mauiApp.AddiOSSimulator("mauiapp-ios-simulator")
		.WithOtlpDevTunnel()
		.WithReference(webApi, publicDevTunnel);
}

if (OperatingSystem.IsWindows())
{
	mauiApp.AddWindowsDevice("mauiapp-windows")
		.WithReference(webApi);
}

mauiApp.AddAndroidEmulator("mauiapp-android-emulator")
	.WithOtlpDevTunnel()
	.WithReference(webApi, publicDevTunnel);

builder.Build().Run();
