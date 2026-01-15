using ProjectSaas.Api.Common.Extensions;


var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger (OpenAPI) for .NET 8
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Application and Infrastructure Services(AppDbContext configured here)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Optional: comment out for now if HTTPS causes confusion

app.UseAuthorization();

app.MapControllers();

app.Run();
