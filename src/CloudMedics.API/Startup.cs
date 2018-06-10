﻿using CloudMedics.API.Helpers.Extensions;
using CloudMedics.Data;
using CloudMedics.Data.Repositories;
using CloudMedics.Domain.Models;
using CouldMedics.Services.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CloudMedics.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectString = Configuration.GetConnectionString("cloudmedicsDbConnection");
            services.AddDbContext<CloudMedicDbContext>(options => options.UseMySql(connectString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                   .AddEntityFrameworkStores<CloudMedicDbContext>()
                   .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options => options.ConfigureIdentiyOptions());
            services.AddAuthentication(option =>
            {
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options => options.ConfigureJWTBearerOptions(Configuration));

            services.AddCors(options => options.ConfigureCorsPolicy());
            //register framework services
            services.AddMvc();
            //register application services
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<AppDbInitializer>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, AppDbInitializer dbInitializer)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

            }

            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseMvc();
            dbInitializer.Seed().Wait();
        }


        #region privates
        #endregion
    }
}
