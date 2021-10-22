using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Mmcc.Bot.Mojang.Errors;
using Mmcc.Bot.Mojang.Models;
using Remora.Results;

namespace Mmcc.Bot.Mojang;

/// <summary>
/// Service for communicating with the Mojang API.
/// </summary>
public interface IMojangApiService
{
    /// <summary>
    /// Gets a UUID based on an IGN. Optionally gets a UUID based on an IGN on a given date.
    /// </summary>
    /// <param name="username">Username (IGN).</param>
    /// <param name="date">Date in Unix timestamp format without milliseconds. (Optional).</param>
    /// <returns>UUID.</returns>
    Task<Result<IPlayerUuidInfo?>> GetPlayerUuidInfo(string username, long? date = null);
        
    /// <summary>
    /// Gets username history for a given player.
    /// </summary>
    /// <param name="uuid">UUID of the player.</param>
    /// <returns>Name history.</returns>
    Task<Result<IEnumerable<IPlayerNameInfo>>> GetNameHistory(string uuid);
}
    
/// <inheritdoc />
public class MojangApiService : IMojangApiService
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _serializerOptions;
        
    /// <summary>
    /// Instantiates a new instance of the <see cref="MojangApiService"/> class.
    /// </summary>
    /// <param name="client">HTTP client.</param>
    public MojangApiService(HttpClient client)
    {
        client.BaseAddress = new Uri("https://api.mojang.com/");
        _client = client;

        var serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        _serializerOptions = serializerOptions;
    }
        
    /// <inheritdoc />
    public async Task<Result<IPlayerUuidInfo?>> GetPlayerUuidInfo(string username, long? date = null)
    {
        var uri = date is null
            ? $"/users/profiles/minecraft/{username}"
            : $"/users/profiles/minecraft/{username}?at={date}";
        var response = await _client.GetAsync(uri);
        if (response.StatusCode is HttpStatusCode.OK)
        {
            await using var responseStream = await response.Content.ReadAsStreamAsync();
            var res = await JsonSerializer.DeserializeAsync<PlayerUuidInfo?>(responseStream, _serializerOptions);
            return Result<IPlayerUuidInfo?>.FromSuccess(res);
        }

        if (response.StatusCode is HttpStatusCode.NoContent)
        {
            return new NotFoundError("Could not find a player with that username.");
        }

        if (response.StatusCode is HttpStatusCode.BadRequest)
        {
            await using var responseStream = await response.Content.ReadAsStreamAsync();
            var res = await JsonSerializer.DeserializeAsync<ErrorResponse?>(responseStream, _serializerOptions);
            return res is null
                ? new MojangApiError($"API error: {response.StatusCode.ToString()}")
                : new MojangApiError($"{res.Error}; {res.ErrorMessage}");
        }

        return new MojangApiError($"API error: {response.StatusCode.ToString()}");
    }
        
    /// <inheritdoc />
    public async Task<Result<IEnumerable<IPlayerNameInfo>>> GetNameHistory(string uuid)
    {
        var uri = $"/user/profiles/{uuid}/names";
        var response = await _client.GetAsync(uri);

        if (!response.IsSuccessStatusCode)
        {
            return new MojangApiError($"API error: {response.StatusCode.ToString()}");
        }
            
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var res = await JsonSerializer.DeserializeAsync<IEnumerable<PlayerNameInfo>>(responseStream, _serializerOptions);
        return Result<IEnumerable<IPlayerNameInfo>>.FromSuccess(res ?? Enumerable.Empty<IPlayerNameInfo>());
    }
}