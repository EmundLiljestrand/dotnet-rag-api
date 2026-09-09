using System.Net.Http.Headers;
using Anthropic;
using RagApi.Models;
using RagApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var ragSection = builder.Configuration.GetSection(RagOptions.SectionName);
builder.Services.Configure<RagOptions>(ragSection);
var ragOptions = ragSection.Get<RagOptions>() ?? new RagOptions();

// Anything registered here can be requested further down by naming it as a parameter.
builder.Services.AddSingleton(new TextChunker(ragOptions.ChunkSize));
builder.Services.AddSingleton<IChunkStore, InMemoryChunkStore>();

// Voyage when a key is configured, otherwise the local stand-in.
if (string.Equals(ragOptions.EmbeddingProvider, "voyage", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddHttpClient<IEmbeddingClient, VoyageEmbeddingClient>(http =>
    {
        var apiKey = Environment.GetEnvironmentVariable("VOYAGE_API_KEY")
            ?? throw new InvalidOperationException(
                "VOYAGE_API_KEY is not set. Set the environment variable, or set "
                + "Rag:EmbeddingProvider to \"local\" to run without a key.");

        http.BaseAddress = new Uri("https://api.voyageai.com/");
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    });
}
else
{
    builder.Services.AddSingleton<IEmbeddingClient, HashingEmbeddingClient>();
}

// Claude when a key is configured, otherwise the local stand-in.
if (string.Equals(ragOptions.AnswerProvider, "claude", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton(_ => new AnthropicClient
    {
        ApiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
            ?? throw new InvalidOperationException(
                "ANTHROPIC_API_KEY is not set. Set the environment variable, or set "
                + "Rag:AnswerProvider to \"echo\" to run without a key."),
    });
    builder.Services.AddSingleton<IAnswerGenerator, ClaudeAnswerGenerator>();
}
else
{
    builder.Services.AddSingleton<IAnswerGenerator, EchoAnswerGenerator>();
}

builder.Services.AddScoped<RagService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => "OK");

app.MapPost("/index", async (IndexRequest request, RagService rag, CancellationToken ct) =>
    await rag.IndexAsync(request, ct));

app.MapPost("/query", async (QueryRequest request, RagService rag, CancellationToken ct) =>
    await rag.QueryAsync(request, ct));

app.Run();
