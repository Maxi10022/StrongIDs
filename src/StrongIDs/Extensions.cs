using Microsoft.CodeAnalysis;

namespace StrongIDs;

public static class Extensions
{
    public static string ToDisplayString(this Accessibility accessibility)
    {
        switch (accessibility)
        {
            case Accessibility.Private:
                return "private";
            case Accessibility.ProtectedAndInternal:
                return "protected internal";
            case Accessibility.Protected:
                return "protected";
            case Accessibility.Internal:
                return "internal";
            case Accessibility.Public:
                return "public";
            default:
                return string.Empty;
        }
    }
}