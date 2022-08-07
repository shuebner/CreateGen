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
namespace Samples.HelloWorld
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
        var expectedGeneratedCode = @"// Auto-generated code
using System;

namespace Samples.HelloWorld
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


    [Test]
    [TestCaseSource(nameof(Samples))]
    public Task Creates_a_static_method2(string code, string expectedGeneratedCode)
    {
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

    public static readonly IEnumerable<TestCaseData> Samples = Directory.GetFiles(
        Path.Combine(TestContext.CurrentContext.TestDirectory, "Samples"), "*.cs", SearchOption.AllDirectories)
        .Select(f => (FilePath: f, SampleDirectory: f.Split(Path.DirectorySeparatorChar)[^2]))
        .GroupBy(pair => pair.SampleDirectory)
        .Select(group => new TestCaseData(
            File.ReadAllText(group.Single(e => e.FilePath.EndsWith("Input.cs")).FilePath),
            File.ReadAllText(group.Last(e => e.FilePath.EndsWith("GeneratedCode.cs")).FilePath))
            .SetArgDisplayNames(group.Key));
}