using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using VerifyCS = SvSoft.CreateGen.CSharpSourceGeneratorVerifier<SvSoft.CreateGen.CreateSourceGenerator>;

namespace SvSoft.CreateGen;

public class CreateSourceGeneratorTests
{
    [Test]
    [TestCaseSource(nameof(Samples))]
    public Task Creates_a_static_method2(string code, string expectedGeneratedCode)
    {
        return new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                AdditionalReferences = { MetadataReference.CreateFromFile(typeof(CreateAttribute).Assembly.Location) },
                GeneratedSources =
                {
                    (typeof(CreateSourceGenerator), "Foo.g.cs", SourceText.From(expectedGeneratedCode, Encoding.UTF8, SourceHashAlgorithm.Sha1)),
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