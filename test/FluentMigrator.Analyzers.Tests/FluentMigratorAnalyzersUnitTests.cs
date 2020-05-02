using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using FluentMigrator.Analyzers;

namespace FluentMigrator.Analyzers.Tests
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void Doesnt_Warns_On_Unique_Migration_Version()
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

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void Warns_On_Duplicate_Migration_Version()
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
            var expected = new DiagnosticResult
            {
                Id = nameof(MigrationAttributeVersionShouldBeUnique),
                Message = string.Format("Type name '{0}' contains lowercase letters", "TypeName"),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 15)
                }
            };

            VerifyCSharpDiagnostic(test, expected);
        }


        //    var fixtest = @"
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
        //    VerifyCSharpFix(test, fixtest);

        //    protected override CodeFixProvider GetCSharpCodeFixProvider()
        //    {
        //        return new MigrationAttributeVersionShouldBeUniqueCodeFixProvider();
        //    }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MigrationAttributeVersionShouldBeUnique();
        }
    }
}
