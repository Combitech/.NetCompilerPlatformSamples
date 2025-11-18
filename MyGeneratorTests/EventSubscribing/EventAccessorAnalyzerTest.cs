using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyGenerator.EventSubscribing;
using MyTypes;
using System.Threading.Tasks;

namespace MyGeneratorTests.EventSubscribing;

[TestClass]
public class EventAccessorAnalyzerTest
{
	[TestMethod]
	public async Task Triggers_error_when_condition_met()
	{
		var testCode = @"
using MyTypes; 

public class Subscriber 
{ 
	[EventReceiver] 
	public void Hello(string _) 
	{
	} 
}
";
		var expected = new DiagnosticResult("MY0002", DiagnosticSeverity.Error).WithSpan(7, 14, 7, 19).WithArguments("Hello");

		var test = new CSharpAnalyzerTest<EventAccessorAnalyzer, DefaultVerifier>
		{
			TestCode = testCode,
			ExpectedDiagnostics = { expected }
		};
		test.TestState.AdditionalReferences.Add(typeof(EventReceiverAttribute).Assembly);
		await test.RunAsync(TestContext.CancellationTokenSource.Token);
	}

	[TestMethod]
	public async Task Does_not_trigger_error_by_default()
	{
		var test = new CSharpAnalyzerTest<EventAccessorAnalyzer, DefaultVerifier>
		{
			TestCode = "",
		};
		await test.RunAsync(TestContext.CancellationTokenSource.Token);
	}

	public TestContext TestContext { get; set; }
}
