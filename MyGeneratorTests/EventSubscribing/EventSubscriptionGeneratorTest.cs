using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using MyTypes;
using MyGenerator.EventSubscribing;

namespace MyGeneratorTests.EventSubscribing;

[TestClass]
public class EventSubscriptionGeneratorTest
{
	[TestMethod]
	public async Task GeneratesSubscribeMethodForValidClass()
	{
		const string className = "Target";
		const string containingNamespace = "MyNamespace";
		const string methodName = "HandlerForEvent";
		const string typeName = "MyEventType";
		var methodList = new List<EventSubscriptionGenerator.NameType>() 
		{ 
			new EventSubscriptionGenerator.NameType() 
			{ 
				Name = methodName, 
				Type = $"global::{containingNamespace}.{typeName}" 
			} 
		};
		await new CSharpSourceGeneratorTest<EventSubscriptionGenerator, DefaultVerifier>
		{
			TestState =
			{
				AdditionalReferences = { typeof(BindableProperty).Assembly },
				Sources =
				{
					$$"""
					    using MyTypes;
						namespace {{containingNamespace}} {
							public class {{typeName}} {}
							public partial class {{className}} 
							{
								[{{nameof(EventReceiverAttribute)}}]
								internal void {{methodName}}({{typeName}} eventData) {}
							}
						}
					""",
				},
				GeneratedSources =
				{
					($"MyGenerator\\MyGenerator.EventSubscribing.EventSubscriptionGenerator\\{className}_EventSubscription.g.cs", SourceText.From(EventSubscriptionGenerator.GeneratePropertyCode(className, containingNamespace, methodList), Encoding.UTF8)),
				},
			},
		}.RunAsync(TestContext.CancellationTokenSource.Token);
	}

	public TestContext TestContext { get; set; }
}
