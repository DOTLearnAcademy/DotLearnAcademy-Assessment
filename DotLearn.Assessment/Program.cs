using Amazon;
using DotLearn.Assessment.Data;
using DotLearn.Assessment.Middleware;
using DotLearn.Assessment.Repositories;
using DotLearn.Assessment.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// AWS Secrets Manager (Only in non-Development environments)
if (!builder.Environment.IsDevelopment())
{
    builder.Configuration.AddSecretsManager(region: RegionEndpoint.APSoutheast2);
}

// Add services to the container.
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AssessmentDbContext>(options =>
    options.UseSqlServer(connStr));

builder.Services.AddScoped<IAssessmentRepository, AssessmentRepository>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();

builder.Services.AddHealthChecks().AddSqlServer(connStr!);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Authentication & Authorization (Placeholder)
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// CORS — DOT-24 Security Lockdown
builder.Services.AddCors(options =>
{
    options.AddPolicy("DotLearnPolicy", policy =>
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200",
                builder.Configuration["AllowedOrigins:Ec2"] ?? "",
                builder.Configuration["AllowedOrigins:CloudFront"] ?? "")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Middlewares
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandler>();

app.UseCors("DotLearnPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
