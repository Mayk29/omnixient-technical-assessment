using CnbExchangeRates.Configuration;
using CnbExchangeRates.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Services ───────────────────────────────────────────────────────────────

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title   = "CNB Exchange Rates API",
        Version = "v1",
        Description = "Proxies daily exchange rate data from the Czech National Bank."
    });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// Configuration
builder.Services.Configure<CnbApiOptions>(
    builder.Configuration.GetSection(CnbApiOptions.SectionName));

// Named HttpClient for ExchangeRateProvider (respects BaseUrl from config)
builder.Services.AddHttpClient<IExchangeRateProvider, ExchangeRateProvider>(client =>
{
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(15);
});

// CORS — allow Angular dev server and any configured origin
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ── Pipeline ───────────────────────────────────────────────────────────────

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();
app.Run();

// Partial class so integration tests can reference the entry point
public partial class Program { }
