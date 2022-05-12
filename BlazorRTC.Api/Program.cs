using BlazorRTC.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddCors(option => option.AddDefaultPolicy(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().Build()));
builder.Services.AddResponseCompression(opts => opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" }));
builder.Services.AddSingleton<AppStateManager>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseCors();


var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
       new WeatherForecast
       (
           DateTime.Now.AddDays(index),
           Random.Shared.Next(-20, 55),
           summaries[Random.Shared.Next(summaries.Length)]
       ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/users", ([FromServices] AppStateManager appStateManager) => appStateManager.GetUsers());
app.MapGet("/meetings", ([FromServices] AppStateManager appStateManager) => appStateManager.GetMeetings());
app.MapGet("/offers/{id}", ([FromServices] AppStateManager appStateManager, [FromRouteAttribute] string id) => appStateManager.GetOffer(id));
app.MapGet("/candidates/{id}", ([FromServices] AppStateManager appStateManager, [FromRouteAttribute] string id) => appStateManager.GetCandidates(id));

app.MapPost("/offers/{id}", ([FromServices] AppStateManager appStateManager, [FromRoute] string id, [FromBody] object offer) => appStateManager.StartCall(id, offer));


app.MapHub<ChatHub>("/blazorrtc");

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}