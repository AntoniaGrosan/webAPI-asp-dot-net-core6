using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using System.Numerics;
using System.Threading;
using Microsoft.AspNetCore.Http;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

//Add services to the container.

builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
}).AddNewtonsoftJson()
.AddXmlDataContractSerializerFormatters();

// it is sufficient because the view will be a json
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//By registering this singleton service, the application ensures that there is only one instance of the service throughout the lifetime of the application.
//This can help improve performance and reduce memory usage, since multiple instances of the same service do not need to be created and managed.
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline. Be careful of the order 
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

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