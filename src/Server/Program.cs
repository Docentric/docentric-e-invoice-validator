using System.Reflection;

using Docentric.ZuGFeRD.Validator.RestServer.Configuration;
using Docentric.ZuGFeRD.Validator.RestServer.Services;

const string openApiVersion = "v1";
const string openApiV1UriSufix = $"/openapi/{openApiVersion}.json";

string appTitle = Assembly
    .GetEntryAssembly()?
    .GetCustomAttribute<AssemblyTitleAttribute>()?
    .Title ?? "Docentric ZuGFeRD and Factur-X Validator (Mustang Project .NET Test Tool)";


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(openApiVersion, options =>
{
    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
});

// Define CORS for development only
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevSpa", policy =>
        policy.WithOrigins("http://localhost:53365")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Add this line with your other service registrations
builder.Services.AddSingleton<JavaService>();
builder.Services.AddSingleton<MustangCliService>();

WebApplication app = builder.Build();

// API endpoints registrations
app.MapApiEndpoints();

// Map OpenAPI endpoints from separate class
app.MapOpenApi(openApiV1UriSufix);

// Register Swagger / ReDoc middlewares
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(openApiV1UriSufix, $"{appTitle} API {openApiVersion}");
    options.RoutePrefix = "api/docs/swagger";
    //options.DocumentTitle = $"{appTitle} API {openApiVersion} - Swagger UI";
});
app.UseReDoc(options =>
{
    options.SpecUrl = openApiV1UriSufix;
    options.RoutePrefix = "api/docs/redoc";
    //options.DocumentTitle = $"{appTitle} API {openApiVersion} - ReDoc";
});

// Configure for reverse proxy scenarios (Azure App Service, nginx, etc.)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                       Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseCors("DevSpa");
}

// Serve the built SPA from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// Fallback to index.html for any non-API route
app.MapFallbackToFile("index.html");

app.Run();
