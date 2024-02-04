using Microsoft.CodeAnalysis;

namespace StrongIDs;

internal static class Template
{
    public static (string Name, string Code) EntityIdInterface =>(
        StrongIDs.MarkerName, 
        $$"""
          namespace {{StrongIDs.Namespace}};
          public interface {{StrongIDs.MarkerName}}<out T> where T : struct;
          """);
    public static string Identifier(string identifierNamespace, string name, Accessibility accessibility)
    {
        return $$"""
                 using StrongIDs;
                 {{(!string.IsNullOrEmpty(identifierNamespace)
                     ? $"namespace {identifierNamespace};" : string.Empty)}}
                 {{accessibility.ToDisplayString()}} partial record struct {{name}} : IEntityId<{{name}}>
                 {
                     public Guid Value { get; }
                 
                     public bool HasValue => Value != Guid.Empty;
                     
                     private {{name}}(Guid value) => Value = value;
                     
                     public static {{name}} Empty => new {{name}}(Guid.Empty);
                     
                     public static {{name}} New() => new {{name}}(Guid.NewGuid());
                 
                     public static {{name}} Parse(string? value) =>
                         Guid.TryParse(value, out Guid id) ? new {{name}}(id) : Empty;
                 }
                 """;
    }
}