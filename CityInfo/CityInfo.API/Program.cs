using CityInfo.API;
using CityInfo.API.DbContexts;
using CityInfo.API.Repositories;
using CityInfo.API.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/cityinfo.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();


//Add services to the container.
builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
}).AddNewtonsoftJson()
.AddXmlDataContractSerializerFormatters();

// it is sufficient because the view will be a json
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer(); // used internally by swashbuckle
builder.Services.AddSwaggerGen(setupAction =>
{
    var xmlCommentsPath = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var fullxmlCommentsPath= Path.Combine(AppContext.BaseDirectory, xmlCommentsPath);

    setupAction.IncludeXmlComments(fullxmlCommentsPath);

    setupAction.AddSecurityDefinition("CityInfoApiBearerAuth", new OpenApiSecurityScheme() { 
        Type =  SecuritySchemeType.Http,
        Scheme = "Bearer",
        Description = "Input a valid token to access the CityInfo API"
    });

    // OpenApiSecurityRequirement consists of a dictionary where:
    // key is OpenApiSecurityScheme with reference to the one added above when adding the scheme
    // value is a list of strings for tokens (nop=t applicable in this demo
    setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "CityInfoApiBearerAuth"
                }                
            },
            new List<string>()
        }
    });
}); // generates the specs

//By registering this singleton service, the application ensures that there is only one instance of the service throughout the lifetime of the application.
//This can help improve performance and reduce memory usage, since multiple instances of the same service do not need to be created and managed.
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

// use this to switch between what we want to use in debug mode and what we want to use in release mode
#if DEBUG
// whenever we have an IMailService we will get a LocalMailService
builder.Services.AddTransient<IMailService, LocalMailService>();
#else
builder.Services.AddTransient<IMailService, CloudMailService>();
#endif

builder.Services.AddSingleton<CitiesDataStore>();

builder.Services.AddDbContext<CityInfoContext>(
    dbContextOptions => dbContextOptions.UseSqlite(
        builder.Configuration["ConnectionStrings:CityInfoDBConnectionString"]));

builder.Services.AddScoped<ICityInfoRepository, CityInfoRepository>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddAuthentication("Bearer").AddJwtBearer(options =>
{
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Authentication:Issuer"],
        ValidAudience = builder.Configuration["Authentication:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Authentication:SecretForKey"]))
    };
});

builder.Services.AddAuthorization(options =>
    options.AddPolicy("MustBeFromAntwerp", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("city", "Antwerp");
    }));

builder.Services.AddApiVersioning(setupAction =>
{
    setupAction.AssumeDefaultVersionWhenUnspecified = true;
    setupAction.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    setupAction.ReportApiVersions = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline. Be careful of the order 
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

// This can be done only in .NET 7, not below
//app.MapControllers();

// This is how we match endpoiints
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

/*
// Sets up a middleware function that will handle incoming HTTP requests.
// async keyword indicates that this function will execute asynchronously,
// which allows other requests to be handled while this one is being processed.
app.Run(async (context) =>
{
    // This line sends the string "Hello World" back to the client as the response body
    // The await keyword is used here to indicate that this operation may take some time to complete,
    // but the thread executing the middleware function can continue running other code in the meantime.
    await context.Response.WriteAsync("Hello World");
});

// Middleware is used to handle requests and responses in a pipeline-like manner.
// Each middleware component in the pipeline can perform some action on the request or response,
// modify it if necessary, and pass it on to the next middleware component in the pipeline.
*/
app.Run();
