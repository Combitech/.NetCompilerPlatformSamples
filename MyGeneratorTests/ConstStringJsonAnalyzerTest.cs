using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyGenerator;
using MyTypes;
using System.Threading.Tasks;

namespace MyGeneratorTests;

[TestClass]
public class ConstStringJsonAnalyzerTest
{
    [TestMethod]
    public async Task Reports_error_for_invalid_json_in_const_string()
    {
        var testCode = @"
using MyTypes;
public class TestClass
{
    [IsJson]
    const string InvalidJson = ""{ invalid json }"";
}
";
        var expected = new DiagnosticResult("MYJSON001", DiagnosticSeverity.Error)
            .WithSpan(6, 18, 6, 50)
            .WithArguments("InvalidJson");

        var test = new CSharpAnalyzerTest<ConstStringJsonAnalyzer, DefaultVerifier>
        {
            TestCode = testCode,
            ExpectedDiagnostics = { expected }
        };
		test.TestState.AdditionalReferences.Add(typeof(IsJsonAttribute).Assembly);
		await test.RunAsync(TestContext.CancellationTokenSource.Token);
    }

    [TestMethod]
    public async Task Does_not_report_error_for_valid_json_in_const_string()
    {
        var testCode = @"
using MyTypes;
public class TestClass
{
    [IsJson]
    const string ValidJson = ""{\""key\"":\""value\""}"";
}
";
        var test = new CSharpAnalyzerTest<ConstStringJsonAnalyzer, DefaultVerifier>
        {
            TestCode = testCode
        };
		test.TestState.AdditionalReferences.Add(typeof(IsJsonAttribute).Assembly);
		await test.RunAsync(TestContext.CancellationTokenSource.Token);
    }

	[TestMethod]
	public async Task Reports_error_for_invalid_json_in_const_string_without_attribute()
	{
		var testCode = @"
public class TestClass
{
    const string InvalidJson = ""{ invalid json }"";
}
";
		var test = new CSharpAnalyzerTest<ConstStringJsonAnalyzer, DefaultVerifier>
		{
			TestCode = testCode,
		};
		await test.RunAsync(TestContext.CancellationTokenSource.Token);
	}

	public TestContext TestContext { get; set; }
}
