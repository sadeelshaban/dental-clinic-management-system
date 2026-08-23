using System.IO;
using Microsoft.Extensions.FileProviders;
using DentalClinic.API.Data;
using DentalClinic.API.Extensions;
using DentalClinic.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDentalClinicInfrastructure(builder.Configuration);
builder.Services.AddDentalClinicAuthentication(builder.Configuration);
builder.Services.AddDentalClinicSwagger();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000",
                "http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Dental Clinic API v1");
        options.RoutePrefix = string.Empty;
    });

    await DatabaseSeeder.SeedDevelopmentDataAsync(
        app.Services,
        app.Logger);
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Ensure uploads folder exists and serve static files under /uploads
var fileStorage = app.Services.GetService<DentalClinic.API.Services.Interfaces.IFileStorage>();
if (fileStorage != null)
{
    fileStorage.EnsureStorageExists();
}

var uploadsPath = app.Configuration["Uploads:Path"] ?? "uploads";
var uploadsFull = Path.Combine(AppContext.BaseDirectory, uploadsPath);
if (Directory.Exists(uploadsFull))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsFull),
        RequestPath = "/uploads"
    });
}

app.Run();
