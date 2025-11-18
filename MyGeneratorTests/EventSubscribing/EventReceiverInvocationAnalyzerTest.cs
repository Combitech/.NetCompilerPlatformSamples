using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyGenerator.EventSubscribing;
using MyTypes;
using System.Threading.Tasks;

namespace MyGeneratorTests.EventSubscribing;

[TestClass]
public class EventReceiverInvocationAnalyzerTest
{
	[TestMethod]
	public async Task Triggers_error_when_condition_met()
	{
		var testCode = @"
using MyTypes; 

public class Subscriber 
{ 
	private void Caller()
	{
		Hello(null);
	}

	[EventReceiver] 
	internal void Hello(string _) 
	{
	} 
}
";
		var expected = new DiagnosticResult("MY0003", DiagnosticSeverity.Error).WithSpan(8, 3, 8, 14).WithArguments("Hello");
		var test = new CSharpAnalyzerTest<EventReceiverInvocationAnalyzer, DefaultVerifier>
		{
			TestCode = testCode,
			ExpectedDiagnostics = { expected }
		};
		test.TestState.AdditionalReferences.Add(typeof(EventReceiverAttribute).Assembly);
		await test.RunAsync(TestContext.CancellationTokenSource.Token);
	}

	[TestMethod]
	public async Task Does_not_trigger_when_method_referenced()
	{
		var testCode = $@"
using MyTypes; 
using System;
internal static class SubscriberRefer
{{
	internal static void Referenced()
	{{
		var sub = new Subscriber();
		Action<string> a = sub.Hello;
	}}
}}

public class Subscriber 
{{ 
	[EventReceiver] 
	internal void Hello(string _) 
	{{
	}} 
}}
";
		var test = new CSharpAnalyzerTest<EventReceiverInvocationAnalyzer, DefaultVerifier>
		{
			TestCode = testCode,
		};
		test.TestState.AdditionalReferences.Add(typeof(EventReceiverAttribute).Assembly);
		await test.RunAsync(TestContext.CancellationTokenSource.Token);
	}

	[TestMethod]
	public async Task Does_not_trigger_error_by_default()
	{
		var test = new CSharpAnalyzerTest<EventReceiverInvocationAnalyzer, DefaultVerifier>
		{
			TestCode = "",
		};
		await test.RunAsync(TestContext.CancellationTokenSource.Token);
	}

	public TestContext TestContext { get; set; }
}
