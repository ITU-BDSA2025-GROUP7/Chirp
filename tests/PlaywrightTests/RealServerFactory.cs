using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;


    public class RealServerFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
        where TEntryPoint : class
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            // Force Kestrel to be used instead of the in-memory TestServer
            builder.UseEnvironment("Development");
            builder.ConfigureWebHost(webHostBuilder =>
            {
                webHostBuilder.UseKestrel(options =>
                {
                    // Use a fixed port for simplicity
                    options.ListenLocalhost(5005);
                });
            });

            var host = builder.Build();
            host.Start(); // 🔥 This actually starts the server

            return host;
        }
    }
