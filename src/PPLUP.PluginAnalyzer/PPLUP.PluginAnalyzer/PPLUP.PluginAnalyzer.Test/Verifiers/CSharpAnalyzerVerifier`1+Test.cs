using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Collections.Immutable;

namespace PPLUP.PluginAnalyzer.Test
{
    public static partial class CSharpAnalyzerVerifier<TAnalyzer>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        public class Test : CSharpAnalyzerTest<TAnalyzer, MSTestVerifier>
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
