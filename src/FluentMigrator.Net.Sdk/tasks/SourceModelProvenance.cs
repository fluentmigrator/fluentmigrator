#region License
//
// Copyright (c) 2026, Fluent Migrator Project
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

// Compiled at build time by RoslynCodeTaskFactory. Same constraints as
// BuildSourceModel.cs: no LINQ, no System.Text.Json.
//
// The provenance file records how a source model manifest was produced, next
// to the manifest and separate from it: the manifest says what the database
// model is, the provenance says where that claim came from. It carries the
// manifest's SHA-256, so a manifest altered after the build no longer matches
// its provenance, and the Host SDK and VerifySourceModel check exactly that.
//
// Two formats, chosen by SourceModelProvenanceFormat:
//   slsa    (default) an in-toto Statement v1 with a SLSA Provenance v1
//           predicate, which supply-chain tooling already understands
//   simple  the same facts as a small flat document
//
// The file is deterministic: no timestamps, no machine names, no absolute
// paths. The same commit, inputs and global properties give the same bytes,
// which is what lets VerifySourceModel regenerate it and compare.
//
// What "how it was generated" means here: MSBuild doesn't expose the command
// line, but the global properties are exactly the -p: values (and what a
// parent build passed down). Those are recorded, minus build-host noise
// (NuGetInteractive, solution and Visual Studio bookkeeping, anything starting
// with '_') and minus any whose value is an absolute path. Configuration and
// TargetFramework are recorded as fields of their own.
//
// Source control comes from SourceModelProvenanceSource, one item that the
// CollectSourceModelProvenanceSource targets produce: the .NET SDK's
// SourceLink data for git by default, or a user-supplied target for any other
// VCS. Its metadata: Vcs, RepositoryUrl, Ref, Revision, DigestName (the
// in-toto digest key, gitCommit for git), RootPath, WorkingTree.
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;

namespace FluentMigrator.Net.Sdk.Tasks
{
    public sealed class WriteSourceModelProvenance : Microsoft.Build.Utilities.Task
    {
        /// The manifest this provenance describes; hashed as it is on disk.
        [Required]
        public string ManifestPath { get; set; }

        /// The manifest's name as recorded (the file name it ships under).
        [Required]
        public string ManifestName { get; set; }

        [Required]
        public string ProvenancePath { get; set; }

        /// slsa (default) or simple.
        public string Format { get; set; }

        [Required]
        public string ProjectPath { get; set; }

        public string Configuration { get; set; }

        public string TargetFramework { get; set; }

        public string Dialect { get; set; }

        public string DefaultSchema { get; set; }

        public string Convention { get; set; }

        public bool IsRootModule { get; set; }

        public string SourceModelRoot { get; set; }

        public string BuilderId { get; set; }

        /// Zero or one item; see the header comment for its metadata.
        public ITaskItem[] Source { get; set; }

        /// Paths relative to the source root that differ from the revision. An
        /// item with GitPorcelain="true" is a `git status --porcelain` line.
        public ITaskItem[] ModifiedFiles { get; set; }

        /// More global property names to leave out, beyond the built-in list.
        public string[] ExcludedGlobalProperties { get; set; }

        private static readonly string[] NoiseGlobalProperties =
        {
            "Configuration", "TargetFramework",
            "NuGetInteractive", "MSBuildRestoreSessionId", "ExcludeRestorePackageImports",
            "RestoreUseStaticGraphEvaluation", "BuildingSolutionFile", "BuildingInsideVisualStudio",
            "BuildingProject", "CurrentSolutionConfigurationContents", "DevEnvDir", "VisualStudioVersion",
            "SolutionDir", "SolutionExt", "SolutionFileName", "SolutionName", "SolutionPath",
        };

        public override bool Execute()
        {
            string format = string.IsNullOrEmpty(Format) ? "slsa" : Format.Trim().ToLowerInvariant();
            if (format != "slsa" && format != "simple")
            {
                Log.LogError(null, "FMSDK010", null, null, 0, 0, 0, 0,
                    "SourceModelProvenanceFormat '" + Format + "' is not a provenance format. Use slsa (the default) or simple.");
                return false;
            }

            var p = new Provenance();
            p.ManifestName = ManifestName;
            p.ManifestSha256 = Sha256Hex(ManifestPath);
            p.BuilderId = BuilderId ?? string.Empty;
            p.Configuration = Configuration ?? string.Empty;
            p.TargetFramework = TargetFramework ?? string.Empty;
            p.GlobalProperties = RecordedGlobalProperties();

            ITaskItem source = Source != null && Source.Length > 0 ? Source[0] : null;
            string sourceRoot = source == null ? null : source.GetMetadata("RootPath");
            p.Project = RelativeOrName(sourceRoot, ProjectPath);
            p.SourceModelRoot = string.IsNullOrEmpty(sourceRoot) || string.IsNullOrEmpty(SourceModelRoot)
                ? null
                : RelativeDirectory(sourceRoot, SourceModelRoot);
            p.Dialect = Dialect ?? string.Empty;
            p.DefaultSchema = DefaultSchema ?? string.Empty;
            p.Convention = Convention ?? string.Empty;
            p.IsRootModule = IsRootModule;

            if (source != null)
            {
                p.Vcs = source.GetMetadata("Vcs");
                if (string.IsNullOrEmpty(p.Vcs))
                {
                    p.Vcs = source.ItemSpec;
                }
                p.RepositoryUrl = source.GetMetadata("RepositoryUrl");
                p.Ref = source.GetMetadata("Ref");
                p.Revision = source.GetMetadata("Revision");
                p.DigestName = source.GetMetadata("DigestName");
                if (string.IsNullOrEmpty(p.DigestName))
                {
                    p.DigestName = p.Vcs == "git" ? "gitCommit" : "revision";
                }
                p.ModifiedFiles = CollectModifiedFiles();
                p.WorkingTree = source.GetMetadata("WorkingTree");
                if (string.IsNullOrEmpty(p.WorkingTree))
                {
                    p.WorkingTree = "unchecked";
                }
                else if (p.WorkingTree == "checked")
                {
                    p.WorkingTree = p.ModifiedFiles.Count == 0 ? "clean" : "modified";
                }
            }

            string json = format == "slsa" ? WriteSlsa(p) : WriteSimple(p);
            WriteIfChanged(ProvenancePath, json);
            return !Log.HasLoggedErrors;
        }

        private sealed class Provenance
        {
            public string ManifestName;
            public string ManifestSha256;
            public string BuilderId;
            public string Project;
            public string Configuration;
            public string TargetFramework;
            public SortedDictionary<string, string> GlobalProperties;
            public string Dialect;
            public string DefaultSchema;
            public string Convention;
            public bool IsRootModule;
            public string SourceModelRoot;
            public string Vcs;
            public string RepositoryUrl;
            public string Ref;
            public string Revision;
            public string DigestName;
            public string WorkingTree;
            public List<string> ModifiedFiles = new List<string>();
        }

        private SortedDictionary<string, string> RecordedGlobalProperties()
        {
            var recorded = new SortedDictionary<string, string>(StringComparer.Ordinal);
            var engine = BuildEngine as IBuildEngine6;
            if (engine == null)
            {
                return recorded;
            }

            var excluded = new HashSet<string>(NoiseGlobalProperties, StringComparer.OrdinalIgnoreCase);
            foreach (string name in ExcludedGlobalProperties ?? new string[0])
            {
                excluded.Add(name.Trim());
            }

            foreach (KeyValuePair<string, string> pair in engine.GetGlobalProperties())
            {
                if (excluded.Contains(pair.Key)
                    || pair.Key.StartsWith("_", StringComparison.Ordinal)
                    || pair.Key.StartsWith("SourceModelVerify", StringComparison.OrdinalIgnoreCase)
                    || HasAbsolutePath(pair.Value))
                {
                    continue;
                }
                recorded[pair.Key] = pair.Value;
            }
            return recorded;
        }

        private static bool HasAbsolutePath(string value)
        {
            foreach (string part in (value ?? string.Empty).Split(';'))
            {
                string trimmed = part.Trim();
                if (trimmed.Length > 0 && trimmed.IndexOfAny(Path.GetInvalidPathChars()) < 0 && Path.IsPathRooted(trimmed))
                {
                    return true;
                }
            }
            return false;
        }

        private static readonly Regex PorcelainLine = new Regex(@"^\s*[ MTADRCU?!]{1,2}\s(.*)$");

        private List<string> CollectModifiedFiles()
        {
            var files = new SortedSet<string>(StringComparer.Ordinal);
            foreach (ITaskItem item in ModifiedFiles ?? new ITaskItem[0])
            {
                string path = item.ItemSpec;
                if (string.Equals(item.GetMetadata("GitPorcelain"), "true", StringComparison.OrdinalIgnoreCase))
                {
                    Match m = PorcelainLine.Match(path);
                    if (!m.Success)
                    {
                        continue;
                    }
                    path = m.Groups[1].Value;
                    int arrow = path.IndexOf(" -> ", StringComparison.Ordinal);
                    if (arrow >= 0)
                    {
                        path = path.Substring(arrow + 4);
                    }
                    path = path.Trim();
                    if (path.Length > 1 && path[0] == '"' && path[path.Length - 1] == '"')
                    {
                        path = path.Substring(1, path.Length - 2);
                    }
                }
                path = path.Replace('\\', '/').Trim();
                if (path.Length > 0)
                {
                    files.Add(path);
                }
            }
            return new List<string>(files);
        }

        private static string WriteSlsa(Provenance p)
        {
            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("  \"_type\": \"https://in-toto.io/Statement/v1\",\n");
            sb.Append("  \"subject\": [\n");
            sb.Append("    {\n");
            sb.Append("      \"name\": ").Append(Quote(p.ManifestName)).Append(",\n");
            sb.Append("      \"digest\": {\"sha256\": ").Append(Quote(p.ManifestSha256)).Append("}\n");
            sb.Append("    }\n");
            sb.Append("  ],\n");
            sb.Append("  \"predicateType\": \"https://slsa.dev/provenance/v1\",\n");
            sb.Append("  \"predicate\": {\n");
            sb.Append("    \"buildDefinition\": {\n");
            sb.Append("      \"buildType\": ").Append(Quote(BuildType)).Append(",\n");
            sb.Append("      \"externalParameters\": {\n");
            sb.Append("        \"project\": ").Append(Quote(p.Project)).Append(",\n");
            sb.Append("        \"configuration\": ").Append(Quote(p.Configuration)).Append(",\n");
            sb.Append("        \"targetFramework\": ").Append(Quote(p.TargetFramework)).Append(",\n");
            sb.Append("        \"globalProperties\": ");
            AppendMap(sb, p.GlobalProperties, "        ");
            sb.Append("\n      },\n");
            sb.Append("      \"internalParameters\": {\n");
            AppendParameters(sb, p, "        ");
            sb.Append("      },\n");
            sb.Append("      \"resolvedDependencies\": [");
            if (p.Vcs == null)
            {
                sb.Append("]\n");
            }
            else
            {
                sb.Append("\n        {\n");
                if (!string.IsNullOrEmpty(p.RepositoryUrl))
                {
                    string uri = p.Vcs + "+" + p.RepositoryUrl + (string.IsNullOrEmpty(p.Ref) ? string.Empty : "@" + p.Ref);
                    sb.Append("          \"uri\": ").Append(Quote(uri)).Append(",\n");
                }
                sb.Append("          \"digest\": {").Append(Quote(p.DigestName)).Append(": ").Append(Quote(p.Revision)).Append("},\n");
                sb.Append("          \"annotations\": {");
                var annotations = new List<string>();
                if (string.IsNullOrEmpty(p.RepositoryUrl))
                {
                    annotations.Add("\"vcs\": " + Quote(p.Vcs));
                    if (!string.IsNullOrEmpty(p.Ref))
                    {
                        annotations.Add("\"ref\": " + Quote(p.Ref));
                    }
                }
                annotations.Add("\"workingTree\": " + Quote(p.WorkingTree));
                if (p.ModifiedFiles.Count > 0)
                {
                    annotations.Add("\"modifiedFiles\": " + List(p.ModifiedFiles));
                }
                sb.Append(string.Join(", ", annotations.ToArray())).Append("}\n");
                sb.Append("        }\n");
                sb.Append("      ]\n");
            }
            sb.Append("    },\n");
            sb.Append("    \"runDetails\": {\n");
            sb.Append("      \"builder\": {\"id\": ").Append(Quote(p.BuilderId)).Append("}\n");
            sb.Append("    }\n");
            sb.Append("  }\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        private static string WriteSimple(Provenance p)
        {
            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("  \"format\": ").Append(Quote(BuildType)).Append(",\n");
            sb.Append("  \"manifest\": {\"name\": ").Append(Quote(p.ManifestName))
              .Append(", \"sha256\": ").Append(Quote(p.ManifestSha256)).Append("},\n");
            sb.Append("  \"builder\": ").Append(Quote(p.BuilderId)).Append(",\n");
            sb.Append("  \"project\": ").Append(Quote(p.Project)).Append(",\n");
            sb.Append("  \"configuration\": ").Append(Quote(p.Configuration)).Append(",\n");
            sb.Append("  \"targetFramework\": ").Append(Quote(p.TargetFramework)).Append(",\n");
            sb.Append("  \"globalProperties\": ");
            AppendMap(sb, p.GlobalProperties, "  ");
            sb.Append(",\n");
            sb.Append("  \"parameters\": {\n");
            AppendParameters(sb, p, "    ");
            sb.Append("  }");
            if (p.Vcs != null)
            {
                sb.Append(",\n  \"source\": {\n");
                sb.Append("    \"vcs\": ").Append(Quote(p.Vcs)).Append(",\n");
                if (!string.IsNullOrEmpty(p.RepositoryUrl))
                {
                    sb.Append("    \"repository\": ").Append(Quote(p.RepositoryUrl)).Append(",\n");
                }
                if (!string.IsNullOrEmpty(p.Ref))
                {
                    sb.Append("    \"ref\": ").Append(Quote(p.Ref)).Append(",\n");
                }
                sb.Append("    \"revision\": ").Append(Quote(p.Revision)).Append(",\n");
                sb.Append("    \"workingTree\": ").Append(Quote(p.WorkingTree));
                if (p.ModifiedFiles.Count > 0)
                {
                    sb.Append(",\n    \"modifiedFiles\": ").Append(List(p.ModifiedFiles));
                }
                sb.Append("\n  }");
            }
            sb.Append("\n}\n");
            return sb.ToString();
        }

        /// The document describing both formats; the in-toto buildType and the
        /// simple format's "format" field both point at it.
        private const string BuildType =
            "https://github.com/fluentmigrator/fluentmigrator/blob/main/src/FluentMigrator.Net.Sdk/README.md#provenance";

        private static void AppendParameters(StringBuilder sb, Provenance p, string indent)
        {
            sb.Append(indent).Append("\"dialect\": ").Append(Quote(p.Dialect)).Append(",\n");
            sb.Append(indent).Append("\"defaultSchema\": ").Append(Quote(p.DefaultSchema)).Append(",\n");
            sb.Append(indent).Append("\"convention\": ").Append(Quote(p.Convention)).Append(",\n");
            sb.Append(indent).Append("\"isRootModule\": ").Append(p.IsRootModule ? "true" : "false");
            if (p.SourceModelRoot != null)
            {
                sb.Append(",\n").Append(indent).Append("\"sourceModelRoot\": ").Append(Quote(p.SourceModelRoot));
            }
            sb.Append("\n");
        }

        private static void AppendMap(StringBuilder sb, SortedDictionary<string, string> map, string indent)
        {
            if (map.Count == 0)
            {
                sb.Append("{}");
                return;
            }
            sb.Append("{\n");
            bool first = true;
            foreach (KeyValuePair<string, string> pair in map)
            {
                if (!first)
                {
                    sb.Append(",\n");
                }
                first = false;
                sb.Append(indent).Append("  ").Append(Quote(pair.Key)).Append(": ").Append(Quote(pair.Value));
            }
            sb.Append("\n").Append(indent).Append("}");
        }

        private static string List(List<string> values)
        {
            var quoted = new string[values.Count];
            for (int i = 0; i < values.Count; i++)
            {
                quoted[i] = Quote(values[i]);
            }
            return "[" + string.Join(", ", quoted) + "]";
        }

        /// A path relative to the source root with '/' separators, or the bare
        /// file name when there is no source root or the path is outside it:
        /// the provenance never records an absolute path.
        private static string RelativeOrName(string sourceRoot, string path)
        {
            string full = Path.GetFullPath(path);
            if (!string.IsNullOrEmpty(sourceRoot))
            {
                string root = Path.GetFullPath(sourceRoot);
                if (!root.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    root += Path.DirectorySeparatorChar;
                }
                if (full.StartsWith(root, PathComparison))
                {
                    return full.Substring(root.Length).Replace('\\', '/');
                }
            }
            return Path.GetFileName(full);
        }

        private static string RelativeDirectory(string sourceRoot, string directory)
        {
            string full = Path.GetFullPath(directory).TrimEnd('\\', '/');
            string root = Path.GetFullPath(sourceRoot).TrimEnd('\\', '/');
            if (string.Equals(full, root, PathComparison))
            {
                return ".";
            }
            if (full.StartsWith(root + Path.DirectorySeparatorChar, PathComparison))
            {
                return full.Substring(root.Length + 1).Replace('\\', '/');
            }
            return null;
        }

        private static readonly StringComparison PathComparison =
            Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        private static string Sha256Hex(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                byte[] hash = sha.ComputeHash(stream);
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        private static void WriteIfChanged(string path, string content)
        {
            if (File.Exists(path) && File.ReadAllText(path) == content)
            {
                return;
            }
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(path, content, new UTF8Encoding(false));
        }

        private static string Quote(string value)
        {
            var sb = new StringBuilder("\"");
            foreach (char c in value ?? string.Empty)
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
            return sb.Append('"').ToString();
        }
    }
}
