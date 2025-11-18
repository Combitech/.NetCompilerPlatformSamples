using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using MyTypes;
using System.Linq;
using System.Text;

namespace MyGenerator;

[Generator]
public class HelloGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var candidateClasses = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: static (node, _) => IsCandidateClass(node),
				transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node
			)
			.Where(static classDecl => classDecl is not null);

		var compilationAndClasses = context.CompilationProvider.Combine(candidateClasses.Collect());

		context.RegisterSourceOutput(compilationAndClasses, (spc, source) =>
		{
			var (compilation, classes) = source;
			foreach (var classDeclaration in classes)
			{
				var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
				var classSymbol = model.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
				if (classSymbol == null)
					continue;

				var sourceText = GenerateMethodCode(classSymbol, "Hello");
				spc.AddSource($"{classSymbol.Name}_Generated.cs", SourceText.From(sourceText, Encoding.UTF8));
			}
		});
	}

	private static bool IsCandidateClass(SyntaxNode node)
	{
		if (node is ClassDeclarationSyntax classDecl && classDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
		{
			return classDecl.AttributeLists
				.SelectMany(list => list.Attributes)
				.Any(attr => attr.Name.ToString() == nameof(CanSayHello));
		}
		return false;
	}

	private static string GenerateMethodCode(INamedTypeSymbol classSymbol, string methodName)
	{
		return $@"
namespace {classSymbol.ContainingNamespace.ToDisplayString()}
{{
    public partial class {classSymbol.Name}
    {{
        public static void {methodName}()
        {{
            System.Console.WriteLine(""Method {methodName} invoked"");
        }}
    }}
}}";
	}
}
