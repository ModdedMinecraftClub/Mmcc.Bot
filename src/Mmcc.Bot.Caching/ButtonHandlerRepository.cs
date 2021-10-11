using System;
using Microsoft.Extensions.Caching.Memory;
using Mmcc.Bot.Caching.Entities;

namespace Mmcc.Bot.Caching;

/// <summary>
/// Represents a button handler repository.
/// </summary>
public interface IButtonHandlerRepository
{
    /// <summary>
    /// Registers a new button handler from a <see cref="Button"/> object with the repository.
    /// </summary>
    /// <param name="button"><see cref="Button"/> object containing the handler.</param>
    void Register(Button button);
        
    /// <summary>
    /// Registers a button handler with the repository.
    /// </summary>
    /// <param name="buttonId">The ID the button handler corresponds to.</param>
    /// <param name="handler">The <see cref="ButtonHandler"/>.</param>
    void Register(ulong buttonId, ButtonHandler handler);
        
    /// <summary>
    /// De-registers a button handler from the repository. 
    /// </summary>
    /// <param name="buttonId">The ID of the button to deregister.</param>
    void Deregister(ulong buttonId);
        
    /// <summary>
    /// Gets a button handler for a button with a given ID or default value of <see cref="ButtonHandler"/> if not found.
    /// </summary>
    /// <param name="buttonId">The ID of the button for which to get a button handler.</param>
    /// <returns>The button handler corresponding to a button with a given ID or default value of <see cref="ButtonHandler"/> if not found.</returns>
    ButtonHandler? GetOrDefault(ulong buttonId);
}
    
/// <inheritdoc />
public class ButtonHandlerRepository : IButtonHandlerRepository
{
    private readonly IMemoryCache _cache;

    /// <summary>
    /// Sliding button handler cache expiration in minutes.
    /// </summary>
    private const int SlidingExpirationInMinutes = 5;

    /// <summary>
    /// Absolute button handler cache expiration in minutes.
    /// </summary>
    ///
    /// <remarks>15 minutes is how an interaction lasts by default in the Discord API.</remarks>
    private const int AbsoluteExpirationInMinutes = 15;

    /// <summary>
    /// Instantiates a new instance of <see cref="ButtonHandlerRepository"/>.
    /// </summary>
    /// <param name="cache">The memory cache.</param>
    public ButtonHandlerRepository(IMemoryCache cache)
    {
        _cache = cache;
    }

    /// <inheritdoc />
    public void Register(Button button) =>
        Register(button.Id.Value, button.Handler);
        
    /// <inheritdoc />
    public void Register(ulong buttonId, ButtonHandler handler) =>
        _cache.Set(buttonId, handler, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(SlidingExpirationInMinutes),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(AbsoluteExpirationInMinutes)
        });

    /// <inheritdoc />
    public void Deregister(ulong buttonId) =>
        _cache.Remove(buttonId);
        
    /// <inheritdoc />
    public ButtonHandler? GetOrDefault(ulong buttonId) =>
        _cache.Get<ButtonHandler>(buttonId);
}