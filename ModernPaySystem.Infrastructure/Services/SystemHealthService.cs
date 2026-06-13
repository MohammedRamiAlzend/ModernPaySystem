using System.IO;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModernPaySystem.Infrastructure.Options;
using ModernPaySystem.Infrastructure.Persistence;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using SemanticSearchLib.Models;

namespace ModernPaySystem.Infrastructure.Services;

public class SystemHealthService
{
    private readonly string _ollamaBaseUrl;
    private readonly string _qdrantHost;
    private readonly int _qdrantPort;
    private readonly string _qdrantApiKey;
    private readonly bool _qdrantUseTls;
    private readonly ILogger<SystemHealthService> _logger;
    private readonly string _errorLogPath;

    public bool IsOllamaHealthy { get; private set; }
    public bool IsQdrantHealthy { get; private set; }

    public SystemHealthService(
        IOptions<QdrantOptions> qdrantOptions,
        IOptions<EmbeddingOptions> embeddingOptions,
        ILogger<SystemHealthService> logger)
    {
        _ollamaBaseUrl = embeddingOptions.Value.BaseUrl.TrimEnd('/');
        _qdrantHost = qdrantOptions.Value.Host;
        _qdrantPort = qdrantOptions.Value.Port;
        _qdrantApiKey = qdrantOptions.Value.ApiKey;
        _qdrantUseTls = qdrantOptions.Value.UseTls;
        _logger = logger;
        var healthDir = Path.Combine(Directory.GetCurrentDirectory(), "helaths");
        Directory.CreateDirectory(healthDir);
        _errorLogPath = Path.Combine(healthDir, $"server-errors-{DateTime.UtcNow:yyyy-MM-dd}.txt");
    }

    public async Task CheckAsync(AppDbContext dbContext)
    {
        var errors = new List<string>();

        await CheckDatabaseAsync(dbContext, errors);
        await CheckOllamaAsync(errors);
        await CheckQdrantAsync(errors);

        if (errors.Count > 0)
        {
            await File.AppendAllLinesAsync(_errorLogPath, errors);
        }
    }

    private async Task CheckDatabaseAsync(AppDbContext dbContext, List<string> errors)
    {
        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync();
            if (!canConnect)
                throw new InvalidOperationException("Database returned false from CanConnectAsync");

            _logger.LogInformation("Database connection OK");
        }
        catch (Exception ex)
        {
            errors.Add($"[{DateTime.UtcNow:O}] DATABASE: {ex.Message}");
            _logger.LogCritical(ex, "Database connection failed at startup");
            await File.AppendAllLinesAsync(_errorLogPath, errors);
            throw;
        }
    }

    private async Task CheckOllamaAsync(List<string> errors)
    {
        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await httpClient.GetAsync($"{_ollamaBaseUrl}/api/tags");
            IsOllamaHealthy = response.IsSuccessStatusCode;

            if (IsOllamaHealthy)
                _logger.LogInformation("Ollama server OK at {Url}", _ollamaBaseUrl);
            else
                _logger.LogWarning("Ollama server at {Url} returned status {Status}", _ollamaBaseUrl, response.StatusCode);
        }
        catch (Exception ex)
        {
            IsOllamaHealthy = false;
            errors.Add($"[{DateTime.UtcNow:O}] OLLAMA: {ex.Message}");
            _logger.LogWarning(ex, "Ollama server is not reachable at {OllamaUrl}", _ollamaBaseUrl);
        }
    }

    private async Task CheckQdrantAsync(List<string> errors)
    {
        try
        {
            QdrantClient client;

            if (!string.IsNullOrEmpty(_qdrantApiKey))
            {
                var channel = QdrantChannel.ForAddress(
                    $"{( _qdrantUseTls ? "https" : "http" )}://{_qdrantHost}:{_qdrantPort}",
                    new ClientConfiguration
                    {
                        ApiKey = _qdrantApiKey,
                        CertificateThumbprint = null
                    });
                var grpcClient = new QdrantGrpcClient(channel);
                client = new QdrantClient(grpcClient);
            }
            else if (_qdrantUseTls)
            {
                client = new QdrantClient($"https://{_qdrantHost}:{_qdrantPort}");
            }
            else
            {
                client = new QdrantClient(_qdrantHost, _qdrantPort);
            }

            await client.ListCollectionsAsync();
            IsQdrantHealthy = true;
            _logger.LogInformation("Qdrant server OK at {Host}:{Port}", _qdrantHost, _qdrantPort);
        }
        catch (Exception ex)
        {
            IsQdrantHealthy = false;
            errors.Add($"[{DateTime.UtcNow:O}] QDRANT: {ex.Message}");
            _logger.LogWarning(ex, "Qdrant server is not reachable at {Host}:{Port}", _qdrantHost, _qdrantPort);
        }
    }
}
