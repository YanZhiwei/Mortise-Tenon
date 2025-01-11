using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Tenon.AspNetCore.OpenApi.Extensions;
using Tenon.AspNetCore.OpenApi.Extensions.Options;

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
builder.Services.AddScalarOpenApi(builder.Configuration.GetSection("ScalarUI"));

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