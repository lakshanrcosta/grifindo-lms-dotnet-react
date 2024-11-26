using grifindo_lms_api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

string connectionString = $"Server={Environment.GetEnvironmentVariable("MSSQL_SERVER")};" +
                          $"Database={Environment.GetEnvironmentVariable("MSSQL_DATABASE")};" +
                          $"User={Environment.GetEnvironmentVariable("MSSQL_USER")};" +
                          $"Password={Environment.GetEnvironmentVariable("MSSQL_PASSWORD")};" +
                          $"TrustServerCertificate={Environment.GetEnvironmentVariable("TRUST_SERVER_CERTIFICATE")};";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
