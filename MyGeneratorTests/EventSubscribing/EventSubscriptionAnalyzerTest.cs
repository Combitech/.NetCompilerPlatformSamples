using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyGenerator.EventSubscribing;
using MyTypes;
using System.Threading.Tasks;

namespace MyGeneratorTests.EventSubscribing;

[TestClass]
public class EventSubscriptionAnalyzerTest
{
    [TestMethod]
    public async Task EventSubscriptionAnalyzer_GivesError()
    {
        var testCode = @"
using MyTypes; 

public class Subscriber 
{ 
    [EventReceiver] 
    internal void Hello(string _) 
    {
    } 
}
";
        var expected = new DiagnosticResult("MY0001", DiagnosticSeverity.Error)
            .WithSpan(4, 14, 4, 24)
            .WithArguments("Subscriber");

        var test = new CSharpAnalyzerTest<EventSubscriptionAnalyzer, DefaultVerifier>
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
