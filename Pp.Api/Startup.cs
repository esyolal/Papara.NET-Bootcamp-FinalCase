using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AutoMapper;
using Pp.Data.Context;
using Pp.Business.Token;
using Pp.Business.Mapper;
using Microsoft.OpenApi.Models;
using Pp.Base.Token;
using Pp.Business.Cqrs;
using System.Reflection;
using MediatR;
using Pp.Data.UnitOfWork;
using Pp.Business.Services;
using System.Security.Claims;
using System.Security.Cryptography;

public class Startup
{
    private readonly IWebHostEnvironment environment;

    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        Configuration = configuration;
        this.environment = environment;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<ICreditCardService, CreditCardService>();

        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddMediatR(typeof(RegisterUserCommand).GetTypeInfo().Assembly);

        var jwtConfig = Configuration.GetSection("JwtConfig").Get<JwtConfig>()
            ?? throw new InvalidOperationException("JwtConfig is missing.");
        if (string.IsNullOrWhiteSpace(jwtConfig.Secret) &&
            environment.IsDevelopment())
        {
            jwtConfig.Secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }

        if (string.IsNullOrWhiteSpace(jwtConfig.Secret) || Encoding.UTF8.GetByteCount(jwtConfig.Secret) < 32)
        {
            throw new InvalidOperationException("Configure JwtConfig:Secret with at least 32 bytes.");
        }

        services.AddSingleton(jwtConfig);

        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = true;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtConfig.Issuer,
                ValidateIssuerSigningKey = true,
                NameClaimType = ClaimTypes.Name,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret)),
                ValidAudience = jwtConfig.Audience,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2)
            };
        });

        
        var connectionStringSql = Configuration.GetConnectionString("MsSqlConnection");
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionStringSql));

    
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new MapperConfig());
        });
        services.AddSingleton(mapperConfig.CreateMapper());

    
        services.AddScoped<ITokenService, TokenService>();

  
        services.AddControllers();


        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Pp API", Version = "v1" });

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Bearer",
                Description = "Enter JWT Bearer token **_only_**",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, Array.Empty<string>() }
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pp API v1"));
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
