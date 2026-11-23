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
// Determinism also has to hold across machines. Two inputs used to break it:
//  - Checksums are over normalized text: a leading UTF-8 BOM is dropped and
//    CRLF becomes LF before hashing. A Windows checkout with core.autocrlf and
//    a Linux CI run then agree, and so does the onChange facet keyed off them.
//  - Paths never leak the build machine. A file outside SourceModelRoot is
//    recorded by its Link metadata, or else relative to the root with '../';
//    on another drive or network share, where no relative path exists, by its
//    file name (FMSDK103 warns about both).
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

        /// Codes that stay warnings even with TreatValidationWarningsAsErrors
        /// (NoWarn and WarningsNotAsErrors), upper case, each between ';'.
        public string WarningsNotAsErrors { get; set; }

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

                string rel = ManifestPathFor(item, root, fullPath);
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
                obj.Properties["execution"] = NormalizeExecution(execution, relPath);
            }
            string stage = item.GetMetadata("Stage");
            if (!string.IsNullOrEmpty(stage))
            {
                obj.Properties["stage"] = stage.ToLowerInvariant();
            }

            return obj;
        }

        private static readonly string[] CanonicalExecutions = { "once", "onChange", "always" };

        /// The execution facet is what makes manifests from different conventions
        /// comparable, so its spelling is part of the contract: a known value in
        /// any case becomes the canonical spelling, and anything else is reported
        /// (FMSDK104) and recorded as written.
        private string NormalizeExecution(string execution, string relPath)
        {
            string trimmed = execution.Trim();
            foreach (string canonical in CanonicalExecutions)
            {
                if (string.Equals(trimmed, canonical, StringComparison.OrdinalIgnoreCase))
                {
                    return canonical;
                }
            }

            Report("FMSDK104",
                "'" + relPath + "' declares Execution '" + execution + "'. The execution facet is one of " +
                "once, onChange or always; any other value makes this manifest incomparable with others.", true);
            return trimmed;
        }

        /// The path the manifest records: relative to SourceModelRoot, with '/'
        /// separators. A file outside the root is recorded by its Link metadata
        /// (the item's path in the project, which MSBuild uses for exactly this),
        /// or else relative to the root with '../' and an FMSDK103 warning. A
        /// file on another drive or network share has no relative path, so it
        /// is recorded by its file name, again with FMSDK103. An absolute path
        /// would make the committed manifest differ per machine.
        private string ManifestPathFor(ITaskItem item, string rootWithSeparator, string fullPath)
        {
            string root = WithoutExtendedPrefix(rootWithSeparator);
            string file = WithoutExtendedPrefix(fullPath);
            if (file.StartsWith(root, PathComparison))
            {
                return file.Substring(root.Length).Replace('\\', '/');
            }

            string link = item.GetMetadata("Link");
            if (!string.IsNullOrEmpty(link))
            {
                return link.Replace('\\', '/');
            }

            string rel = RelativeTo(root, file);
            if (rel != null)
            {
                Report("FMSDK103",
                    "'" + rel + "' is outside SourceModelRoot, so the manifest records it relative to the root. " +
                    "Set Link metadata on the item to give it a path inside the project.", true);
                return rel;
            }

            string name = Path.GetFileName(file);
            Report("FMSDK103",
                "'" + fullPath + "' is on a different drive or network share from SourceModelRoot, so no relative " +
                "path exists and the manifest records only its file name, '" + name + "'. " +
                "Set Link metadata on the item to give it a path inside the project.", true);
            return name;
        }

        /// Windows paths compare without case; elsewhere, case is significant.
        private static readonly StringComparison PathComparison =
            Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        /// \\?\C:\x and \\?\UNC\server\share\x name the same files as C:\x and
        /// \\server\share\x; dropping the prefix lets them compare equal.
        private static string WithoutExtendedPrefix(string path)
        {
            if (path.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase))
            {
                return @"\\" + path.Substring(8);
            }
            if (path.StartsWith(@"\\?\", StringComparison.Ordinal) || path.StartsWith(@"\\.\", StringComparison.Ordinal))
            {
                return path.Substring(4);
            }
            return path;
        }

        /// The '../' path from the root directory to a file, or null when the two
        /// have different path roots: another drive (C:\ vs D:\) or another share
        /// (\\server\a vs \\server\b, or the same share under another host name).
        /// A share's root is the share itself, so '..' can't climb from one share
        /// into another, and a relative path between them would name a different file.
        private static string RelativeTo(string rootWithSeparator, string fullPath)
        {
            string rootRoot = Path.GetPathRoot(rootWithSeparator);
            string fileRoot = Path.GetPathRoot(fullPath);
            if (string.IsNullOrEmpty(rootRoot)
                || !string.Equals(rootRoot.TrimEnd('\\', '/'), fileRoot.TrimEnd('\\', '/'), PathComparison))
            {
                return null;
            }

            char[] separators = { '\\', '/' };
            string[] from = rootWithSeparator.Substring(rootRoot.Length).Split(separators, StringSplitOptions.RemoveEmptyEntries);
            string[] to = fullPath.Substring(fileRoot.Length).Split(separators, StringSplitOptions.RemoveEmptyEntries);

            int common = 0;
            while (common < from.Length && common < to.Length - 1 && string.Equals(from[common], to[common], PathComparison))
            {
                common++;
            }

            var rel = new StringBuilder();
            for (int i = common; i < from.Length; i++)
            {
                rel.Append("../");
            }
            for (int i = common; i < to.Length; i++)
            {
                if (i > common)
                {
                    rel.Append('/');
                }
                rel.Append(to[i]);
            }
            return rel.ToString();
        }

        private static string Display(ModelObject obj)
        {
            return string.IsNullOrEmpty(obj.Schema)
                ? obj.Type + " " + obj.Name
                : obj.Type + " " + obj.Schema + "." + obj.Name;
        }

        private void Report(string code, string message, bool isWarning)
        {
            bool promote = TreatValidationWarningsAsErrors
                && (WarningsNotAsErrors ?? "").IndexOf(";" + code + ";", StringComparison.Ordinal) < 0;
            if (isWarning && !promote)
            {
                Log.LogWarning(null, code, null, null, 0, 0, 0, 0, message);
            }
            else
            {
                Log.LogError(null, code, null, null, 0, 0, 0, 0, message);
            }
        }

        private static bool IsEffectivelyEmpty(string path)
        {
            var info = new FileInfo(path);
            if (info.Length == 0) return true;
            if (info.Length > 4096) return false;
            string text = File.ReadAllText(path);
            return text.Trim().Length == 0;
        }

        /// SHA-256 of the file's normalized text: a leading UTF-8 BOM dropped and
        /// every CRLF turned into LF. Consumers reproducing a checksum apply the
        /// same two steps; see the README.
        private static string Sha256Hex(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            int start = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;
            var normalized = new MemoryStream(bytes.Length - start);
            for (int i = start; i < bytes.Length; i++)
            {
                if (bytes[i] == (byte)'\r' && i + 1 < bytes.Length && bytes[i + 1] == (byte)'\n')
                {
                    continue;
                }
                normalized.WriteByte(bytes[i]);
            }

            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(normalized.ToArray());
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
            sb.Append("  \"dialect\": ").Append(Quote(string.IsNullOrEmpty(Dialect) ? "generic" : Dialect.ToLowerInvariant())).Append(",\n");
            sb.Append("  \"convention\": ").Append(Quote(string.IsNullOrEmpty(Convention) ? "fluentmigrator" : Convention.ToLowerInvariant())).Append(",\n");
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
