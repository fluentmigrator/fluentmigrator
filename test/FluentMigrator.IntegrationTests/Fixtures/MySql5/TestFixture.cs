using System;
using System.IO;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace FluentMigrator.IntegrationTests.Fixtures.MySql5
{
    public class TestFixture : IDisposable
    {
        public readonly TestServer Server;

        public TestFixture()
        {
            var webHostBuilder = WebHost.CreateDefaultBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>();

            Server = new TestServer(webHostBuilder);
        }

        public void Dispose()
        {
            Server.Dispose();
        }
    }
}
