using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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
            context.RegisterSemanticModelAction(SemanticModelAction);
        }

        private static void SemanticModelAction(SemanticModelAnalysisContext context)
        {
            var model = context.SemanticModel;
            var tree = model.SyntaxTree;
            var classes = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach (var item in classes)
            {
                var symbol = model.GetDeclaredSymbol(item);
                var implimentsIPlugin = ImplimentsIPlugin(symbol);
                if (implimentsIPlugin)
                {
                    checkClass(item, out var diags);
                    foreach (var d in diags)
                    {
                        context.ReportDiagnostic(d);
                    }
                }
            }
        }

        private static bool ImplimentsIPlugin(INamedTypeSymbol symbol)
        {
            while (true)
            {
                if (symbol.ToString() == "Microsoft.Xrm.Sdk.IPlugin")
                {
                    return true;
                }
                if (symbol.Interfaces != null && symbol.Interfaces.Length > 0)
                {
                    foreach (var interfaceSymbol in symbol.Interfaces)
                    {
                        if (interfaceSymbol.ToString() == "Microsoft.Xrm.Sdk.IPlugin")
                        {
                            return true;
                        }
                    }
                }
                if (symbol.BaseType != null)
                {
                    symbol = symbol.BaseType;
                    continue;
                }
                break;
            }
            return false;
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
