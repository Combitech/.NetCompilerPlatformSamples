using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using MyTypes;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;

namespace MyGenerator;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ConstStringJsonAnalyzer : DiagnosticAnalyzer
{
	private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
		id: "MYJSON001",
		title: "Const string must contain valid JSON",
		messageFormat: "Const string '{0}' does not contain valid JSON",
		category: "Usage",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSyntaxNodeAction(AnalyzeConstString, SyntaxKind.FieldDeclaration);
	}

	private void AnalyzeConstString(SyntaxNodeAnalysisContext context)
	{
		var node = context.Node;
		VariableDeclaratorSyntax variable = null;
		TypeSyntax typeSyntax = null;
		bool isConst = false;

		if (node is FieldDeclarationSyntax field)
		{
			isConst = field.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword));
			typeSyntax = field.Declaration.Type;
			variable = field.Declaration.Variables.FirstOrDefault();
		}

		if (!isConst || typeSyntax == null || variable == null)
			return;

		var type = context.SemanticModel.GetTypeInfo(typeSyntax).Type;
		if (type?.SpecialType != SpecialType.System_String)
			return;

		var symbol = context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
		if (!symbol.GetAttributes().Any(x => x.AttributeClass?.Name == nameof(IsJsonAttribute)))
			return;
		var value = symbol?.ConstantValue as string;
		if (value == null)
			return;

		try
		{
			JObject.Parse(value);
		}
		catch
		{
			var diag = Diagnostic.Create(Rule, variable.GetLocation(), variable.Identifier.Text);
			context.ReportDiagnostic(diag);
		}
	}
}