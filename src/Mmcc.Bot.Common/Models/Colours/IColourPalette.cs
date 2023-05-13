using System.Drawing;

namespace Mmcc.Bot.Common.Models.Colours
{
    // TODO: REMOVE THIS CLASS;
    
    /// <summary>
    /// Colour palette for embeds;
    /// </summary>
    public interface IColourPalette
    {
        public Color Black { get; }
        public Color Gray { get; }
        public Color Red { get; }
        public Color Yellow { get; }
        public Color Green { get; }
        public Color Blue { get; }
        public Color Indigo { get; }
        public Color Purple { get; }
        public Color Pink { get; }
    }
    
    public class TailwindColourPalette : IColourPalette
    {
        public Color Black => ColorTranslator.FromHtml("#262626");
        public Color Gray => ColorTranslator.FromHtml("#6B7280");
        public Color Red => ColorTranslator.FromHtml("#EF4444");
        public Color Yellow => ColorTranslator.FromHtml("#F59E0B");
        public Color Green => ColorTranslator.FromHtml("#10B981");
        public Color Blue => ColorTranslator.FromHtml("#3B82F6");
        public Color Indigo => ColorTranslator.FromHtml("#6366F1");
        public Color Purple => ColorTranslator.FromHtml("#8B5CF6");
        public Color Pink => ColorTranslator.FromHtml("#EC4899");
    }
}