using System;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using JiraWorklogSubmitter.Config;
using JiraWorklogSubmitter.Services;
using JiraWorklogSubmitter.Services.Interfaces;

namespace JiraWorklogSubmitter
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public JiraSettings JiraSettings { get; private set; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build();

            JiraSettings = Configuration.GetSection(nameof(JiraSettings)).Get<JiraSettings>();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // This enables us to inject JiraSettings into razor pages
            services.Configure<JiraSettings>(Configuration.GetSection(nameof(JiraSettings)));

            // Newtonsoft has better DateTime handeling, which we need becuase Jira's response is in a format System.Text.Json doesn't support
            services.AddRazorPages().AddNewtonsoftJson();
            services.AddServerSideBlazor();

            AddTransients(services);

            services.AddHttpClient();
            services.AddHttpClient(HttpClientFactoryNameEmum.Jira.ToString(), client =>
            {
                client.BaseAddress = new Uri(JiraSettings.BaseUrl);   //TODO: Get this from appsettings.local.json
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", GetEncodedEmailAndToken());
            });
        }

        private void AddTransients(IServiceCollection services)
        {
            services.AddTransient<ITimeEntryService, TimeEntryService>();
        }

        private string GetEncodedEmailAndToken()
        {
            var email = JiraSettings.Email;
            var token = JiraSettings.Token;
            var utf8EmailAndToken = Encoding.UTF8.GetBytes($"{email}:{token}");
            var base64Utf8EmailAndToken = Convert.ToBase64String(utf8EmailAndToken);

            return base64Utf8EmailAndToken;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
