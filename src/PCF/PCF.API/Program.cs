using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PCF.Data.Context;
using PCF.Data.Interface;
using PCF.Data.Repository;

var builder = WebApplication.CreateBuilder(args);

//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.AllowAnyOrigin() // Permite qualquer origem (ajuste conforme necess�rio)
               .AllowAnyMethod() // Permite todos os m�todos (GET, POST, etc.)
               .AllowAnyHeader(); // Permite todos os cabe�alhos
    });
});


// Adiciona a Connection String do SQLite
var connectionString = builder.Configuration.GetConnectionString("SQLiteConnection");
builder.Services.AddDbContext<PCFDBContext>(options =>
    options.UseSqlite(connectionString));

// Configura servi�os do Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<PCFDBContext>()
    .AddDefaultTokenProviders();

// Adiciona servi�os da API
builder.Services.AddControllers();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Configura Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var dbContext = services.GetRequiredService<PCFDBContext>();
            await dbContext.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao aplicar migrations: {ex.Message}");
        }
    }

    // Configura��es do Swagger
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PCF API v1");
        c.RoutePrefix = string.Empty; // Faz com que o Swagger seja a p�gina inicial
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors("CorsPolicy");
app.MapControllers();

app.Run();
