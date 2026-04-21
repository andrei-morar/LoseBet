using LoseBet.API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Spunem aplicației să folosească fișierele de tip Controller (adică AuthController-ul tău)
builder.Services.AddControllers();

// 2. Conectăm Baza de Date
builder.Services.AddDbContext<LoseBetDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Pregătim Swagger-ul
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. Activăm interfața grafică pentru API (doar pe calculatorul tău, în Development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 5. Activăm rutele
app.MapControllers();

app.Run();