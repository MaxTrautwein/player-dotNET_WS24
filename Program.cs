using System.Text.Json;
using Newtonsoft.Json;
using PlayerDotNet.logic;
using PlayerDotNet.models;

var app = WebApplication.Create(args);



app.Urls.Add("http://*:3000");

app.MapPost("/",
    (JsonElement gameState) => JsonConvert.SerializeObject(Strategy.Decide(JsonConvert.DeserializeObject<GameState>(gameState.GetRawText()))));

app.MapGet("/", () => "Player C#/.net");

app.Run();