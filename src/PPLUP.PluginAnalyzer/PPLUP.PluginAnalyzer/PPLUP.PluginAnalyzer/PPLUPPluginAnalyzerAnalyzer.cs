using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PPLUP.PluginAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PPLUPPluginAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "PPLUPStatelessPlugins";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(Resources.AnalyzerTitle),
            Resources.ResourceManager,
            typeof(Resources)
        );
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(Resources.AnalyzerMessageFormat),
            Resources.ResourceManager,
            typeof(Resources)
        );
        private static readonly LocalizableString Description = new LocalizableResourceString(
            nameof(Resources.AnalyzerDescription),
            Resources.ResourceManager,
            typeof(Resources)
        );
        private const string Category = "Dataverse";

        private static readonly Action<CompilationAnalysisContext> CompilationAction =
            CompilationAnalysis;

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(Rule); }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            //context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
            //context.RegisterSyntaxTreeAction(SyntaxTreeAnalysis);
            context.RegisterCompilationAction(CompilationAction);
        }

        private static void CompilationAnalysis(CompilationAnalysisContext context)
        {
            Console.WriteLine(
                $"Parsing {context.Compilation.SyntaxTrees.ToArray().Length} SyntaxTrees"
            );
            foreach (var tree in context.Compilation.SyntaxTrees)
            {
                var root = tree.GetRoot();

                var classNodes = (
                    from c in root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                    where
                        c.BaseList
                            .DescendantNodes()
                            .OfType<IdentifierNameSyntax>()
                            .Any(b => b.Identifier.Text == "IPlugin")
                    select c
                ).ToList();

                foreach (var item in classNodes)
                {
                    checkClass(item, out var diags);

                    foreach (var d in diags)
                    {
                        context.ReportDiagnostic(d);
                    }
                }

                var baseClasses =
                    from c in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>()
                    where
                        c.BaseList
                            .DescendantNodes()
                            .OfType<IdentifierNameSyntax>()
                            .Any(b => b.Identifier.Text == "IPlugin")
                    select c;

                Console.WriteLine($"Parsing {baseClasses.ToArray().Length} BaseTypeDeclarations");
                foreach (var baseClass in baseClasses)
                {
                    var bc =
                        from c in root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                        where
                            c.BaseList
                                .DescendantNodes()
                                .OfType<IdentifierNameSyntax>()
                                .Any(b => b.Identifier.Text == baseClass.Identifier.Text)
                        select c;
                    Console.WriteLine($"Parsing {bc.ToArray().Length} ClassDeclarations");
                    foreach (var item in bc)
                    {
                        checkClass(item, out var diags);

                        foreach (var d in diags)
                        {
                            context.ReportDiagnostic(d);
                        }
                    }
                }
            }
        }

        private static void SyntaxTreeAnalysis(SyntaxTreeAnalysisContext context)
        {
            var root = context.Tree.GetRoot();

            var classNodes = (
                from c in root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                where
                    c.BaseList
                        .DescendantNodes()
                        .OfType<IdentifierNameSyntax>()
                        .Any(b => b.Identifier.Text == "IPlugin")
                select c
            ).ToList();

            foreach (var item in classNodes)
            {
                checkClass(item, out var diags);
                foreach (var d in diags)
                {
                    context.ReportDiagnostic(d);
                }
            }
        }

        private static void checkClass(ClassDeclarationSyntax root, out List<Diagnostic> diags)
        {
            diags = new List<Diagnostic>();

            var fieldNodes =
                from f in root.DescendantNodes().OfType<FieldDeclarationSyntax>()
                where
                    f.Declaration
                        .DescendantNodes()
                        .OfType<IdentifierNameSyntax>()
                        .Any(t => t.Identifier.Text == "IOrganizationService")
                select f;
            foreach (var f in fieldNodes)
            {
                var diagnostic = Diagnostic.Create(Rule, f.GetLocation(), root.Identifier.Text);
                diags.Add(diagnostic);
            }
        }
    }
}
