using System.Reflection;

using Docentric.EInvoice.Validator.RestServer.Configuration;
using Docentric.EInvoice.Validator.RestServer.Services;

using JavaService = Docentric.EInvoice.Validator.RestServer.Services.JavaService;

string appTitle = Assembly
    .GetEntryAssembly()?
    .GetCustomAttribute<AssemblyTitleAttribute>()?
    .Title ?? Constants.Application.DefaultTitle;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(Constants.OpenApi.Version, options =>
{
    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
});

// Add Request Timeout services
builder.Services.AddRequestTimeouts(options =>
{
    options.DefaultPolicy = new Microsoft.AspNetCore.Http.Timeouts.RequestTimeoutPolicy
    {
        Timeout = Constants.RequestTimeouts.DefaultTimeout, // Default 60 second timeout
        TimeoutStatusCode = StatusCodes.Status408RequestTimeout
    };

    // Add named policies for specific endpoints
    options.AddPolicy(Constants.RequestTimeouts.LongRunningPolicy, Constants.RequestTimeouts.LongRunning);
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

// Add Request Timeout middleware (must be early in pipeline)
app.UseRequestTimeouts();

// API endpoints registrations
app.MapApiEndpoints();

// Map OpenAPI endpoints from separate class
app.MapOpenApi(Constants.OpenApi.V1UriSufix);

// Register Swagger / ReDoc middlewares
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(Constants.OpenApi.V1UriSufix, $"{appTitle} API {Constants.OpenApi.Version}");
    options.RoutePrefix = "api/docs/swagger";
});
app.UseReDoc(options =>
{
    options.SpecUrl = Constants.OpenApi.V1UriSufix;
    options.RoutePrefix = "api/docs/redoc";
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
