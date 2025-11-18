using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using MyTypes;
using System.Collections.Immutable;
using System.Linq;

namespace MyGenerator.EventSubscribing;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EventAccessorAnalyzer : DiagnosticAnalyzer
{
	private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
		id: "MY0002",
		title: $"Method should be internal when using {nameof(EventReceiverAttribute)}",
		messageFormat: $"Method {{0}} has [{nameof(EventReceiverAttribute)}] but is not internal",
		category: "Usage",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
	}

	private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
	{
		var methodDecl = (MethodDeclarationSyntax)context.Node;
		var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDecl) as IMethodSymbol;
		if (methodSymbol == null)
			return;

		if (!EventAttributeUtils.HasEventReceiverAttribute(methodSymbol))
			return;

		if (methodSymbol.DeclaredAccessibility != Accessibility.Internal)
		{
			var diag = Diagnostic.Create(
				Rule,
				methodDecl.Identifier.GetLocation(),
				methodSymbol.Name
			);
			context.ReportDiagnostic(diag);
		}
	}
}
