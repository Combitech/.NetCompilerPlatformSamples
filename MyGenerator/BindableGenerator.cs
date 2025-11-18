using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using MyTypes;
using System.Linq;
using System.Text;

namespace MyGenerator;

    [Generator]
    public class BindableGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var fieldDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => IsCandidateField(node),
                    transform: static (ctx, _) => GetFieldInfo(ctx))
                .Where(static fieldInfo => fieldInfo is not null);

            context.RegisterSourceOutput(fieldDeclarations, (spc, fieldInfoObj) =>
            {
                var fieldInfo = fieldInfoObj!;
                var source = GeneratePropertyCode(
                    fieldInfo.ContainingNamespace,
                    fieldInfo.ClassName,
                    fieldInfo.FieldName,
                    fieldInfo.PropertyName,
                    fieldInfo.PropertyType);

                spc.AddSource($"{fieldInfo.ClassName}_{fieldInfo.PropertyName}_Generated.cs", SourceText.From(source, Encoding.UTF8));
            });
        }

        private static bool IsCandidateField(SyntaxNode node)
        {
            if (node is FieldDeclarationSyntax fieldDeclaration)
            {
                return fieldDeclaration.AttributeLists.SelectMany(list => list.Attributes).Any(attr => attr.Name.ToString() == nameof(BindableProperty));
            }
            return false;
        }

        private static FieldInfo GetFieldInfo(GeneratorSyntaxContext context)
        {
            var fieldDeclaration = (FieldDeclarationSyntax)context.Node;
            var variable = fieldDeclaration.Declaration.Variables.FirstOrDefault();
            if (variable == null)
                return null;

            var fieldSymbol = context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
            if (fieldSymbol == null)
                return null;

            var classSymbol = fieldSymbol.ContainingType;
            var propertyName = GeneratePropertyName(fieldSymbol.Name);
            var propertyType = fieldSymbol.Type.ToDisplayString();
            var containingNamespace = classSymbol.ContainingNamespace.ToDisplayString();

            return new FieldInfo
            {
                ContainingNamespace = containingNamespace,
                ClassName = classSymbol.Name,
                FieldName = fieldSymbol.Name,
                PropertyName = propertyName,
                PropertyType = propertyType
            };
        }

        public static string GeneratePropertyName(string fieldName)
        {
            if (fieldName.Length > 1 && fieldName.StartsWith("_"))
            {
                fieldName = fieldName.Substring(1);
            }
            if (fieldName.Length > 1)
            {
                fieldName = char.ToUpper(fieldName[0]) + fieldName.Substring(1);
            }
            return fieldName;
        }

        public static string GeneratePropertyCode(string containingNamespace, string className, string fieldName, string propertyName, string propertyType)
        {
            return $@"
namespace {containingNamespace}
{{
    public partial class {className}
    {{
        public {propertyType} {propertyName}
        {{
            get => {fieldName};
            set => {fieldName} = value;
        }}
    }}
}}";
        }

        private class FieldInfo
        {
            public string ContainingNamespace { get; set; } = "";
            public string ClassName { get; set; } = "";
            public string FieldName { get; set; } = "";
            public string PropertyName { get; set; } = "";
            public string PropertyType { get; set; } = "";
        }
    }
