using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StrongIDs;

[Generator]
public class StrongIDsSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            var tuple = Template.EntityIdInterface;
            ctx.AddSource(tuple.Name, tuple.Code);
        });

        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                (s, _) => s is RecordDeclarationSyntax,
                (ctx, _) => GetStructDeclarationForSourceGen(ctx))
            .Where(t => 
                t is { EntityIdInterfaceFound: true, DeclarationSyntax: not null })
            .Select((t, _) => t.DeclarationSyntax);
        
        context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()), Execute);
    }

    private void Execute(
        SourceProductionContext context, 
        (Compilation Left, ImmutableArray<RecordDeclarationSyntax> Right) tuple)
    {
        var (compilation, recordDeclarations) = tuple;
        foreach (var recordDeclarationSyntax in recordDeclarations)
        {
            if (compilation
                    .GetSemanticModel(recordDeclarationSyntax.SyntaxTree)
                    .GetDeclaredSymbol(recordDeclarationSyntax) is not INamedTypeSymbol symbol) 
                continue;
            
            var globalNamespace = compilation.GlobalNamespace.ToDisplayString();
            var recordNamespace = symbol.ContainingNamespace.ToDisplayString();
            recordNamespace = 
                globalNamespace == recordNamespace ? 
                    string.Empty : recordNamespace;

            var accessibility = symbol.DeclaredAccessibility;
            var recordName = recordDeclarationSyntax.Identifier.Text;

            var sourceCode = Template.Identifier(recordNamespace, recordName, accessibility);
            context.AddSource($"{recordName}.g.cs", sourceCode);
        }
    }

    private static (RecordDeclarationSyntax DeclarationSyntax, bool EntityIdInterfaceFound) GetStructDeclarationForSourceGen(
        GeneratorSyntaxContext context)
    {
        var recordDeclarationSyntax = (RecordDeclarationSyntax)context.Node;
        var notFound = (structDeclarationSyntax: recordDeclarationSyntax, false);
        if (recordDeclarationSyntax.BaseList is null)
            return notFound;
        
        var symbol = context.SemanticModel
            .GetDeclaredSymbol(recordDeclarationSyntax) as INamedTypeSymbol;
        
        if (symbol is null || symbol.IsAbstract)
            return notFound;
        
        // TODO raise DiagnosticDescriptor error
        if (!symbol.IsReadOnly)
            return notFound;
        
        var implementsEntityIdInterface = symbol.Interfaces
            .Any(x => x.IsAbstract && x.Name.Contains(StrongIDs.MarkerName));
        
        return implementsEntityIdInterface ? 
            (structDeclarationSyntax: recordDeclarationSyntax, true) : 
            notFound;
    }
}