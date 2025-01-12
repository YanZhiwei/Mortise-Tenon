using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Tenon.AspNetCore.OpenApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 添加认证
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtConfig = builder.Configuration.GetSection("Jwt");
    options.Authority = jwtConfig["Authority"];
    options.RequireHttpsMetadata = false; // 开发环境可以禁用 HTTPS
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtConfig["Issuer"],
        ValidAudience = jwtConfig["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtConfig["Key"] ?? string.Empty)
        )
    };
});

// 添加授权
builder.Services.AddAuthorization(options =>
{
    // 基础 API 访问策略
    options.AddPolicy("WeatherApiScope", policy =>
        policy.RequireClaim("scope", "weather_api"));

    // 读取权限策略
    options.AddPolicy("WeatherApiReadScope", policy =>
        policy.RequireClaim("scope", "weather_api.read"));

    // 写入权限策略
    options.AddPolicy("WeatherApiWriteScope", policy =>
        policy.RequireClaim("scope", "weather_api.write"));

    // 管理员权限策略
    options.AddPolicy("WeatherApiAdminScope", policy =>
        policy.RequireClaim("scope", "weather_api.admin"));
});

// 添加控制器
builder.Services.AddControllers();

// 开发环境配置
if (builder.Environment.IsDevelopment())
{
    // 添加 OpenAPI 服务
    builder.Services.AddScalarOpenApi(builder.Configuration.GetSection("ScalarUI"));
}

var app = builder.Build();

// 开发环境配置
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseScalarOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// 使用路由
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 映射控制器
app.MapControllers();

app.Run(); 