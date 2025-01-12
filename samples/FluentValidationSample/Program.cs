using FluentValidation;
using FluentValidationSample.Models;
using FluentValidationSample.Services;
using FluentValidationSample.Validators;
using Tenon.AspNetCore.OpenApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 添加 FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<UserRegistrationValidator>();
builder.Services.AddScoped<IValidator<UserRegistrationRequest>, UserRegistrationValidator>();

// 添加业务服务
builder.Services.AddScoped<IUserService, UserService>();

// 配置 Scalar OpenAPI
builder.Services.AddScalarOpenApi(builder.Configuration.GetSection("ScalarUI"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseScalarOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
