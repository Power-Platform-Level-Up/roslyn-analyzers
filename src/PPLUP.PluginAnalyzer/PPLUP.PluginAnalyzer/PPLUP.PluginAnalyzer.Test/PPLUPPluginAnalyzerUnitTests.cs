using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = PPLUP.PluginAnalyzer.Test.CSharpCodeFixVerifier<
    PPLUP.PluginAnalyzer.PPLUPPluginAnalyzerAnalyzer,
    PPLUP.PluginAnalyzer.PPLUPPluginAnalyzerCodeFixProvider
>;
using Microsoft.Xrm.Sdk;
using Microsoft.CodeAnalysis.Testing;

namespace PPLUP.PluginAnalyzer.Test
{
    [TestClass]
    public class PPLUPPluginAnalyzerUnitTest
    {
        [TestMethod]
        public async Task AnalyzerShouldPass()
        {
            var test =
                @"
using System;
using Microsoft.Xrm.Sdk;

namespace MyPlugin
{
    class {|#0:TypeName|} : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
        }
    }
}";

            //        var fixtest =
            //            @"
            //using System;
            //using System.Collections.Generic;
            //using System.Linq;
            //using System.Text;
            //using System.Threading.Tasks;
            //using System.Diagnostics;

            //namespace ConsoleApplication1
            //{
            //    class TYPENAME
            //    {
            //    }
            //}";

            await VerifyCS.VerifyAnalyzerAsync(test);
            //await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }

        [TestMethod]
        public async Task AnalyzerShouldFail()
        {
            var test =
                @"
using System;
using Microsoft.Xrm.Sdk;

namespace MyPlugin
{
    class {|#0:TypeName|} : IPlugin
    {
        private IOrganizationService _organizationService;
        public void Execute(IServiceProvider serviceProvider)
        {
        }
    }
}";

            var expected = VerifyCS
                .Diagnostic("PPLUPStatelessPlugins")
                .WithLocation(9, 9)
                .WithArguments("TypeName");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task AnalyzerWithPluginBaseShouldFail()
        {
            var test =
                @"
using System;
using Microsoft.Xrm.Sdk;

namespace MyPlugin
{
    class {|#0:TypeName|} : PluginBase
    {
        private IOrganizationService _organizationService;
    }

    public abstract class PluginBase : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
        }
    }
}";

            var expected = VerifyCS
                .Diagnostic("PPLUPStatelessPlugins")
                .WithLocation(9, 9)
                .WithArguments("TypeName");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task AnalyzerWithPluginBaseShouldPass()
        {
            var test =
                @"
using System;
using Microsoft.Xrm.Sdk;

namespace MyPlugin
{
    class {|#0:TypeName|} : PluginBase
    {
    }

    public abstract class PluginBase : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }
    }
}
