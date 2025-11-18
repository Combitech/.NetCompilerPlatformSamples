using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using MyTypes;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace MyGenerator.EventSubscribing;


[Generator]
public class EventSubscriptionGenerator : IIncrementalGenerator
{

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var candidateMethods = context.SyntaxProvider.CreateSyntaxProvider(IsCandidateMethod, GetMethodInfo).Where(NotNull);
		var collected = candidateMethods.Collect();
		context.RegisterSourceOutput(collected, GenerateOutput);
	}

	private bool IsCandidateMethod(SyntaxNode node, CancellationToken _)
	{
		var methodDeclaration = node as MethodDeclarationSyntax;
		if (methodDeclaration == null)
			return false;

		foreach (var attrList in methodDeclaration.AttributeLists)
		{
			foreach (var attr in attrList.Attributes)
			{
				if (attr.Name.ToString() == nameof(EventReceiverAttribute) || attr.Name + "Attribute" == nameof(EventReceiverAttribute))
					return true;
			}
		}
		return false;
	}

	private static MethodInfo GetMethodInfo(GeneratorSyntaxContext ctx, CancellationToken _)
	{
		var methodSyntax = (MethodDeclarationSyntax)ctx.Node;
		var methodSymbol = ctx.SemanticModel.GetDeclaredSymbol(methodSyntax);

		if (methodSymbol is null || methodSymbol.Parameters.Length == 0 || methodSymbol.DeclaredAccessibility != Accessibility.Internal)
			return null;

		var firstParamType = methodSymbol.Parameters[0].Type;
		var paramTypeFullName = firstParamType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		return new MethodInfo(
			methodSymbol.ContainingType,
			methodSymbol.Name,
			paramTypeFullName
		);
	}

	private static bool NotNull(MethodInfo info) => info != null;

	private static void GenerateOutput(SourceProductionContext spc, ImmutableArray<MethodInfo> methods)
	{
		var grouped = new Dictionary<INamedTypeSymbol, List<NameType>>(SymbolEqualityComparer.Default);

		foreach (var m in methods)
		{
			if (m is null)
				continue;

			if (!grouped.TryGetValue(m.ContainingClass, out var list))
			{
				list = [];
				grouped[m.ContainingClass] = list;
			}

			list.Add(new NameType(m.MethodName, m.ParamType));
		}

		foreach (var kvp in grouped)
		{
			var source = GeneratePropertyCode(kvp.Key.Name, kvp.Key.ContainingNamespace.ToDisplayString(), kvp.Value);
			spc.AddSource($"{kvp.Key.Name}_EventSubscription.g.cs", SourceText.From(source, Encoding.UTF8));
		}
	}

	public static string GeneratePropertyCode(string className, string containingNamespace, List<NameType> eventType)
	{
		return $@"
using MyTypes;

namespace {containingNamespace}
{{
    internal static class {className}{EventSubscribingConstants.SubscriptionExtensionClassName}
    {{
        internal static void {EventSubscribingConstants.InProcessSubscriptionMethodName}(this {className} target)
        {{
            {string.Join("\r\n            ", eventType.Select(x => $"EventAggregator.Subscribe<{x.Type}>(target.{x.Name});"))}
        }}
    }}
}}";
	}

	public class MethodInfo
	{
		public INamedTypeSymbol ContainingClass;
		public string MethodName;
		public string ParamType;

		public MethodInfo(INamedTypeSymbol ContainingClass, string MethodName, string ParamType)
		{
			this.ContainingClass = ContainingClass;
			this.MethodName = MethodName;
			this.ParamType = ParamType;
		}
	}

	public struct NameType
	{
		public string Name;
		public string Type;
		public NameType(string Name, string Type)
		{
			this.Name = Name;
			this.Type = Type;
		}
	}
}
