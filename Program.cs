using System.Net.Http.Json; // Necesario para usar HttpClient con JSON
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Configura Kestrel para que escuche en todas las interfaces de red en el puerto 80
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(80);
});

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configura CORS para permitir cualquier origen
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Aplica la política CORS
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Base URL de tu API Flask

// Obtener la URL de la API Flask desde la variable de entorno
var flaskApiUrl = Environment.GetEnvironmentVariable("FLASK_API_URL") ?? "http://default-url-if-env-var-is-not-set"; // Cambia <flask-api-host> por la dirección IP o dominio de tu API Flask

// Endpoint para obtener la lista de posts desde la API Flask
app.MapGet("/posts", async () =>
{
    using var client = new HttpClient();
    var response = await client.GetFromJsonAsync<List<Post>>($"{flaskApiUrl}/posts");

    if (response == null)
    {
        return Results.Problem("Error retrieving posts from the Flask API.");
    }

    return Results.Ok(response);
})
.WithName("GetPosts")
.WithOpenApi();

// Endpoint para obtener recomendaciones desde la API Flask
app.MapGet("/recommendations/{rowNum:int}", async (int rowNum) =>
{
    using var client = new HttpClient();
    var response = await client.GetAsync($"{flaskApiUrl}/recommendations/{rowNum}");

    if (!response.IsSuccessStatusCode)
    {
        return Results.Problem($"Error retrieving recommendations for row number {rowNum} from the Flask API.");
    }

    var recommendations = await response.Content.ReadFromJsonAsync<List<string>>();
    return Results.Ok(recommendations);
})
.WithName("GetRecommendations")
.WithOpenApi();

// Endpoint para obtener los posts con mayores votos desde la API Flask
app.MapGet("/top-posts", async () =>
{
    using var client = new HttpClient();
    var response = await client.GetFromJsonAsync<List<Post>>($"{flaskApiUrl}/posts");

    if (response == null)
    {
        return Results.Problem("Error retrieving posts from the Flask API.");
    }

    // Filtrar los posts con el mayor número de votos
    var topPosts = response.Where(post => post.Votes > 0)
                           .OrderByDescending(post => post.Votes)
                           .ToList();

    return Results.Ok(topPosts);
})
.WithName("GetTopPosts")
.WithOpenApi();

app.Run();

// Definición de la clase Post
record Post
{
    public int Id { get; init; }
    public string Title { get; init; }
    public string Content { get; init; }
    public string Tags { get; init; }
    public string Link { get; init; }
    public int Votes { get; init; }
}

// Definición de la clase Recommendation
record Recommendation
{
    public string Title { get; init; }
    public string Score { get; init; }
}
