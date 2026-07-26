#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

// Compiled at build time by RoslynCodeTaskFactory (works under both
// Visual Studio's .NET Framework MSBuild and the dotnet CLI).
//
// Deliberately conservative C#: no LINQ, no System.Text.Json, no
// dependencies outside the default RoslynCodeTaskFactory reference set.
// The JSON writer is hand-rolled so the manifest is byte-for-byte
// deterministic (sorted objects, '\n' line endings, lowercase hex
// checksums) -- clean diffs are the entire point of a source control model.
//
// Classification is driven entirely by pivot metadata on the items
// (ObjectType, NamePolicy, Role, ContentType, UniqueIdentity), declared in
// FluentMigrator.Net.Sdk.Conventions.props. This task contains no directory
// heuristics: the only parsing it does is the SchemaDotObject filename
// policy ("sales.open_orders.sql" -> schema "sales", name "open_orders").
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Build.Framework;

namespace FluentMigrator.Net.Sdk.Tasks
{
    public sealed class BuildSourceModel : Microsoft.Build.Utilities.Task
    {
        [Required]
        public ITaskItem[] SourceFiles { get; set; }

        [Required]
        public string ProjectName { get; set; }

        [Required]
        public string ManifestPath { get; set; }

        public string SourceModelRoot { get; set; }

        public string Dialect { get; set; }

        public string DefaultSchema { get; set; }

        public bool TreatValidationWarningsAsErrors { get; set; }

        public bool IsRootModule { get; set; }

        public string Convention { get; set; }

        private sealed class ModelObject
        {
            public string Provider;
            public string Type;
            public string Schema;
            public string Name;
            public string Path;
            public string Checksum;
            public bool UniqueIdentity;
            public SortedDictionary<string, string> Properties = new SortedDictionary<string, string>(StringComparer.Ordinal);
        }

        public override bool Execute()
        {
            var objects = new List<ModelObject>();
            var providers = new SortedSet<string>(StringComparer.Ordinal);

            string root = string.IsNullOrEmpty(SourceModelRoot)
                ? Directory.GetCurrentDirectory()
                : SourceModelRoot;
            root = Path.GetFullPath(root);
            if (!root.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                root += Path.DirectorySeparatorChar;
            }

            foreach (ITaskItem item in SourceFiles ?? new ITaskItem[0])
            {
                string fullPath = item.GetMetadata("FullPath");
                if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
                {
                    Report("FMSDK002", "Source model file not found: " + item.ItemSpec, false);
                    continue;
                }

                string rel = MakeRelative(root, fullPath);
                ModelObject obj = Classify(item, rel);
                obj.Path = rel;
                obj.Checksum = "sha256:" + Sha256Hex(fullPath);

                if (IsEffectivelyEmpty(fullPath))
                {
                    Report("FMSDK101", "Source model file is empty: " + rel, true);
                }

                if (obj.Type == "unclassified")
                {
                    Report("FMSDK100",
                        "'" + rel + "' carries no ObjectType metadata. Declare it through a pivot item " +
                        "(SqlObject, MigrationScript, ...) or set ObjectType on the item.", true);
                }

                providers.Add(obj.Provider);
                objects.Add(obj);
            }

            // Deterministic order: type, schema, name, path (ordinal).
            objects.Sort(delegate (ModelObject a, ModelObject b)
            {
                int c = string.CompareOrdinal(a.Type, b.Type);
                if (c != 0) return c;
                c = string.CompareOrdinal(a.Schema, b.Schema);
                if (c != 0) return c;
                c = string.CompareOrdinal(a.Name, b.Name);
                if (c != 0) return c;
                return string.CompareOrdinal(a.Path, b.Path);
            });

            // Duplicate detection on logical identity, only for pivots that
            // declare UniqueIdentity (the Schema/* programmable objects).
            var seen = new Dictionary<string, ModelObject>(StringComparer.OrdinalIgnoreCase);
            foreach (ModelObject obj in objects)
            {
                if (!obj.UniqueIdentity)
                {
                    continue;
                }
                string key = obj.Provider + "|" + obj.Type + "|" + obj.Schema + "|" + obj.Name;
                ModelObject first;
                if (seen.TryGetValue(key, out first))
                {
                    Log.LogError(null, "FMSDK001", null, obj.Path, 0, 0, 0, 0,
                        "Duplicate object '" + Display(obj) + "' defined in both '" + first.Path + "' and '" + obj.Path + "'.");
                }
                else
                {
                    seen.Add(key, obj);
                }
            }

            // Tools/ artifacts (CreateDatabase, baseline) belong to the root
            // module of a database; anywhere else is almost certainly a
            // composition mistake once multiple modules share a database.
            if (!IsRootModule)
            {
                foreach (ModelObject obj in objects)
                {
                    if (obj.Type == "tool")
                    {
                        Report("FMSDK102",
                            "'" + obj.Path + "' is a Tools/ artifact but IsDatabaseRootModule is not set. " +
                            "Set <IsDatabaseRootModule>true</IsDatabaseRootModule> on the module that owns " +
                            "database creation and the baseline.", true);
                    }
                }
            }

            if (Log.HasLoggedErrors)
            {
                return false;
            }

            string json = WriteManifest(objects, providers);
            WriteIfChanged(ManifestPath, json);

            Log.LogMessage(MessageImportance.Normal,
                "Source model: {0} object(s) across {1} provider(s).", objects.Count, providers.Count);
            return true;
        }

        private ModelObject Classify(ITaskItem item, string relPath)
        {
            var obj = new ModelObject();

            string provider = item.GetMetadata("Provider");
            obj.Provider = string.IsNullOrEmpty(provider) ? "sql" : provider.ToLowerInvariant();

            string objectType = item.GetMetadata("ObjectType");
            obj.Type = string.IsNullOrEmpty(objectType) ? "unclassified" : objectType.ToLowerInvariant();

            obj.UniqueIdentity = string.Equals(item.GetMetadata("UniqueIdentity"), "true", StringComparison.OrdinalIgnoreCase);

            int lastSlash = relPath.LastIndexOf('/');
            string fileName = lastSlash >= 0 ? relPath.Substring(lastSlash + 1) : relPath;
            int extDot = fileName.LastIndexOf('.');
            string stem = extDot > 0 ? fileName.Substring(0, extDot) : fileName;

            string schema = string.Empty;
            string name = stem;

            string namePolicy = item.GetMetadata("NamePolicy");
            if (string.Equals(namePolicy, "SchemaDotObject", StringComparison.OrdinalIgnoreCase))
            {
                int firstDot = stem.IndexOf('.');
                if (firstDot > 0 && firstDot < stem.Length - 1)
                {
                    schema = stem.Substring(0, firstDot);
                    name = stem.Substring(firstDot + 1);
                }
                else
                {
                    schema = DefaultSchema ?? string.Empty;
                    name = stem;
                }
            }
            else if (string.Equals(namePolicy, "FlywayPrefixed", StringComparison.OrdinalIgnoreCase))
            {
                // V1__Add_Users -> version "1", name "Add_Users"
                // R__01_Create_View -> no version, name "01_Create_View"
                int sep = stem.IndexOf("__", StringComparison.Ordinal);
                string head = sep >= 0 ? stem.Substring(0, sep) : stem;
                name = sep >= 0 ? stem.Substring(sep + 2) : stem;
                if (sep >= 0 && head.Length > 1 && char.IsLetter(head[0]))
                {
                    obj.Properties["version"] = head.Substring(1);
                }
            }

            string explicitSchema = item.GetMetadata("Schema");
            string explicitName = item.GetMetadata("ObjectName");
            obj.Schema = string.IsNullOrEmpty(explicitSchema) ? schema : explicitSchema;
            obj.Name = string.IsNullOrEmpty(explicitName) ? name : explicitName;

            string role = item.GetMetadata("Role");
            if (!string.IsNullOrEmpty(role))
            {
                obj.Properties["role"] = role.ToLowerInvariant();
            }

            string contentType = item.GetMetadata("ContentType");
            if (!string.IsNullOrEmpty(contentType))
            {
                obj.Properties["contentType"] = contentType.ToLowerInvariant();
            }

            // Normalized execution facet: once | onChange | always. Declared
            // per pivot by the convention pack so manifests from different
            // layout conventions (fluentmigrator, flyway, ...) stay comparable.
            string execution = item.GetMetadata("Execution");
            if (!string.IsNullOrEmpty(execution))
            {
                obj.Properties["execution"] = execution;
            }
            string stage = item.GetMetadata("Stage");
            if (!string.IsNullOrEmpty(stage))
            {
                obj.Properties["stage"] = stage.ToLowerInvariant();
            }

            return obj;
        }

        private static string Display(ModelObject obj)
        {
            return string.IsNullOrEmpty(obj.Schema)
                ? obj.Type + " " + obj.Name
                : obj.Type + " " + obj.Schema + "." + obj.Name;
        }

        private void Report(string code, string message, bool isWarning)
        {
            if (isWarning && !TreatValidationWarningsAsErrors)
            {
                Log.LogWarning(null, code, null, null, 0, 0, 0, 0, message);
            }
            else
            {
                Log.LogError(null, code, null, null, 0, 0, 0, 0, message);
            }
        }

        private static string MakeRelative(string rootWithSeparator, string fullPath)
        {
            string rel;
            if (fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
            {
                rel = fullPath.Substring(rootWithSeparator.Length);
            }
            else
            {
                rel = fullPath;
            }
            return rel.Replace('\\', '/');
        }

        private static bool IsEffectivelyEmpty(string path)
        {
            var info = new FileInfo(path);
            if (info.Length == 0) return true;
            if (info.Length > 4096) return false;
            string text = File.ReadAllText(path);
            return text.Trim().Length == 0;
        }

        private static string Sha256Hex(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                byte[] hash = sha.ComputeHash(stream);
                var sb = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        private string WriteManifest(List<ModelObject> objects, SortedSet<string> providers)
        {
            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("  \"manifestVersion\": 1,\n");
            sb.Append("  \"project\": ").Append(Quote(ProjectName)).Append(",\n");
            sb.Append("  \"dialect\": ").Append(Quote(string.IsNullOrEmpty(Dialect) ? "generic" : Dialect)).Append(",\n");
            sb.Append("  \"convention\": ").Append(Quote(string.IsNullOrEmpty(Convention) ? "fluentmigrator" : Convention)).Append(",\n");
            sb.Append("  \"defaultSchema\": ").Append(Quote(DefaultSchema ?? string.Empty)).Append(",\n");
            if (IsRootModule)
            {
                sb.Append("  \"role\": \"root\",\n");
            }

            sb.Append("  \"providers\": [");
            bool firstProvider = true;
            foreach (string p in providers)
            {
                if (!firstProvider) sb.Append(", ");
                sb.Append(Quote(p));
                firstProvider = false;
            }
            sb.Append("],\n");

            sb.Append("  \"objects\": [");
            for (int i = 0; i < objects.Count; i++)
            {
                ModelObject o = objects[i];
                sb.Append(i == 0 ? "\n" : ",\n");
                sb.Append("    {\n");
                sb.Append("      \"provider\": ").Append(Quote(o.Provider)).Append(",\n");
                sb.Append("      \"type\": ").Append(Quote(o.Type)).Append(",\n");
                sb.Append("      \"schema\": ").Append(Quote(o.Schema)).Append(",\n");
                sb.Append("      \"name\": ").Append(Quote(o.Name)).Append(",\n");
                sb.Append("      \"path\": ").Append(Quote(o.Path)).Append(",\n");
                sb.Append("      \"checksum\": ").Append(Quote(o.Checksum));
                if (o.Properties.Count > 0)
                {
                    sb.Append(",\n      \"properties\": {");
                    bool firstProp = true;
                    foreach (KeyValuePair<string, string> kv in o.Properties)
                    {
                        if (!firstProp) sb.Append(", ");
                        sb.Append(Quote(kv.Key)).Append(": ").Append(Quote(kv.Value));
                        firstProp = false;
                    }
                    sb.Append("}");
                }
                sb.Append("\n    }");
            }
            sb.Append(objects.Count == 0 ? "]\n" : "\n  ]\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        private static string Quote(string value)
        {
            var sb = new StringBuilder("\"");
            if (value != null)
            {
                foreach (char c in value)
                {
                    switch (c)
                    {
                        case '"': sb.Append("\\\""); break;
                        case '\\': sb.Append("\\\\"); break;
                        case '\n': sb.Append("\\n"); break;
                        case '\r': sb.Append("\\r"); break;
                        case '\t': sb.Append("\\t"); break;
                        default:
                            if (c < 0x20)
                            {
                                sb.Append("\\u").Append(((int)c).ToString("x4"));
                            }
                            else
                            {
                                sb.Append(c);
                            }
                            break;
                    }
                }
            }
            sb.Append('"');
            return sb.ToString();
        }

        private void WriteIfChanged(string path, string content)
        {
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (File.Exists(path))
            {
                string existing = File.ReadAllText(path);
                if (string.Equals(existing, content, StringComparison.Ordinal))
                {
                    return;
                }
            }
            File.WriteAllText(path, content, new UTF8Encoding(false));
        }
    }
}
