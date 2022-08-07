using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace SvSoft.CreateGen;

[Generator]
public class CreateSourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is AttributeSyntaxReceiver { AttributeCandidates: var attributeCandidates })
        {
            INamedTypeSymbol createAttributeType = context.Compilation.GetTypeByMetadataName("SvSoft.CreateGen.CreateAttribute")
                ?? throw new ArgumentException("Did not find Create attribute type. This is a bug.");

            foreach (AttributeSyntax attribute in attributeCandidates)
            {
                SemanticModel semanticModel = context.Compilation.GetSemanticModel(attribute.SyntaxTree);
                TypeInfo typeInfo = semanticModel.GetTypeInfo(attribute);
                if (typeInfo.Type is INamedTypeSymbol type && type.Equals(context.Compilation.GetTypeByMetadataName("SvSoft.CreateGen.CreateAttribute"), SymbolEqualityComparer.Default))
                {
                    Generate(attribute);
                }
            }
        }

        void Generate(AttributeSyntax attribute)
        {
            if (attribute.Parent?.Parent is ClassDeclarationSyntax classDeclaration)
            {
                SemanticModel semanticModel = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                if (semanticModel.GetDeclaredSymbol(classDeclaration) is INamedTypeSymbol typeSymbol)
                {
                    if (classDeclaration.Parent is NamespaceDeclarationSyntax namespaceDeclaration)
                    {
                        string className = classDeclaration.Identifier.Text;
                        StringBuilder sb = new();
                        string generatedCode = $@"// Auto-generated code
namespace {namespaceDeclaration.Name.GetText()}{{
    partial class {className}
    {{
        {string.Join(Environment.NewLine, typeSymbol.Constructors.Select(c => $"        {GetCreateMethod(c)}"))}
    }}
}}
";

                        var syntaxTree = CSharpSyntaxTree.ParseText(generatedCode);
                        var formattedText = syntaxTree.GetRoot().NormalizeWhitespace().GetText(Encoding.UTF8);

                        context.AddSource($"{className}.g.cs", formattedText);

                        string GetCreateMethod(IMethodSymbol ctor)
                        {
                            return @$"{SyntaxFacts.GetText(ctor.DeclaredAccessibility)} static {className} Create()
{{
    return new {className}();
}}";
                        }
                    }
                }
            }
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new AttributeSyntaxReceiver());
    }

    sealed class AttributeSyntaxReceiver : ISyntaxReceiver
    {
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is AttributeSyntax attribute)
            {
                if (attribute.Name is QualifiedNameSyntax { Right.Identifier.Text: "Create" }
                                   or SimpleNameSyntax { Identifier.Text: "Create" })
                {
                    AttributeCandidates.Add(attribute);
                }
            }
        }

        public List<AttributeSyntax> AttributeCandidates { get; } = new();
    }
}
