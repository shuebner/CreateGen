using Microsoft.CodeAnalysis.Text;
using System.Text;
using VerifyCS = SvSoft.CreateGen.CSharpSourceGeneratorVerifier<SvSoft.CreateGen.CreateSourceGenerator>;

namespace SvSoft.CreateGen;

public class CreateSourceGeneratorTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public Task Creates_a_static_method()
    {
        var code = @"
#nullable enable
namespace Test
{
    partial class Program
    {
        public static void Main(string[] args)
        {
            HelloFrom(""Generated Code"");
        }

        static partial void HelloFrom(string name);
    }
}
#nullable restore
";
        var expectedGeneratedCode = @" // Auto-generated code
using System;

namespace Test
{
    partial class Program
    {
        static partial void HelloFrom(string name) =>
            Console.WriteLine($""Generator says: Hi from '{name}'"");
    }
}
";

        return new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                    (typeof(CreateSourceGenerator), "Program.g.cs", SourceText.From(expectedGeneratedCode, Encoding.UTF8, SourceHashAlgorithm.Sha1)),
                },
            },
        }.RunAsync();
    }
}