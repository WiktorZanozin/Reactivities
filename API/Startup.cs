using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Activities;
using MediatR;
using FluentValidation.AspNetCore;
using API.Middleware;
using Domain;
using Application.Interfaces;
using Infrastructure.Security;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Infrastructure.Photos;
using API.SignalR;
using Application.Profiles;

namespace API
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
            services.AddControllers();
            services.AddCors(opt => 
            {
                opt.AddPolicy("CorsPolicy", policy => 
                {
                    policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithExposedHeaders("WWW-Authenticate")
                    .WithOrigins("http://localhost:3000")
                    .AllowCredentials();
                });
            });
            services.AddMediatR(typeof(List.Handler).Assembly);
            services.AddAutoMapper(typeof(List.Handler));
            services.AddSignalR();
            services.AddMvc(opt=>
             {
               var policy=new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
               opt.Filters.Add(new AuthorizeFilter(policy));
             })
                     .AddFluentValidation(cfg => cfg.RegisterValidatorsFromAssemblyContaining<Create>())
                     .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                     .AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            
             services.AddDbContext<DataContext>(opt => 
            {
                opt.UseLazyLoadingProxies();
                opt.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
            });
              services.TryAddSingleton<ISystemClock, SystemClock>();
              var builder = services.AddIdentityCore<AppUser>();
              var identityBuilder = new IdentityBuilder(builder.UserType,  builder.Services);
              identityBuilder.AddEntityFrameworkStores<DataContext>();
              identityBuilder.AddSignInManager<SignInManager<AppUser>>();

            services.AddAuthorization(opt => 
            {
                opt.AddPolicy("IsActivityHost", policy =>
                {
                    policy.Requirements.Add(new IsHostRequirement());
                });
            });
            services.AddTransient<IAuthorizationHandler, IsHostRequirementHandler>();
              var key=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["TokenKey"]));
              services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
              .AddJwtBearer(opt=>
              {
                  opt.TokenValidationParameters=new TokenValidationParameters
                  {
                      ValidateIssuerSigningKey=true,
                      IssuerSigningKey=key,
                      ValidateAudience=false,
                      ValidateIssuer=false,
                      ValidateLifetime=true,
                      ClockSkew=TimeSpan.Zero
                  };
                  opt.Events=new JwtBearerEvents
                  { 
                      OnMessageReceived=context=>
                      {
                          var accessToken=context.Request.Query["access_token"];
                          var path=context.HttpContext.Request.Path;
                          if(!string.IsNullOrEmpty(accessToken) &&
                          (path.StartsWithSegments("/chat")))
                          {
                              context.Token=accessToken;
                          }
                          return Task.CompletedTask;
                      }

                  };
              });
              services.AddScoped<IJwtGenerator, JwtGenerator>();
              services.AddScoped<IUserAccessor, UserAccessor>();
              services.AddScoped<IPhotoAccessor, PhotoAccessor>();
              services.AddScoped<IProfileReader, ProfileReader>();
              services.Configure<CloudinarySettings>(Configuration.GetSection("Cloudinary"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,  IWebHostEnvironment env)
        {
            app.UseMiddleware<ErrorHandlingMiddleware>();
            if (env.IsDevelopment())
            {
               // app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();
            app.UseCors("CorsPolicy");
            app.UseSignalR(routes=>{routes.MapHub<ChatHub>("/chat");});

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
