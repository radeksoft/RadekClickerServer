using HashidsNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RadekClickerServer;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<PlayerDb>(c => c.UseSqlite("Data Source=db/radek.db"));

var logger = new LoggerConfiguration()
    .WriteTo.Console(builder.Environment.IsDevelopment() ? LogEventLevel.Information : LogEventLevel.Warning)
    .WriteTo.File("logs/log.txt", LogEventLevel.Warning)
    .CreateLogger();
Log.Logger = logger;
builder.Logging.ClearProviders().AddSerilog(logger);

var app = builder.Build();

// random hash idk
var hashids = new Hashids(app.Configuration.GetValue("hashSalt", ""), 6, "ABCDEFGHIJKLMNOPQRSTUVWXYZ");

app.MapGet("/leaderboard", ([FromServices]PlayerDb db) =>
    db.Players.OrderByDescending(x => x.Radeks));

app.MapGet("/gethash/{secret}/{name}", ([FromServices]IConfiguration cf, [FromServices]PlayerDb db, string secret, string name) => 
{
    if (secret != cf.GetValue("adminSecret", ""))
        return Results.Unauthorized();

    var player = db.Players.FirstOrDefault(x => x.DisplayName == name);

    if (player == null)
        return Results.NotFound();

    return Results.Ok(hashids.Encode(player.Id));
});
    

app.MapPut("/newplayer/{secret}/{name}", async ([FromServices]PlayerDb db, [FromServices]IConfiguration cf, string secret, string name) =>
{
    // you don't have to comment on this :)
    if (secret != cf.GetValue("adminSecret", ""))
        return Results.Unauthorized();

    if (db.Players.Select(x => x.DisplayName).Contains(name))
        return Results.Conflict();

    var player = new Player
    {
        DisplayName = name,
        Radeks = 0,
    };
    
    db.Players.Add(player);
    await db.SaveChangesAsync();

    return Results.Ok(hashids.Encode(player.Id));
});

app.MapPut("/removeplayer/{secret}/{name}", async ([FromServices] PlayerDb db, [FromServices]IConfiguration cf, string secret, string name) =>
{
    var player = db.Players.SingleOrDefault(x => x.DisplayName == name);

    if (secret != cf.GetValue("adminSecret", ""))
        return Results.Unauthorized();

    if (player == null)
        return Results.NotFound();

    db.Players.Remove(player);
    await db.SaveChangesAsync();

    return Results.Ok();
});

app.MapPut("/updateradeks/{token}/{radeks}", async ([FromServices] PlayerDb db, string token, float radeks) =>
{
    try
    {
        var player = db.Players.SingleOrDefault(x => x.Id == hashids.DecodeSingle(token.ToUpper()));
        
        if (player == null)
            return Results.Unauthorized();

        player.Radeks = radeks;
        await db.SaveChangesAsync();

        return Results.Ok(db.Players.OrderByDescending(x => x.Radeks));
    }
    catch (InvalidOperationException)
    {
        return Results.Unauthorized();
    }
});

app.Run();
