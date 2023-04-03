using System.Drawing;

namespace Mmcc.Bot.Common.Models.Colours;

// TODO: Remove all usages of IColourPalette;
public static class ColourPalette
{
    public static Color Black => ColorTranslator.FromHtml("#262626");
    public static Color Gray => ColorTranslator.FromHtml("#6B7280");
    public static Color Red => ColorTranslator.FromHtml("#EF4444");
    public static Color Yellow => ColorTranslator.FromHtml("#F59E0B");
    public static Color Green => ColorTranslator.FromHtml("#10B981");
    public static Color Blue => ColorTranslator.FromHtml("#3B82F6");
    public static Color Indigo => ColorTranslator.FromHtml("#6366F1");
    public static Color Purple => ColorTranslator.FromHtml("#8B5CF6");
    public static Color Pink => ColorTranslator.FromHtml("#EC4899");
}