using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace SvSoft.CreateGen;

[Generator]
public class CreateSourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        // Find the main method
        INamedTypeSymbol? programType = context.Compilation.GetTypeByMetadataName("Samples.HelloWorld.Program");
        if (programType is null)
        {
            return;
        }

        var helloFromMethodCandidates = programType.GetMembers().OfType<IMethodSymbol>()
            .Where(m => m.Name.Equals("HelloFrom", StringComparison.Ordinal));

        IMethodSymbol? helloFromMethod = helloFromMethodCandidates.Where(m => !m.IsGenericMethod && m.Parameters.Length == 1 && m.Parameters[0].Type.Equals(context.Compilation.GetSpecialType(SpecialType.System_String).WithNullableAnnotation(NullableAnnotation.NotAnnotated), SymbolEqualityComparer.IncludeNullability))
            .SingleOrDefault();

        if (helloFromMethod is null)
        {
            return;
        }

        // Build up the source code
        string source = $@"// Auto-generated code
using System;

namespace {programType.ContainingNamespace.ToDisplayString()}
{{
    partial class {programType.Name}
    {{
        static partial void HelloFrom(string name) =>
            Console.WriteLine($""Generator says: Hi from '{{name}}'"");
    }}
}}
";
        var typeName = programType.Name;

        // Add the source code to the compilation
        context.AddSource($"{typeName}.g.cs", source);
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization required for this one
    }
}
