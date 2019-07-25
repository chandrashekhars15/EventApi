using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApi.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.AspNetCore.Cors.Infrastructure;
using EventApi.CustomFormatter;
using EventApi.CustomFilters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EventApi
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

            services.AddDbContext<EventDbContext>(options =>
            { 
                options.UseInMemoryDatabase(databaseName: "EventDb");
                //options.UseSqlServer(Configuration.GetConnectionString("EventSqlConnection"));
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title="Event API",
                    Version = "v1",
                    Contact=new Contact { Name="Chandrashekhar S", Email="chandrashekhars@hexaware.com" }
                });
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration.GetValue<string>("Jwt:Issuer"),
                        ValidAudience = Configuration.GetValue<string>("Jwt:Audience"),
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("Jwt:Secret")))
                    };

                });

            services.AddCors(c=>
            {
                c.AddPolicy("MSPolicy", builder =>
                {
                    builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });
                c.AddPolicy("SysnPolicy", builder =>
                {
                    builder.WithOrigins("*.synergetics-india.com")
                    .WithMethods("GET")
                    .WithHeaders("Autherization","Content-Type","Accept");
                });
                c.DefaultPolicyName = "MSPolicy";
            });

            services.AddMvc(c=>
            {
                c.Filters.Add(typeof(CustomExceptionHandler));
                c.OutputFormatters.Add(new CsvCustomFormatter());
            })
                .AddXmlDataContractSerializerFormatters()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseCors(c =>
            //{
            //    c.WithOrigins("*.microsoft.com")
            //        .AllowAnyMethod()
            //        .AllowAnyHeader();
            //    c.WithOrigins("*.synergetics-india.com")
            //        .WithMethods("GET")
            //        .WithHeaders("Authorization", "Content-Type", "Accept");
            //});

            InitializeDatabase(app);

            app.UseCors();

            app.UseSwagger();
            app.UseAuthentication();
    
            if (env.IsDevelopment())
            {
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event API");
                });
            }
            
            app.UseMvc();
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetService<EventDbContext>();

                db.Events.Add(new Models.EventInfo
                {
                    Title = "Sample Event1",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(2),
                    StartTime = "9:00 AM",
                    EndTime = "5:30 PM",
                    Host = "Microsoft",
                    Speaker = "Shekhar",
                    RegistrationUrl = "https://events.microsoft.com/1234"
                });

                db.Events.Add(new Models.EventInfo
                {
                    Title = "Sample Event2",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(2),
                    StartTime = "9:00 AM",
                    EndTime = "5:30 PM",
                    Host = "Google",
                    Speaker = "Shekhar",
                    RegistrationUrl = "https://events.google.com/1234"
                });
                db.SaveChanges();
            }
        }
    }
}
