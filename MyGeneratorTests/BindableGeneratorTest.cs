using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using MyGenerator;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using MyTypes;

namespace MyGeneratorTests;

[TestClass]
public class BindableGeneratorTest
{
	[TestMethod]
	public async Task GeneratesPropertyForFieldWithAttribute()
	{
		const string className = "Target";
		const string fieldName = "_myBindableString";
		const string fieldType = "string";
		const string containingNamespace = "MyNamespace";
		var propertyName = BindableGenerator.GeneratePropertyName(fieldName);
		await new CSharpSourceGeneratorTest<BindableGenerator, DefaultVerifier>
		{
			TestState =
			{
				AdditionalReferences = { typeof(BindableProperty).Assembly },
				Sources =
				{
					$$"""
					    using MyTypes;
						namespace {{containingNamespace}} {
							public partial class {{className}} 
							{
								[{{nameof(BindableProperty)}}]
								private {{fieldType}} {{fieldName}};
							}
						}
					""",
				},
				GeneratedSources =
				{
					($"MyGenerator\\MyGenerator.BindableGenerator\\{className}_{propertyName}_Generated.cs", SourceText.From(BindableGenerator.GeneratePropertyCode(containingNamespace, className, fieldName, propertyName, fieldType), Encoding.UTF8)),
				},
			},
		}.RunAsync(TestContext.CancellationTokenSource.Token);
	}

	public TestContext TestContext { get; set; }
}
