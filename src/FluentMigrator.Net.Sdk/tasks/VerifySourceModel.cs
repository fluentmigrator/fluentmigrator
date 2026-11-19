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
// Checks a manifest and its provenance against this checkout. The
// VerifySourceModel target regenerates both from the checkout, with the
// global properties of the verify invocation, and this task compares:
//
//   FMSDK006 (error) the manifest or its provenance is missing
//   FMSDK007 (error) the manifest's SHA-256 isn't the one its provenance
//                    records: the manifest changed after the build
//   FMSDK008 (error) this checkout doesn't produce this manifest
//   FMSDK009 (error) this checkout doesn't produce this provenance: another
//                    revision, other global properties, another format, or a
//                    different working tree state
//
// Both documents are deterministic, so "produces" means byte for byte. No
// JSON parsing is needed beyond finding the first "sha256" value, which in
// both formats is the manifest's.
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;

namespace FluentMigrator.Net.Sdk.Tasks
{
    public sealed class VerifySourceModel : Microsoft.Build.Utilities.Task
    {
        [Required]
        public string ManifestPath { get; set; }

        [Required]
        public string ProvenancePath { get; set; }

        [Required]
        public string RegeneratedManifestPath { get; set; }

        [Required]
        public string RegeneratedProvenancePath { get; set; }

        private static readonly Regex Sha256Value = new Regex("\"sha256\":\\s*\"([0-9a-f]{64})\"");

        public override bool Execute()
        {
            if (!File.Exists(ManifestPath))
            {
                Log.LogError(null, "FMSDK006", null, ManifestPath, 0, 0, 0, 0,
                    "There is no source model manifest at '" + ManifestPath + "' to verify.");
                return false;
            }
            if (!File.Exists(ProvenancePath))
            {
                Log.LogError(null, "FMSDK006", null, ManifestPath, 0, 0, 0, 0,
                    "The manifest has no provenance: '" + ProvenancePath + "' doesn't exist. A manifest without " +
                    "its provenance can't be verified.");
                return false;
            }

            string provenance = File.ReadAllText(ProvenancePath);
            Match recorded = Sha256Value.Match(provenance);
            string actual = Sha256Hex(ManifestPath);
            if (!recorded.Success || recorded.Groups[1].Value != actual)
            {
                Log.LogError(null, "FMSDK007", null, ManifestPath, 0, 0, 0, 0,
                    "The manifest's SHA-256 is " + actual + ", but its provenance records " +
                    (recorded.Success ? recorded.Groups[1].Value : "none") +
                    ". The manifest was changed after the build that wrote the provenance.");
                return false;
            }

            string manifest = File.ReadAllText(ManifestPath);
            string regeneratedManifest = File.ReadAllText(RegeneratedManifestPath);
            if (manifest != regeneratedManifest)
            {
                Log.LogError(null, "FMSDK008", null, ManifestPath, 0, 0, 0, 0,
                    "This checkout doesn't produce the manifest being verified. " +
                    Differences(manifest, regeneratedManifest, "manifest", "checkout") +
                    " Check out the revision its provenance records, and pass the same -p: values it records.");
                return false;
            }

            string regeneratedProvenance = File.ReadAllText(RegeneratedProvenancePath);
            if (provenance != regeneratedProvenance)
            {
                Log.LogError(null, "FMSDK009", null, ProvenancePath, 0, 0, 0, 0,
                    "The manifest matches this checkout, but its provenance doesn't. " +
                    Differences(provenance, regeneratedProvenance, "recorded", "checkout") +
                    " The revision, the global properties, the format or the working tree state differ.");
                return false;
            }

            Log.LogMessage(MessageImportance.High,
                "Verified: this checkout produces '" + Path.GetFileName(ManifestPath) + "' and its provenance.");
            return true;
        }

        /// The differing lines, a few at most, as "recorded: ... / checkout: ...".
        private static string Differences(string expected, string actual, string expectedLabel, string actualLabel)
        {
            string[] a = expected.Replace("\r\n", "\n").Split('\n');
            string[] b = actual.Replace("\r\n", "\n").Split('\n');
            var onlyA = new List<string>();
            var onlyB = new List<string>();
            var inB = new HashSet<string>(b, StringComparer.Ordinal);
            var inA = new HashSet<string>(a, StringComparer.Ordinal);
            foreach (string line in a)
            {
                if (!inB.Contains(line) && line.Trim().Length > 0) onlyA.Add(line.Trim().TrimEnd(','));
            }
            foreach (string line in b)
            {
                if (!inA.Contains(line) && line.Trim().Length > 0) onlyB.Add(line.Trim().TrimEnd(','));
            }

            var sb = new StringBuilder();
            AppendLines(sb, expectedLabel, onlyA);
            AppendLines(sb, actualLabel, onlyB);
            return sb.Length == 0 ? "The documents differ in line order or layout." : sb.ToString().TrimEnd();
        }

        private static void AppendLines(StringBuilder sb, string label, List<string> lines)
        {
            if (lines.Count == 0)
            {
                return;
            }
            const int max = 5;
            sb.Append(char.ToUpperInvariant(label[0])).Append(label.Substring(1)).Append(": ");
            for (int i = 0; i < lines.Count && i < max; i++)
            {
                if (i > 0) sb.Append(" | ");
                sb.Append(lines[i]);
            }
            if (lines.Count > max)
            {
                sb.Append(" | (" + (lines.Count - max) + " more)");
            }
            sb.Append(". ");
        }

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
    }
}
