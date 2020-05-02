using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<
    FluentMigrator.Analyzers.MigrationAttributeVersionShouldBeUniqueAnalyzer,
    FluentMigrator.Analyzers.MigrationAttributeVersionShouldBeUniqueCodeFixProvider>;

namespace FluentMigrator.Analyzers.Tests
{
    [TestClass]
    public class MigrationAttributeVersionShouldBeUniqueUnitTests
    {
        [TestMethod]
        public async Task Doesnt_Warns_On_Unique_Migration_Version()
        {
            var test = @"
    using FluentMigrator;

    namespace ConsoleApplication1
    {
        [Migration(1)]
        public class TypeName
        {   
        }
    }";
            var expected = Verify.Diagnostic();
            await Verify.VerifyAnalyzerAsync(test, expected);
        }

        [TestMethod]
        public async Task Warns_On_Duplicate_Migration_Version()
        {
            var test = @"
    using FluentMigrator;

    namespace ConsoleApplication1
    {
        [Migration(1)]
        public class TypeName
        {   
        }

        [Migration(1)]
        public class TypeName2
        {   
        }
    }";

            var fixtest = @"";

            var expected = Verify.Diagnostic(nameof(MigrationAttributeVersionShouldBeUniqueAnalyzer)).WithLocation(11, 15).WithArguments("TypeName");
            await Verify.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}
