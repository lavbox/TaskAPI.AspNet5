using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Data.Entity;
using Microsoft.Dnx.Runtime;
using Microsoft.Dnx.Runtime.Infrastructure;
using Microsoft.Framework.DependencyInjection;
using Swashbuckle.Swagger;
using Swashbuckle.Swagger.XmlComments;
using TaskAPI.Models;

namespace Web_Api_Sample
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)  {  }

        public void ConfigureServices(IServiceCollection services)
        {
            var appEnv = CallContextServiceLocator.Locator.ServiceProvider.GetRequiredService<IApplicationEnvironment>();

            // using inmemory database
            services
                .AddEntityFramework()
                .AddInMemoryDatabase()
                .AddDbContext<TaskContext>(options => options.UseInMemoryDatabase(true));

            services
                .AddMvc()
                .AddJsonOptions(options =>
            {
                //handling of circular references
                options.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.All;
            });

            // reading settings from json config
            var configBuilder = new Microsoft.Framework.Configuration.ConfigurationBuilder(appEnv.ApplicationBasePath, null);
            configBuilder.Add(new Microsoft.Framework.Configuration.Json.JsonConfigurationSource(appEnv.ApplicationBasePath + "//appsettings.json"));
            var appsettings = configBuilder.Build();

            // swagger configuration
            string path = string.Format(@"{0}\WebApiSwagger.XML", appEnv.ApplicationBasePath);
            services.AddSwagger(s =>
            {
                s.SwaggerGeneratorOptions.Schemes = new[] { "http", "https" };
                s.SwaggerGeneratorOptions.SingleApiVersion(new Info
                {
                    Version = appsettings["Swagger:Version"],
                    Title = appsettings["Swagger:Title"],
                    Description = appsettings["Swagger:Description"],
                });
                s.IncludeXmlComments(path);
                s.SchemaGeneratorOptions.DescribeAllEnumsAsStrings = true;
                s.SchemaGeneratorOptions.ModelFilters.Add(new ApplyXmlTypeComments(path));
                s.SwaggerGeneratorOptions.OperationFilters.Add(new ApplyXmlActionComments(path));
                s.SwaggerGeneratorOptions.BasePath = appsettings["Swagger:BaseURL"];
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStaticFiles();
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUi();
        }
    }
}
