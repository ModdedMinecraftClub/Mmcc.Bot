using System.Drawing;

namespace Mmcc.Bot.Common.Models.Colours
{
    /// <summary>
    /// Colour palette for embeds;
    /// </summary>
    public interface IColourPalette
    {
        public Color Gray { get; }
        public Color Red { get; }
        public Color Yellow { get; }
        public Color Green { get; }
        public Color Blue { get; }
        public Color Indigo { get; }
        public Color Purple { get; }
        public Color Pink { get; }
    }
}