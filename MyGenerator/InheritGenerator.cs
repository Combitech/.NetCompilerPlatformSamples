using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using MyTypes;
using System.Linq;
using System.Text;

namespace MyGenerator;

    [Generator]
    public class InheritGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Find all partial class declarations
            var classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => node is ClassDeclarationSyntax cds && cds.Modifiers.Any(SyntaxKind.PartialKeyword),
                    transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
                .Where(static cds => cds is not null);

            // Combine with the compilation
            var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndClasses, (spc, source) =>
            {
                var (compilation, classList) = source;
                var attributeTypeSymbol = compilation.GetTypeByMetadataName(typeof(InheritAttribute).FullName);

                if (attributeTypeSymbol == null)
                    return;

                foreach (var classDeclaration in classList)
                {
                    var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                    var classSymbol = model.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
                    if (classSymbol == null)
                        continue;

                    var fooAttributes = classSymbol.GetAttributes()
                        .Where(ad => SymbolEqualityComparer.Default.Equals(ad.AttributeClass, attributeTypeSymbol));

                    foreach (var attribute in fooAttributes)
                    {
                        var targetType = attribute.ConstructorArguments.FirstOrDefault().Value as INamedTypeSymbol;
                        if (targetType != null)
                        {
                            var code = GenerateClassCode(classSymbol, targetType);
                            spc.AddSource($"{classSymbol.Name}_g_{targetType.Name}.cs", SourceText.From(code, Encoding.UTF8));
                        }
                    }
                }
            });
        }

        private static string GenerateClassCode(INamedTypeSymbol classSymbol, INamedTypeSymbol targetType)
        {
            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine($"namespace {classSymbol.ContainingNamespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public partial class {classSymbol.Name}");
            sb.AppendLine("    {");

            foreach (var field in targetType.GetMembers().OfType<IFieldSymbol>().Where(x => !x.IsImplicitlyDeclared))
            {
                sb.AppendLine($"{field.DeclaredAccessibility.ToString().ToLower()} {field.Type.Name} {field.DeclaringSyntaxReferences.First().GetSyntax().ToFullString()};");
            }

            foreach (var property in targetType.GetMembers().OfType<IPropertySymbol>().Where(x => !x.IsImplicitlyDeclared))
            {
                sb.AppendLine(property.DeclaringSyntaxReferences.First().GetSyntax().ToFullString());
            }

            foreach (var method in targetType.GetMembers().OfType<IMethodSymbol>().Where(x => !x.IsImplicitlyDeclared))
            {
                if (method.MethodKind == MethodKind.Ordinary)
                {
                    sb.AppendLine(method.DeclaringSyntaxReferences.First().GetSyntax().ToFullString());
                }
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
