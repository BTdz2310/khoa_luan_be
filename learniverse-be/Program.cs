using System.Text;
using System.Text.Json;
using Amazon.S3;
using learniverse_be.Data;
using learniverse_be.Extensions;
using learniverse_be.Models;
using learniverse_be.Services;
using learniverse_be.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowAll", policy =>
  {
    policy.AllowAnyOrigin()
      .AllowAnyMethod()
      .AllowAnyHeader();
  });
});

builder.Services.AddControllers()
  .ConfigureApiBehaviorOptions(options =>
  {
    // Override the default invalid model state response
    // InvalidModelStateResponseFactory: This delegate is invoked when model validation fails.
    // By overriding it, we can control the structure and content of the validation error responses.
    options.InvalidModelStateResponseFactory = context =>
    {
      // Extract the error messages from the model state
      var errors = context.ModelState
        .Where(e => e.Value?.Errors.Count > 0)
        .Select(e => new FieldError
        {
          Field = e.Key,
          // Option 1: Use only the first error message
          // Error = e.Value.Errors.FirstOrDefault()?.ErrorMessage
          // Join multiple error messages into a single string separated by semicolons
          Error = string.Join("; ", e.Value?.Errors.Select(x => x.ErrorMessage ?? string.Empty) ?? Array.Empty<string>())
        }).ToList();
      // Create a custom error response object
      return ResponseHelper.Fail("Dữ liệu không hợp lệ", errors);
    };
  }).AddJsonOptions(options =>
  {
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    // options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
  });

// var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
// var secretKey = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

// builder.Services.AddAuthentication(options =>
//   {
//     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//   })
//   .AddJwtBearer(options =>
//   {
//     options.TokenValidationParameters = new TokenValidationParameters
//     {
//       ValidateIssuer = true,
//       ValidateAudience = true,
//       ValidateIssuerSigningKey = true,
//       ValidateLifetime = true,
//       ValidIssuer = jwtSettings.Issuer,
//       ValidAudience = jwtSettings.Audience,
//       IssuerSigningKey = new SymmetricSecurityKey(secretKey)
//     };
//   });


builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
  var jwtSection = builder.Configuration.GetSection("JwtSettings");
  var secretKey = Encoding.UTF8.GetBytes(jwtSection.GetValue<string>("SecretKey"));

  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateIssuerSigningKey = true,
    ValidateLifetime = true,
    ValidIssuer = jwtSection.GetValue<string>("Issuer"),
    ValidAudience = jwtSection.GetValue<string>("Audience"),
    IssuerSigningKey = new SymmetricSecurityKey(secretKey)
  };
});

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

builder.Services.AddTransient<IMailService, MailService>();

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

  c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    In = ParameterLocation.Header,
    Description = "Enter 'Bearer {token}'",
    Name = "Authorization",
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer"
  });

  c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

builder.Services.AddSwaggerGen(c =>
{
  c.EnableAnnotations();
});

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddScoped<IS3Service, S3Service>();

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IInstructorService, InstructorService>();

builder.WebHost.ConfigureKestrel(options =>
{
  options.ListenAnyIP(8080);
});

var app = builder.Build();

// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//     db.Database.Migrate();
// }

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
  app.UseSwagger();
  app.UseSwaggerUI(c =>
  {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
  });
}
app.ApplyMigrations();

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// app.MapStaticAssets();

// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Home}/{action=Index}/{id?}")
//     .WithStaticAssets();

app.MapControllers();

app.Run();
