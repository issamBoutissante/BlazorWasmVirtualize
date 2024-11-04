using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using VirtualizeGrid.Client;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient for REST requests (if needed)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Configure GrpcChannel with gRPC-Web support for Blazor WASM
builder.Services.AddSingleton(services =>
{
    // Create HttpClient with GrpcWebHandler to support gRPC-Web
    var grpcWebHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler());

    // Configure the GrpcChannel to use the gRPC server URL
    var grpcChannel = GrpcChannel.ForAddress("https://localhost:7238", new GrpcChannelOptions
    {
        HttpClient = new HttpClient(grpcWebHandler)
    });

    return grpcChannel;
});

await builder.Build().RunAsync();
