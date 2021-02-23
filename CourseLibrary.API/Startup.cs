using AutoMapper;
using CourseLibrary.API.DbContexts;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using System;

namespace CourseLibrary.API
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
            services.AddControllers(setupAction =>
            {
                setupAction.ReturnHttpNotAcceptable = true;

            })
            
            .AddNewtonsoftJson(setupAction =>
            {
                setupAction.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();
            })
            .AddXmlDataContractSerializerFormatters()
            .ConfigureApiBehaviorOptions(setupAction =>
            {
                setupAction.InvalidModelStateResponseFactory = context =>
                    {

                    var problemDetailsFactory = context.HttpContext.RequestServices
                    .GetRequiredService<ProblemDetailsFactory>();

                    var ProblemDetails = problemDetailsFactory.CreateValidationProblemDetails(
                    context.HttpContext,
                    context.ModelState);

                    ProblemDetails.Detail = "see the errors field for details.";
                    ProblemDetails.Instance = context.HttpContext.Request.Path;

                    var actionExecutingContext =
                        context as Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext;

                        if ((context.ModelState.ErrorCount > 0) &&
                            (actionExecutingContext?.ActionArguments.Count ==
                            context.ActionDescriptor.Parameters.Count))
                        {
                            ProblemDetails.Type = "https://courselibrary.com/modelvalidationproblem";
                            ProblemDetails.Status = StatusCodes.Status422UnprocessableEntity;
                            ProblemDetails.Title = "One or moe validation errors occured.";

                            return new UnprocessableEntityObjectResult(ProblemDetails)
                            {
                                ContentTypes = { "application/problem+json" }
                            };
                        };

                            ProblemDetails.Status = StatusCodes.Status400BadRequest;
                            ProblemDetails.Title = "one or more errors on input occured";
                            return new BadRequestObjectResult(ProblemDetails)
                            {
                                ContentTypes = { "application/problem+json" }
                            };
                        };

                    });
        

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
             
            services.AddScoped<ICourseLibraryRepository, CourseLibraryRepository>();

            services.AddDbContext<CourseLibraryContext>(options =>
            {
                options.UseSqlServer(
                    @"Server=(localdb)\mssqllocaldb;Database=CourseLibraryDB;Trusted_Connection=True;");
            }); 
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
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again Later ");
                    });
                });
            }
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
