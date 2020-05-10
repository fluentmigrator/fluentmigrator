using System.Threading.Tasks;
using NUnit.Framework;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.NUnit.CodeFixVerifier<
    FluentMigrator.Analyzers.MigrationAttributeVersionShouldBeUniqueAnalyzer,
    FluentMigrator.Analyzers.MigrationAttributeVersionShouldBeUniqueCodeFixProvider>;

namespace FluentMigrator.Analyzers.Tests
{
    public class MigrationAttributeVersionShouldBeUniqueUnitTests
    {
        [Test]
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

        [Test]
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
