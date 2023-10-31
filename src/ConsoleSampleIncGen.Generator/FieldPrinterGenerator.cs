using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace ConsoleSampleIncGen.Generator;

[Generator]
public class FieldPrinterGenerator : IIncrementalGenerator
{
       public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register the syntax provider to select classes with properties
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax classDeclaration && classDeclaration.Members.OfType<PropertyDeclarationSyntax>().Any(),
                transform: static (context, _) => (ClassDeclarationSyntax)context.Node)
            .Collect();

        // Combine the syntax and compilation
        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations);

        // Register the source generator to generate source code
        context.RegisterSourceOutput(compilationAndClasses, GenerateSource);
    }

    private void GenerateSource(SourceProductionContext context, (Compilation Left, ImmutableArray<ClassDeclarationSyntax> Right) data)
    {
        var (compilation, classes) = data;
        foreach (var classDeclaration in classes)
        {
            var x = classDeclaration.Identifier.Text;
            var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var classSymbol = model.GetDeclaredSymbol(classDeclaration);

            if (classSymbol == null || !classSymbol.ContainingNamespace.ToDisplayString().Contains("Dto")) continue;

            var sourceCode = GenerateSourceCode(classSymbol);
            context.AddSource($"{classSymbol.Name}_PropertyList.cs", SourceText.From(sourceCode, Encoding.UTF8));
        }
    }

    private string GenerateSourceCode(INamedTypeSymbol classSymbol)
    {
        var className = classSymbol.Name;
        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
        var properties = classSymbol.GetMembers().OfType<IPropertySymbol>();

        var propertyListCode = string.Join(Environment.NewLine, properties.Select(p =>
            $@"            Console.WriteLine(""{p.Name}: "" + (instance.{p.Name}?.ToString() ?? ""none""));"));

        return $@"
using System;

namespace {namespaceName}
{{
    public static partial class {className}Extensions
    {{
        public static void PrintProperties(this {className} instance)
        {{
            if (instance == null) throw new ArgumentNullException(nameof(instance));
{propertyListCode}
        }}
    }}
}}
";
    }
}