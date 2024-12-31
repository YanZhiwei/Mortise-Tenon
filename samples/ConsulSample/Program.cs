
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Tenon.Abstractions;
using Tenon.AspNetCore;
using Tenon.Infra.Consul.Extensions;

namespace ConsulSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            var startAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            var serviceInfo = WebServiceDescriptor.CreateInstance(startAssembly);
            builder.Services.AddConsul(builder.Configuration.GetSection("Consul"));
            builder.Services.AddSingleton<IWebApiServiceDescriptor>(serviceInfo);
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            var app = builder.Build();
           
            app.UseConsulRegistrationCenter(() =>
            {
                var server = app.Services.GetService<IServer>();
                var addressFeature = server.Features.Get<IServerAddressesFeature>();
                return new Uri(addressFeature.Addresses.FirstOrDefault().ToString());
            });
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
