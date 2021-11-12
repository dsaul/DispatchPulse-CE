using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using API.Hubs;
using System.Net.Mail;
using System.Net;
using System;
using System.Linq;
using System.Collections.Generic;

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
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			if (string.IsNullOrWhiteSpace(SharedCode.EMail.Konstants.SMTP_HOST_FQDN))
				throw new Exception("SMTP_HOST_FQDN_FILE not set.");
			if (null == SharedCode.EMail.Konstants.SMTP_HOST_PORT)
				throw new Exception("SMTP_HOST_PORT_FILE not set.");
			if (string.IsNullOrWhiteSpace(SharedCode.EMail.Konstants.SMTP_USERNAME))
				throw new Exception("SMTP_USERNAME_FILE not set.");
			if (string.IsNullOrWhiteSpace(SharedCode.EMail.Konstants.SMTP_PASSWORD))
				throw new Exception("SMTP_PASSWORD_FILE not set.");

			IEnumerable<string>? corsOrigins = SharedCode.CORS.Konstants.CORS_ORIGINS;
			if (null == corsOrigins)
				throw new Exception("CORS_ORIGINS_FILE not set, or not JSON array.");

			services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
			{
				builder
					.AllowAnyMethod()
					.AllowAnyHeader()
					.AllowCredentials()
					.WithOrigins(corsOrigins.ToArray());
			}));


			//services.AddApplicationInsightsTelemetry(Configuration);
			services.AddResponseCompression(options =>
			{
				options.EnableForHttps = true;
				//options.Providers.Add<GzipCompressionProvider>();
				options.MimeTypes = new[] {
					"text/plain",
					"text/css",
					"application/javascript",
					"text/html",
					"application/xml",
					"text/xml",
					"application/json",
					"text/json"
				};
			});

			services.Configure<FormOptions>(options => {
				options.ValueCountLimit = int.MaxValue;
				options.ValueLengthLimit = int.MaxValue;
				options.KeyLengthLimit = int.MaxValue;
				options.MultipartBodyLengthLimit = int.MaxValue;
				options.MultipartBoundaryLengthLimit = int.MaxValue;
			});

			services.AddSignalR()
				.AddHubOptions<APIHub>(options =>
				{
					options.MaximumReceiveMessageSize = long.MaxValue;
				})
				.AddNewtonsoftJsonProtocol();

			services
				.AddFluentEmail(SharedCode.EMail.Konstants.SMTP_USERNAME)
				.AddRazorRenderer()
				.AddSmtpSender(new SmtpClient(SharedCode.EMail.Konstants.SMTP_HOST_FQDN, SharedCode.EMail.Konstants.SMTP_HOST_PORT.Value) {
					DeliveryMethod = SmtpDeliveryMethod.Network,
					Credentials = new NetworkCredential(SharedCode.EMail.Konstants.SMTP_USERNAME, SharedCode.EMail.Konstants.SMTP_PASSWORD)
					}
				);



			services.AddControllers().AddNewtonsoftJson();
			

		}
























		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseResponseCompression();

			app.UseCors("CorsPolicy");

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();


			var t = typeof(APIHub);

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapHub<APIHub>("/APIHub");
				endpoints.MapControllers();
				
			});


			app.Run(async (context) =>
			{
				await context.Response.WriteAsync("MVC didn't find anything.").ConfigureAwait(false);
			});

		}
	}
}
