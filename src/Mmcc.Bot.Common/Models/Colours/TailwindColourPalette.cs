using System.Drawing;

namespace Mmcc.Bot.Common.Models.Colours
{
    /// <inheritdoc />
    public class TailwindColourPalette : IColourPalette
    {
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