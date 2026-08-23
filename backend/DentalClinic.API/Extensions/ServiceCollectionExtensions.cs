using System.Text;
using DentalClinic.API.Configuration;
using DentalClinic.API.Data;
using DentalClinic.API.Services.Implementations;
using DentalClinic.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace DentalClinic.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDentalClinicInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        var connectionString = configuration.GetConnectionString("DentalClinicDb")
            ?? throw new InvalidOperationException("Connection string 'DentalClinicDb' is missing.");

        services.AddDbContext<DentalClinicDbContext>(options =>
            options.UseMySql(
                connectionString,
                new MariaDbServerVersion(new Version(10, 4, 32)),
                mySqlOptions =>
                {
                    mySqlOptions.EnableStringComparisonTranslations();
                }));

        services.AddHttpContextAccessor();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IVisitService, VisitService>();
        services.AddScoped<ITreatmentCatalogService, TreatmentCatalogService>();
        services.AddScoped<IPatientTreatmentService, PatientTreatmentService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentMethodService, PaymentMethodService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IExpenseCategoryService, ExpenseCategoryService>();

        // File storage and attachments
        services.AddSingleton<IFileStorage, DentalClinic.API.Services.Implementations.LocalFileStorage>();
        services.AddScoped<IAttachmentService, DentalClinic.API.Services.Implementations.AttachmentService>();

        return services;
    }

    public static IServiceCollection AddDentalClinicAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings are missing.");

        if (string.IsNullOrWhiteSpace(jwtSettings.Secret) || jwtSettings.Secret.Length < 32)
        {
            throw new InvalidOperationException("JWT secret must be at least 32 characters.");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddDentalClinicSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Dental Clinic API",
                Version = "v1",
                Description = "ASP.NET Core API for dental clinic management (MariaDB 10.4). " +
                              "All endpoints except login require JWT Bearer authentication. " +
                              "Rate limiting: 5 login attempts per 15 minutes per IP.",
                Contact = new OpenApiContact
                {
                    Name = "Dental Clinic API Support"
                }
            });

            // Include XML comments for better documentation
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // JWT Bearer Authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. " +
                              "Enter 'Bearer' [space] and then your token in the text input below. " +
                              "Example: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Operation filter to handle ApiResponse<T> envelope
            options.OperationFilter<ApiResponseOperationFilter>();
        });

        return services;
    }
}
