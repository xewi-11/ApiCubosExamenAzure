using ApiCubosExamenAzure.Data;
using ApiCubosExamenAzure.Repositories;
using ApiCubosExamenAzure.Services;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configurar Azure Key Vault leyendo la URL del appsettings
// En vez de registrar AddAzureClients, añadimos el KeyVault DIRECTAMENTE a las
// configuraciones de la app para que GetValue<> pueda leer los secretos.
builder.Services.AddAzureClients(factory =>
{
    factory.AddSecretClient(builder.Configuration.GetSection("KeyVault:VaultUri"));
});

// Inicializar el helper estático de criptografía con la nueva configuración
ApiOAuthEmpleados.Helpers.HelperCryptography.Initialize(builder.Configuration);

// Add services to the container.

builder.Services.AddControllers();

// Al usar AddAzureKeyVault, AzureStorage--BlobUriUser es convertido automáticamente
// a "AzureStorage:BlobUriUser". Ya podemos llamarlo normal.
string? blobUri = builder.Configuration.GetSection("AzureStorage:BlobUriUser").Value;

if (string.IsNullOrEmpty(blobUri))
{
    throw new Exception("La configuración de AzureStorage:BlobUriUser es nula.");
}

// Use the connection string constructor to properly initialize the client
BlobServiceClient blobServiceClient = new BlobServiceClient(blobUri);
builder.Services.AddTransient<BlobServiceClient>(x => blobServiceClient);
builder.Services.AddTransient<BlobService>();

// Y ConnectionStrings--SqlAzure es convertido a GetConnectionString("SqlAzure")
builder.Services.AddDbContext<CubosContext>(options =>
{
    string? sqlString = builder.Configuration.GetConnectionString("SqlAzure");
    if (string.IsNullOrEmpty(sqlString)) throw new Exception("La configuración de BD es nula.");

    options.UseSqlServer(sqlString);
});

builder.Services.AddTransient<RepositoryCubos>();
// Add the missing registration for RepositoryUsuario
builder.Services.AddTransient<RepositoryUsuario>();
// Add the missing registration for the Helper
builder.Services.AddSingleton<ApiOAuthEmpleados.Helpers.HelperActionOAuthService>();

// Obtenemos el HelperActionOAuthService previamente registrado o lo extraemos de la configuración
ApiOAuthEmpleados.Helpers.HelperActionOAuthService helper =
    new ApiOAuthEmpleados.Helpers.HelperActionOAuthService(builder.Configuration);

// Faltaba configurar la autenticación JWT
builder.Services.AddAuthentication(helper.GetAuthenticationSchema())
    .AddJwtBearer(helper.GetJWtBearerOptions());

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/", context =>
{
    context.Response.Redirect("/scalar");
    return Task.CompletedTask;
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();




