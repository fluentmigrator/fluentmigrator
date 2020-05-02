using System.Threading.Tasks;
using Xunit;
using Verify = Microsoft.CodeAnalysis.CSharp.CodeFix.Testing.MSTest.CodeFixVerifier<
    FluentMigrator.Analyzers.MigrationAttributeVersionShouldBeUniqueAnalyzer,
    FluentMigrator.Analyzers.MigrationAttributeVersionShouldBeUniqueCodeFixProvider>;

namespace FluentMigrator.Analyzers.Tests
{
    public class MigrationAttributeVersionShouldBeUniqueUnitTests
    {
        [Fact]
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

            await Verify.VerifyCSharpDiagnosticAsync(test);
        }

        [Fact]
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
            await Verify.VerifyCSharpFixAsync(test, expected, fixtest);
        }
    }
}
