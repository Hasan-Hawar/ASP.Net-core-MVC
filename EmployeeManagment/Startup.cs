using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagment.Models;
using EmployeeManagment.Secutity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EmployeeManagment
{
	public class Startup
	{
		private IConfiguration _config;

		public Startup(IConfiguration config)
		{
			_config = config;
		}

		//private bool AuthorizationAccess(AuthorizationHandlerContext context)
		//{
		//	return context.User.IsInRole("Admin") &&
		//			context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") ||
		//			context.User.IsInRole("Super Admin");
		//}

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContextPool<AppDbContext>(
				options => options.UseSqlServer(_config.GetConnectionString("EmployeeDBConnection")));

			services.AddIdentity<ApplicationUser, IdentityRole>(options =>
			{
				options.Password.RequiredLength = 10;
				options.Password.RequiredUniqueChars = 3;

				options.SignIn.RequireConfirmedEmail = true;

				options.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";

				options.Lockout.MaxFailedAccessAttempts = 5;
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
			})
				.AddEntityFrameworkStores<AppDbContext>()
				.AddDefaultTokenProviders()
				.AddTokenProvider<CustomConfirmationTokenProvider
					<ApplicationUser>>("CustomEmailConfirmation");

			// Change token lifespan of all token types ..
			services.Configure<DataProtectionTokenProviderOptions>(o =>
				o.TokenLifespan = TimeSpan.FromHours(5));

			// Change token lifespan of just the Email Confirmation Token type ...
			services.Configure<CustomEmailConfirmationTokenProviderOptions>(o =>
				o.TokenLifespan = TimeSpan.FromDays(3));



			services.AddMvc(option =>
			{
				var policy = new AuthorizationPolicyBuilder()
								.RequireAuthenticatedUser()
								.Build();
				option.Filters.Add(new AuthorizeFilter(policy));
			}).AddXmlSerializerFormatters();

			services.AddAuthentication()
				.AddGoogle(options =>
				{
					options.ClientId = "270071815073-md6rmlovrbmghj8vegqkd4p4ng27demc.apps.googleusercontent.com";
					options.ClientSecret = "ic1uxhgdQrCXmHb6Owv6exbQ";
					
					// To change the call back path 
					//options.CallbackPath = "";
				})
				.AddFacebook(options => 
				{
					options.AppId = "336154827398232";
					options.AppSecret = "b75c37e106ae8633851c7110b0c34643";
				});

			
			services.ConfigureApplicationCookie(options =>
			{
				options.AccessDeniedPath = new PathString("/Administration/AccessDenied");
			});

			services.AddAuthorization(options => 
			{
				options.AddPolicy("DeleteRolePolicy", 
					policy => policy.RequireClaim("Delete Role"));

				options.AddPolicy("EditRolePolicy",
					policy => policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement()));

				options.AddPolicy("AdminRolePolicy", 
					policy => policy.RequireRole("Admin")); 
			});

			services.AddScoped<IEmployeeRepository, SQLEmployeeRepository>();

			services.AddSingleton<IAuthorizationHandler, CanEditOnlyAdminRolesAndClaimsHandler>();
			services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();
			services.AddSingleton<DataProtectionPurposeStrings>();

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				app.UseStatusCodePagesWithReExecute("/Error/{0}");
			}
			app.UseStaticFiles();
			app.UseAuthentication();
			app.UseMvc(route =>
			{
				route.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
 