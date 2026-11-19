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
// The lock file is the committed copy of the source model manifest, the way
// packages.lock.json is the committed copy of a restore. The manifest is a
// build output in obj/ and bin/ with an incremental up-to-date check; the lock
// sits in the source tree, where git, merges and hand edits change it behind
// MSBuild's back, so none of that machinery can be trusted for it:
//
//  - It is compared on every build, by content, never by timestamp. A hand
//    edit or a mis-resolved merge makes the lock the newest file in the tree,
//    which an up-to-date check would read as "nothing to do".
//  - The comparison ignores line endings and a UTF-8 BOM, so a checkout with
//    core.autocrlf doesn't look like a change. The lock is written with LF.
//  - There is one writer per project. With several target frameworks, every
//    framework's manifest must be identical (FMSDK005): a lock whose content
//    depended on which framework built last would be meaningless.
//  - Locked mode (CI) never writes. A missing or different lock is FMSDK004,
//    with a summary of what differs, rather than a build that quietly fixes
//    the drift and passes.
//
// Diagnostics:
//   FMSDK004 (error) locked mode: the lock is missing or differs from the build
//   FMSDK005 (error) target frameworks produced different manifests
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;

namespace FluentMigrator.Net.Sdk.Tasks
{
    public sealed class SourceModelLock : Microsoft.Build.Utilities.Task
    {
        /// One manifest per target framework, each with TargetFramework metadata.
        [Required]
        public ITaskItem[] Manifests { get; set; }

        [Required]
        public string LockPath { get; set; }

        public bool LockedMode { get; set; }

        /// The explicit writer (-t:RewriteSourceModelLock): writes even in locked mode.
        public bool Rewrite { get; set; }

        public override bool Execute()
        {
            if (Manifests == null || Manifests.Length == 0)
            {
                return true;
            }

            string expected = null;
            string expectedFramework = null;
            foreach (ITaskItem manifest in Manifests)
            {
                string content = Normalize(File.ReadAllText(manifest.ItemSpec));
                string framework = manifest.GetMetadata("TargetFramework");
                if (expected == null)
                {
                    expected = content;
                    expectedFramework = framework;
                }
                else if (!string.Equals(expected, content, StringComparison.Ordinal))
                {
                    Log.LogError(null, "FMSDK005", null, null, 0, 0, 0, 0,
                        "The source model differs between target frameworks '" + expectedFramework + "' and '" + framework +
                        "', so there is no single lock file for this project: " + Summarize(expected, content) +
                        ". Make the source model items the same for every target framework.");
                    return false;
                }
            }

            string lockPath = Path.GetFullPath(LockPath);
            bool exists = File.Exists(lockPath);
            string current = exists ? Normalize(File.ReadAllText(lockPath)) : null;

            if (exists && string.Equals(current, expected, StringComparison.Ordinal))
            {
                Log.LogMessage(MessageImportance.Low, "Source model lock is up to date: {0}", lockPath);
                return true;
            }

            if (LockedMode && !Rewrite)
            {
                Log.LogError(null, "FMSDK004", null, lockPath, 0, 0, 0, 0,
                    exists
                        ? "The source model lock file is out of date: " + Summarize(current, expected) +
                          ". SourceModelLockedMode doesn't update it; run 'dotnet build -t:RewriteSourceModelLock' and commit the result."
                        : "SourceModelLockedMode is set but the source model lock file '" + lockPath + "' doesn't exist. " +
                          "Run 'dotnet build -t:RewriteSourceModelLock' and commit the result.");
                return false;
            }

            string dir = Path.GetDirectoryName(lockPath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(lockPath, expected, new UTF8Encoding(false));
            Log.LogMessage(MessageImportance.High, "{0} source model lock file: {1}", exists ? "Updated" : "Created", lockPath);
            return true;
        }

        /// LF line endings, no BOM: the form the lock is written in, and the
        /// form both sides are compared in.
        private static string Normalize(string text)
        {
            if (text.Length > 0 && text[0] == '﻿')
            {
                text = text.Substring(1);
            }
            return text.Replace("\r\n", "\n");
        }

        /// Which objects (by path) were added, removed or changed between two
        /// manifests, and whether the header (dialect, schema, ...) changed.
        /// Relies on the manifest's fixed layout: one object per block, opened
        /// by a line "    {" and closed by "    }".
        private static string Summarize(string before, string after)
        {
            string headerBefore;
            string headerAfter;
            Dictionary<string, string> objectsBefore = Objects(before, out headerBefore);
            Dictionary<string, string> objectsAfter = Objects(after, out headerAfter);

            var parts = new List<string>();
            if (!string.Equals(headerBefore, headerAfter, StringComparison.Ordinal))
            {
                parts.Add("module properties changed");
            }

            AddPaths(parts, "added", objectsAfter, objectsBefore, false);
            AddPaths(parts, "removed", objectsBefore, objectsAfter, false);
            AddPaths(parts, "changed", objectsAfter, objectsBefore, true);

            return parts.Count == 0 ? "formatting differs" : string.Join("; ", parts.ToArray());
        }

        private static void AddPaths(List<string> parts, string label, Dictionary<string, string> from,
            Dictionary<string, string> other, bool changedOnly)
        {
            var paths = new List<string>();
            foreach (KeyValuePair<string, string> kv in from)
            {
                string otherBlock;
                bool inOther = other.TryGetValue(kv.Key, out otherBlock);
                if (changedOnly ? inOther && !string.Equals(kv.Value, otherBlock, StringComparison.Ordinal) : !inOther)
                {
                    paths.Add(kv.Key);
                }
            }
            if (paths.Count == 0)
            {
                return;
            }
            paths.Sort(StringComparer.Ordinal);
            const int Shown = 5;
            var shown = new List<string>();
            for (int i = 0; i < paths.Count && i < Shown; i++)
            {
                shown.Add(paths[i]);
            }
            string more = paths.Count > Shown ? " and " + (paths.Count - Shown) + " more" : string.Empty;
            parts.Add(label + " " + string.Join(", ", shown.ToArray()) + more);
        }

        private static Dictionary<string, string> Objects(string manifest, out string header)
        {
            var objects = new Dictionary<string, string>(StringComparer.Ordinal);
            var headerBuilder = new StringBuilder();
            StringBuilder block = null;
            string path = null;
            bool inObjects = false;

            foreach (string line in manifest.Split('\n'))
            {
                if (line.StartsWith("  \"objects\"", StringComparison.Ordinal))
                {
                    inObjects = true;
                    continue;
                }
                if (!inObjects)
                {
                    headerBuilder.Append(line).Append('\n');
                    continue;
                }
                if (line == "    {")
                {
                    block = new StringBuilder();
                    path = null;
                    continue;
                }
                if (block == null)
                {
                    continue;
                }
                if (line == "    }" || line == "    },")
                {
                    objects[path ?? ("#" + objects.Count)] = block.ToString();
                    block = null;
                    continue;
                }
                block.Append(line).Append('\n');
                string trimmed = line.Trim();
                if (trimmed.StartsWith("\"path\": \"", StringComparison.Ordinal))
                {
                    int start = "\"path\": \"".Length;
                    int end = trimmed.LastIndexOf('"');
                    path = end > start ? trimmed.Substring(start, end - start) : trimmed;
                }
            }

            header = headerBuilder.ToString();
            return objects;
        }
    }
}
