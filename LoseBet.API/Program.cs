using LoseBet.API.Data;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// 1. Spunem aplicației să folosească fișierele de tip Controller (adică AuthController-ul tău)
builder.Services.AddControllers();

// 2. Conectăm Baza de Date
builder.Services.AddDbContext<LoseBetDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Pregătim Swagger-ul
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed trivia questions if none exist
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<LoseBetDbContext>();
        if (!db.TriviaQuestions.Any())
        {
            var items = TriviaSeeder.GetInitialQuestions();
            db.TriviaQuestions.AddRange(items);
            db.SaveChanges();
        }
    }
    catch
    {
        // ignore seeding errors in startup
    }
}

// 4. Activăm interfața grafică pentru API (doar pe calculatorul tău, în Development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 5. Activăm rutele
app.MapControllers();

app.Run();