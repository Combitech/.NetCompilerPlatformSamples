using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using MyTypes;
using System.Collections.Immutable;
using System.Linq;

namespace MyGenerator.EventSubscribing;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EventReceiverInvocationAnalyzer : DiagnosticAnalyzer
{
	private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
	id: "MY0003",
	title: $"Invocation of method with [{nameof(EventReceiverAttribute)}] must be from the EventAggregator using Reflection",
	messageFormat: $"Method '{{0}}' with [{nameof(EventReceiverAttribute)}] is invoked from '{{1}}', which is not allowed",
	category: "Usage",
	DiagnosticSeverity.Error,
	isEnabledByDefault: true
	);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
	}

	private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
	{
		var invocation = (InvocationExpressionSyntax)context.Node;
		var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
		var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
		if (methodSymbol == null)
			return;

		if (!EventAttributeUtils.HasEventReceiverAttribute(methodSymbol))
			return;

		var diagnostic = Diagnostic.Create(
			Rule,
			invocation.GetLocation(),
			methodSymbol.Name
		);
		context.ReportDiagnostic(diagnostic);
	}
}
