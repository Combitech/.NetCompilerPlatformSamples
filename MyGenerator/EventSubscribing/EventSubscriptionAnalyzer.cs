using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using MyTypes;
using System.Collections.Immutable;
using System.Linq;

namespace MyGenerator.EventSubscribing;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EventSubscriptionAnalyzer : DiagnosticAnalyzer
{
	private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
		id: "MY0001",
		title: $"{EventSubscribingConstants.InProcessSubscriptionMethodName} should be called when using {nameof(EventReceiverAttribute)}",
		messageFormat: $"Class {{0}} has a method with [{nameof(EventReceiverAttribute)}] but never calls {EventSubscribingConstants.InProcessSubscriptionMethodName}",
		category: "Usage",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.ClassDeclaration);
	}

	private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
	{
		var classDecl = (ClassDeclarationSyntax)context.Node;
		var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
		if (classSymbol == null) 
			return;

		var classUsesAttribute = classSymbol
			.GetMembers()
			.OfType<IMethodSymbol>()
			.Any(EventAttributeUtils.HasEventReceiverAttribute);

		if (!classUsesAttribute)
			return;

		bool classCallsSubscribe = classDecl
			.DescendantNodes()
			.OfType<InvocationExpressionSyntax>()
			.Select(inv => context.SemanticModel.GetSymbolInfo(inv).Symbol as IMethodSymbol)
			.Any(CallsSubscribeMethod);

		if (!classCallsSubscribe)
		{
			var diag = Diagnostic.Create(
				Rule,
				classDecl.Identifier.GetLocation(),
				classSymbol.Name
			);
			context.ReportDiagnostic(diag);
		}
	}

	private static bool CallsSubscribeMethod(IMethodSymbol method)
	{
		return method != null && 
                   (method.Name == EventSubscribingConstants.InProcessSubscriptionMethodName);
	}
}
