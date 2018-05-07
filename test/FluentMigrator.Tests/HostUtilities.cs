#region License
// Copyright (c) 2018, FluentMigrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using NUnit.Framework;

namespace FluentMigrator.Tests
{
    public static class HostUtilities
    {
        private const string DataDirectoryMacro = "|DataDirectory|";
        private const string DataDirectory = "DataDirectory";

        public static string ReplaceDataDirectory(string inputString)
        {
            string str = inputString.Trim();
            if (!string.IsNullOrEmpty(inputString) && inputString.StartsWith(DataDirectoryMacro, StringComparison.InvariantCultureIgnoreCase))
            {
                string path1 = AppDomain.CurrentDomain.GetData(DataDirectory) as string;
                if (string.IsNullOrEmpty(path1))
                    path1 = AppDomain.CurrentDomain.BaseDirectory;
                if (string.IsNullOrEmpty(path1))
                    path1 = Environment.CurrentDirectory;
                if (string.IsNullOrEmpty(path1))
                    path1 = string.Empty;
                int length = DataDirectoryMacro.Length;
                if (inputString.Length > DataDirectoryMacro.Length && 92 == inputString[DataDirectoryMacro.Length])
                    ++length;
                str = Path.Combine(path1, inputString.Substring(length));
            }
            return str;
        }

        public static bool TryGetJetCatalogType(out Type jetCatalogType)
        {
            jetCatalogType = Type.GetTypeFromProgID("ADOX.Catalog", false);
            return jetCatalogType != null;
        }

        public static bool ProbeSqlServerCeBehavior()
        {
            var asm = typeof(SqlCeConnection).Assembly;
            var type = asm.GetType("System.Data.SqlServerCe.NativeMethods");
            if (SqlServerCeCanFindItsLibraries(type))
                return true;

            try
            {
                return SqlServerCeLoadBinaries(type);
            }
            catch (Exception ex)
            {
                TestContext.Out.WriteLine(ex);
                return false;
            }
        }

        private static bool SqlServerCeLoadBinaries(Type type)
        {
            var method = type.GetMethod("LoadNativeBinariesFromPrivateFolder", BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Static);
            Debug.Assert(method != null, nameof(method) + " != null");
            var result = (bool)method.Invoke(null, new object[] { AppContext.BaseDirectory });
            return result;
        }

        private static bool SqlServerCeCanFindItsLibraries(Type type)
        {
            var method = type.GetMethod("LoadNativeBinaries", BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Static);
            try
            {
                Debug.Assert(method != null, nameof(method) + " != null");
                method.Invoke(null, null);
                return true;
            }
            catch (TargetInvocationException ex) when (ex.InnerException is SqlCeException sce && sce.NativeError == -1)
            {
                return false;
            }
            catch (Exception ex)
            {
                TestContext.Out.WriteLine(ex);
                return false;
            }
        }
    }
}
