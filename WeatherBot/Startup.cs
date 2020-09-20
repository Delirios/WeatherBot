// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EmptyBot v4.10.3

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.Bot.Builder.Azure;
using WeatherBot.BusinessLogic;
using WeatherBot.Dialogs;
using WeatherBot.Services;

namespace WeatherBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            services.AddSingleton <BotServices>();

            ConfigureDialogs(services);

            ConfigureState(services);

            services.AddTransient<IWeatherService, WeatherService>();
            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, Bots.WeatherBot<MainDialog>>();

        }

        private void ConfigureState(IServiceCollection services)
        {
            var storageAccount = "DefaultEndpointsProtocol=https;AccountName=allweatherstorage;AccountKey=+JKmeSEYuY9TlpoZLA/QmM7rK65roGgtxcXwGR0FCtWm4kutb0LyyBwr4nfVdFYjdkzIX8PMekYKZCazxz3pLg==;EndpointSuffix=core.windows.net";
            var storageContainer = "mystatedata";

            services.AddSingleton<IStorage>(new AzureBlobStorage(storageAccount, storageContainer));

            services.AddSingleton<ConversationState>();

            services.AddSingleton<StateService>();
        }

        private void ConfigureDialogs(IServiceCollection services)
        {
            services.AddSingleton<MainDialog>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
