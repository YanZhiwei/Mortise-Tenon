using FluentValidation;
using FluentValidationSample.Validators;
using Tenon.AspNetCore.OpenApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 添加 FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<UserRegistrationValidator>();

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
