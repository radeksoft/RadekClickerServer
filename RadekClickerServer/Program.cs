var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var leaderboard = new Dictionary<string, float>();

app.MapGet("/leaderboard", () =>
    leaderboard.ToList().Sort((x1, x2) => x1.Value.CompareTo(x2)));

app.MapPost("/score/{id}/{radeks}", (string id, float radeks) => leaderboard[id] = radeks);

app.MapGet("/nameused/{id}", (string id) => leaderboard.Keys.Contains(id));    