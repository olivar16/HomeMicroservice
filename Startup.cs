using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon;
namespace HomeMicroservice
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
            var AWSConfig = Configuration.GetSection("AWS");
            var client = GenerateDynamoClient(AWSConfig.GetSection("accessKey").Value, AWSConfig.GetSection("secretKey").Value);
            // Add framework services.
            services.AddMvc();
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<AmazonDynamoDBClient>(client);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            app.UseMvc(routes => {
                routes.MapRoute("Attendance", "{controller=Attendance}/{action=Index}");
            });
        }

        private AmazonDynamoDBClient GenerateDynamoClient(string accessKey, string secretKey){
            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var config = new AmazonDynamoDBConfig{
                RegionEndpoint = Amazon.RegionEndpoint.USEast1
            };
            AmazonDynamoDBClient client = new AmazonDynamoDBClient(credentials, config);
            return client;
        }
    }
}
