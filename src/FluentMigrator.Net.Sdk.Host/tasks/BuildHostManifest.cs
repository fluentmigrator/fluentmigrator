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

// Compiled at build time by RoslynCodeTaskFactory. Same constraints as
// BuildSourceModel.cs: no LINQ, no System.Text.Json, deterministic output.
//
// Ordering contract: DeploymentTarget document order and (explicit or
// discovered) module order are THE contract; the module reference graph is
// only a linter. This task never reorders anything.
//
// The manifest has two axes, and they are independent:
//   hostContexts  how a command is invoked (argv, MSBuild task properties,
//                 an Aspire command payload, an MCP tool call, ...)
//   targets       what the command runs against, each carrying the runner
//                 package resolved from its dialect
// A consumer reads one manifest instead of rediscovering composition; see
// adr/proposed/FluentMigrator-Host-Design.md.
//
// Diagnostics:
//   FMSDK201 (warn)  declared module order contradicts the reference graph
//                    (suppress per target with AcknowledgeModuleOrder=true)
//   FMSDK202 (error) more than one root module in a target
//   FMSDK203 (error) root module present but not deployed first
//   FMSDK204 (error) target names a module that is not referenced
//   FMSDK205 (warn)  no root module: database creation/baseline unmanaged
//   FMSDK207 (error) unknown host context
//   FMSDK208 (warn)  no runner package registered for a target's dialect
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;

namespace FluentMigrator.Net.Sdk.Host.Tasks
{
    public sealed class BuildHostManifest : Microsoft.Build.Utilities.Task
    {
        [Required]
        public ITaskItem[] Targets { get; set; }

        public ITaskItem[] Modules { get; set; }

        public ITaskItem[] SelectedContexts { get; set; }

        public ITaskItem[] KnownContexts { get; set; }

        public ITaskItem[] RunnerPackages { get; set; }

        [Required]
        public string ProjectName { get; set; }

        [Required]
        public string ManifestPath { get; set; }

        public bool TreatValidationWarningsAsErrors { get; set; }

        private sealed class Module
        {
            public string Name;
            public bool IsRoot;
            public List<string> References = new List<string>();
            public string ManifestPath;
            public string AssemblyPath;
            public string DefaultSchema;
            public string Dialect;
            public string VersionTableName;
            public string VersionTableSchema;
        }

        private sealed class HostContext
        {
            public string Name;
            public string Package;
            public string ReferenceKind;
            public string Invocation;
            public string Availability;
            public string Description;
            public bool Discovery;
            public string Version;
        }

        public override bool Execute()
        {
            List<HostContext> contexts = ResolveContexts();

            // Preserve incoming (ProjectReference declaration) order.
            var moduleOrder = new List<string>();
            var modules = new Dictionary<string, Module>(StringComparer.OrdinalIgnoreCase);

            foreach (ITaskItem item in Modules ?? new ITaskItem[0])
            {
                var m = new Module();
                m.Name = item.ItemSpec;
                m.IsRoot = string.Equals(item.GetMetadata("IsRoot"), "true", StringComparison.OrdinalIgnoreCase);
                m.ManifestPath = item.GetMetadata("ManifestPath");
                m.AssemblyPath = item.GetMetadata("AssemblyPath");
                m.DefaultSchema = item.GetMetadata("DefaultSchema");
                m.Dialect = item.GetMetadata("Dialect");
                m.VersionTableName = item.GetMetadata("VersionTableName");
                m.VersionTableSchema = item.GetMetadata("VersionTableSchema");

                foreach (string r in (item.GetMetadata("References") ?? string.Empty).Split(';'))
                {
                    string trimmed = r.Trim();
                    if (trimmed.Length > 0)
                    {
                        m.References.Add(trimmed);
                    }
                }

                if (!modules.ContainsKey(m.Name))
                {
                    modules.Add(m.Name, m);
                    moduleOrder.Add(m.Name);
                }
            }

            var targetPlans = new List<KeyValuePair<ITaskItem, List<Module>>>();

            foreach (ITaskItem target in Targets ?? new ITaskItem[0])
            {
                List<Module> plan = ResolveModulePlan(target, modules, moduleOrder);
                if (plan == null)
                {
                    continue; // errors already logged
                }
                ValidatePlan(target, plan);
                targetPlans.Add(new KeyValuePair<ITaskItem, List<Module>>(target, plan));
            }

            if (Log.HasLoggedErrors)
            {
                return false;
            }

            WriteIfChanged(ManifestPath, WriteManifest(contexts, targetPlans));
            Log.LogMessage(MessageImportance.Normal,
                "Host manifest: {0} context(s), {1} target(s), {2} module(s) known.",
                contexts.Count, targetPlans.Count, moduleOrder.Count);
            return true;
        }

        /// Joins the user's HostContext selection onto the registry, failing
        /// on names no pack contributes. Output is sorted by name: the
        /// manifest is meant to be committed and diffed, and selection order
        /// carries no precedence semantics (unlike DeploymentTarget order).
        private List<HostContext> ResolveContexts()
        {
            var known = new Dictionary<string, ITaskItem>(StringComparer.OrdinalIgnoreCase);
            var knownNames = new List<string>();
            foreach (ITaskItem item in KnownContexts ?? new ITaskItem[0])
            {
                if (!known.ContainsKey(item.ItemSpec))
                {
                    known.Add(item.ItemSpec, item);
                    knownNames.Add(item.ItemSpec);
                }
            }
            knownNames.Sort(StringComparer.Ordinal);

            var resolved = new List<HostContext>();
            var seen = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            foreach (ITaskItem item in SelectedContexts ?? new ITaskItem[0])
            {
                string selectedName = (item.ItemSpec ?? string.Empty).Trim();
                if (selectedName.Length == 0)
                {
                    continue;
                }

                if (seen.ContainsKey(selectedName))
                {
                    continue;
                }
                seen.Add(selectedName, true);

                ITaskItem definition;
                if (!known.TryGetValue(selectedName, out definition))
                {
                    Log.LogError(null, "FMSDK207", null, null, 0, 0, 0, 0,
                        "Unknown host context '" + selectedName + "'. Known contexts: " +
                        string.Join(", ", knownNames.ToArray()) +
                        ". Contribute your own with a pack listed in $(CustomHostContextPacks)."
                    );
                    continue;
                }

                var ctx = new HostContext();
                ctx.Name = definition.ItemSpec;
                ctx.Package = definition.GetMetadata("Package");
                ctx.ReferenceKind = definition.GetMetadata("ReferenceKind");
                ctx.Invocation = definition.GetMetadata("Invocation");
                ctx.Availability = definition.GetMetadata("Availability");
                ctx.Description = definition.GetMetadata("Description");
                ctx.Discovery = string.Equals(definition.GetMetadata("Discovery"), "true", StringComparison.OrdinalIgnoreCase);
                ctx.Version = definition.GetMetadata("Version");
                resolved.Add(ctx);

                if (string.Equals(ctx.Availability, "planned", StringComparison.OrdinalIgnoreCase))
                {
                    // Not a warning: selecting a context the Host ADR describes
                    // but that has not shipped yet is a legitimate declaration.
                    // It lands in the manifest and adds no package reference.
                    Log.LogMessage(MessageImportance.Normal,
                        "Host context '{0}' is planned; recorded in the manifest, no package reference added.", ctx.Name);
                }
            }

            resolved.Sort(delegate (HostContext a, HostContext b)
            {
                return string.CompareOrdinal(a.Name, b.Name);
            });
            return resolved;
        }

        private string ResolveRunner(ITaskItem target, List<Module> plan)
        {
            string dialect = target.GetMetadata("Dialect");
            if (string.IsNullOrEmpty(dialect))
            {
                // A target without an explicit dialect inherits the root
                // module's (the first in the plan, per FMSDK203).
                for (int i = 0; i < plan.Count; i++)
                {
                    if (!string.IsNullOrEmpty(plan[i].Dialect))
                    {
                        dialect = plan[i].Dialect;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(dialect))
            {
                return string.Empty;
            }

            foreach (ITaskItem item in RunnerPackages ?? new ITaskItem[0])
            {
                if (string.Equals(item.ItemSpec, dialect, StringComparison.OrdinalIgnoreCase))
                {
                    return item.GetMetadata("Package");
                }
            }

            if (!string.Equals(dialect, "generic", StringComparison.OrdinalIgnoreCase))
            {
                Report("FMSDK208",
                    "DeploymentTarget '" + target.ItemSpec + "' uses dialect '" + dialect +
                    "', for which no runner package is registered. The manifest records no runner for it; " +
                    "declare a MigrationRunnerPackage item to map it.");
            }
            return string.Empty;
        }

        private List<Module> ResolveModulePlan(ITaskItem target, Dictionary<string, Module> modules, List<string> moduleOrder)
        {
            var plan = new List<Module>();
            string explicitList = target.GetMetadata("Modules");

            if (!string.IsNullOrEmpty(explicitList))
            {
                foreach (string raw in explicitList.Split(';'))
                {
                    string name = raw.Trim();
                    if (name.Length == 0)
                    {
                        continue;
                    }
                    Module m;
                    if (!modules.TryGetValue(name, out m))
                    {
                        Log.LogError(null, "FMSDK204", null, null, 0, 0, 0, 0,
                            "DeploymentTarget '" + target.ItemSpec + "' names module '" + name +
                            "', which is not a referenced FluentMigrator module. Known modules: " +
                            string.Join(", ", moduleOrder.ToArray()) + ".");
                        return null;
                    }
                    plan.Add(m);
                }
            }
            else
            {
                // Discovery: all known modules, root hoisted first, remainder
                // in ProjectReference declaration order.
                foreach (string name in moduleOrder)
                {
                    Module m = modules[name];
                    if (m.IsRoot)
                    {
                        plan.Insert(0, m);
                    }
                    else
                    {
                        plan.Add(m);
                    }
                }
            }

            return plan;
        }

        private void ValidatePlan(ITaskItem target, List<Module> plan)
        {
            int rootCount = 0;
            int rootIndex = -1;
            for (int i = 0; i < plan.Count; i++)
            {
                if (plan[i].IsRoot)
                {
                    rootCount++;
                    if (rootIndex < 0)
                    {
                        rootIndex = i;
                    }
                }
            }

            if (rootCount > 1)
            {
                Log.LogError(null, "FMSDK202", null, null, 0, 0, 0, 0,
                    "DeploymentTarget '" + target.ItemSpec + "' contains " + rootCount +
                    " root modules; a database target must have exactly one root.");
            }
            else if (rootCount == 1 && rootIndex != 0)
            {
                Log.LogError(null, "FMSDK203", null, null, 0, 0, 0, 0,
                    "DeploymentTarget '" + target.ItemSpec + "': root module '" + plan[rootIndex].Name +
                    "' must deploy first, but is at position " + (rootIndex + 1) + ".");
            }
            else if (rootCount == 0)
            {
                Report("FMSDK205",
                    "DeploymentTarget '" + target.ItemSpec + "' has no root module; database creation " +
                    "and the baseline are unmanaged. Mark one module with IsDatabaseRootModule=true.");
            }

            // Graph-as-linter: declared order is the contract; contradiction
            // with the reference graph is a warning, not a reorder.
            bool acknowledged = string.Equals(target.GetMetadata("AcknowledgeModuleOrder"), "true", StringComparison.OrdinalIgnoreCase);
            if (!acknowledged)
            {
                for (int i = 0; i < plan.Count; i++)
                {
                    for (int j = i + 1; j < plan.Count; j++)
                    {
                        if (References(plan[i], plan[j].Name))
                        {
                            Report("FMSDK201",
                                "DeploymentTarget '" + target.ItemSpec + "': '" + plan[i].Name +
                                "' deploys before '" + plan[j].Name + "' but references it. Declared order wins; " +
                                "set AcknowledgeModuleOrder=true on the target if this is intentional.");
                        }
                    }
                }
            }
        }

        private static bool References(Module from, string toName)
        {
            for (int i = 0; i < from.References.Count; i++)
            {
                if (string.Equals(from.References[i], toName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private void Report(string code, string message)
        {
            if (TreatValidationWarningsAsErrors)
            {
                Log.LogError(null, code, null, null, 0, 0, 0, 0, message);
            }
            else
            {
                Log.LogWarning(null, code, null, null, 0, 0, 0, 0, message);
            }
        }

        private string WriteManifest(List<HostContext> contexts, List<KeyValuePair<ITaskItem, List<Module>>> targetPlans)
        {
            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("  \"manifestVersion\": 1,\n");
            sb.Append("  \"project\": ").Append(Quote(ProjectName)).Append(",\n");

            sb.Append("  \"hostContexts\": [");
            for (int c = 0; c < contexts.Count; c++)
            {
                HostContext ctx = contexts[c];
                sb.Append(c == 0 ? "\n" : ",\n");
                sb.Append("    {\n");
                sb.Append("      \"name\": ").Append(Quote(ctx.Name)).Append(",\n");
                sb.Append("      \"package\": ").Append(Quote(ctx.Package ?? string.Empty)).Append(",\n");
                sb.Append("      \"referenceKind\": ").Append(Quote(ctx.ReferenceKind ?? string.Empty)).Append(",\n");
                sb.Append("      \"invocation\": ").Append(Quote(ctx.Invocation ?? string.Empty)).Append(",\n");
                sb.Append("      \"availability\": ").Append(Quote(ctx.Availability ?? string.Empty)).Append(",\n");
                sb.Append("      \"discovery\": ").Append(ctx.Discovery ? "true" : "false");
                if (!string.IsNullOrEmpty(ctx.Version))
                {
                    sb.Append(",\n      \"version\": ").Append(Quote(ctx.Version));
                }
                sb.Append("\n    }");
            }
            sb.Append(contexts.Count == 0 ? "],\n" : "\n  ],\n");

            sb.Append("  \"targets\": [");

            for (int t = 0; t < targetPlans.Count; t++)
            {
                ITaskItem target = targetPlans[t].Key;
                List<Module> plan = targetPlans[t].Value;

                sb.Append(t == 0 ? "\n" : ",\n");
                sb.Append("    {\n");
                sb.Append("      \"name\": ").Append(Quote(target.ItemSpec)).Append(",\n");
                AppendOptional(sb, "connectionStringName", target.GetMetadata("ConnectionStringName"));
                AppendOptional(sb, "dialect", target.GetMetadata("Dialect"));
                AppendOptional(sb, "runner", ResolveRunner(target, plan));
                AppendOptional(sb, "tags", target.GetMetadata("Tags"));
                sb.Append("      \"modules\": [");

                for (int m = 0; m < plan.Count; m++)
                {
                    Module mod = plan[m];
                    sb.Append(m == 0 ? "\n" : ",\n");
                    sb.Append("        {\n");
                    sb.Append("          \"name\": ").Append(Quote(mod.Name)).Append(",\n");
                    if (mod.IsRoot)
                    {
                        sb.Append("          \"role\": \"root\",\n");
                    }
                    sb.Append("          \"defaultSchema\": ").Append(Quote(mod.DefaultSchema ?? string.Empty)).Append(",\n");
                    sb.Append("          \"versionTable\": {")
                      .Append("\"schema\": ").Append(Quote(mod.VersionTableSchema ?? string.Empty))
                      .Append(", \"name\": ").Append(Quote(mod.VersionTableName ?? string.Empty))
                      .Append("},\n");
                    sb.Append("          \"sourceModel\": ").Append(Quote(FileNameOnly(mod.ManifestPath))).Append(",\n");
                    sb.Append("          \"assembly\": ").Append(Quote(FileNameOnly(mod.AssemblyPath))).Append("\n");
                    sb.Append("        }");
                }

                sb.Append(plan.Count == 0 ? "]\n" : "\n      ]\n");
                sb.Append("    }");
            }

            sb.Append(targetPlans.Count == 0 ? "]\n" : "\n  ]\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        private static void AppendOptional(StringBuilder sb, string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                sb.Append("      \"").Append(key).Append("\": ").Append(Quote(value)).Append(",\n");
            }
        }

        private static string FileNameOnly(string path)
        {
            return string.IsNullOrEmpty(path) ? string.Empty : Path.GetFileName(path);
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
