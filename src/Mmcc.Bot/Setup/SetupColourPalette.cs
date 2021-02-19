using System.Drawing;
using Microsoft.Extensions.DependencyInjection;
using Mmcc.Bot.Core.Models;

namespace Mmcc.Bot.Setup
{
    public static class SetupColourPalette
    {
        public static IServiceCollection AddTailwindColourPalette(this IServiceCollection services)
        {
            var colourPalette = new ColourPalette
            {
                Gray = ColorTranslator.FromHtml("#6B7280"),
                Red = ColorTranslator.FromHtml("#EF4444"),
                Yellow = ColorTranslator.FromHtml("#F59E0B"),
                Green = ColorTranslator.FromHtml("#10B981"),
                Blue = ColorTranslator.FromHtml("#3B82F6"),
                Indigo = ColorTranslator.FromHtml("#6366F1"),
                Purple = ColorTranslator.FromHtml("#8B5CF6"),
                Pink = ColorTranslator.FromHtml("#EC4899")
            };

            services.AddSingleton(colourPalette);

            return services;
        }
    }
}