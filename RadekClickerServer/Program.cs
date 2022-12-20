using HashidsNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RadekClickerServer;

// i know ;-;
const string ADMIN_SECRET = "5976d3b4f49f6ae9d20edef64dfbbcee34452d3e";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<PlayerDb>(c => c.UseSqlite("Data Source=db/radek.db"));

var app = builder.Build();

// random hash idk
var hashids = new Hashids("78b28ded1c9d7afd170edb1", 6, "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890");

app.MapGet("/leaderboard", ([FromServices]PlayerDb db) =>
    db.Players.OrderByDescending(x => x.Radeks));

app.MapGet("/gethash/{secret}/{id:int}", (string secret, int id) => 
    secret != ADMIN_SECRET ? Results.Unauthorized() : Results.Ok(hashids.Encode(id)));

app.MapPut("/newplayer/{secret}/{name}", async ([FromServices]PlayerDb db, string secret, string name) =>
{
    // you don't have to comment on this :)
    if (secret != ADMIN_SECRET)
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

app.MapPut("/removeplayer/{secret}/{id:int}", async ([FromServices] PlayerDb db, string secret, int id) =>
{
    var player = db.Players.SingleOrDefault(x => x.Id == id);
    if (player == null)
        return Results.NotFound();

    db.Players.Remove(player);
    await db.SaveChangesAsync();

    return Results.Ok();
});

app.MapPut("/updateradeks/{token}/{radeks}", async ([FromServices] PlayerDb db, string token, float radeks) =>
{
    var player = db.Players.SingleOrDefault(x => x.Id == hashids.DecodeSingle(token.ToUpper()));
    if (player == null)
        return Results.Unauthorized();

    player.Radeks = radeks;
    await db.SaveChangesAsync();

    return Results.Ok();
});

app.Run();
