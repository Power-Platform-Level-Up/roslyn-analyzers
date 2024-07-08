using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Collections.Immutable;

namespace PPLUP.PluginAnalyzer.Test
{
    public static partial class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        public class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, MSTestVerifier>
        {
            public Test()
            {
                SolutionTransforms.Add(
                    (solution, projectId) =>
                    {
                        var compilationOptions = solution.GetProject(projectId).CompilationOptions;
                        compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
                            compilationOptions.SpecificDiagnosticOptions.SetItems(
                                CSharpVerifierHelper.NullableWarnings
                            )
                        );
                        solution = solution.WithProjectCompilationOptions(
                            projectId,
                            compilationOptions
                        );

                        return solution;
                    }
                );

                ReferenceAssemblies = ReferenceAssemblies.NetFramework.Net462.Default.WithPackages(
                    ImmutableArray.Create(
                        new PackageIdentity("Microsoft.CrmSdk.XrmTooling.CoreAssembly", "9.1.1.45")
                    )
                );
            }
        }
    }
}
