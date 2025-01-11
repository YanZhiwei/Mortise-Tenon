using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Tenon.AspNetCore.OpenApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 添加认证
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty))
        };
    });

// 添加授权
builder.Services.AddAuthorization();

// 添加控制器
builder.Services.AddControllers();

// 添加 OpenAPI 服务
builder.Services.AddScalarOpenApi(options =>
{
    options.Title = "Tenon OpenAPI 示例";
    options.Version = "v1";
    options.Description = "这是一个使用 Scalar UI 的 OpenAPI 示例项目";

    // OAuth2 配置
    options.OAuth2 = new()
    {
        Authority = builder.Configuration["Jwt:Authority"] ?? string.Empty,
        ClientId = builder.Configuration["Jwt:ClientId"] ?? string.Empty,
        Scopes = new List<string> { "api1" }
    };

    // 主题配置
    options.Theme = new()
    {
        DarkMode = true,
        Colors = new Dictionary<string, string>
        {
            { "primary", "#1976d2" }
        }
    };
});

var app = builder.Build();

// 配置 HTTP 请求管道
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 使用 OpenAPI
app.UseScalarOpenApi();

app.MapControllers();

app.Run(); 