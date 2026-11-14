#region License
// Copyright (c) 2018, Fluent Migrator Project
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
using System.Globalization;

using NUnit.Framework;

// Deliberately outside any namespace: an NUnit SetUpFixture declared in the global namespace
// applies to every test in the assembly. Both other SetUpFixtures in this project are
// namespace-scoped and so would not cover the culture-sensitive tests.

/// <summary>
/// Fails the whole run if .NET globalization-invariant mode is active.
/// </summary>
/// <remarks>
/// <para>
/// Several tests here assert <em>culture-specific</em> behaviour - most directly
/// <c>TokenSubstitutionMatrixTests.TokenMatrix_IsCultureInvariant</c>, which renders the token
/// matrix under <c>en-US</c> and <c>de-DE</c> and requires the two to match. That assertion is only
/// meaningful if the two cultures genuinely differ. Under globalization-invariant mode every culture
/// collapses to the invariant one, so the comparison becomes a tautology and the test passes without
/// testing anything.
/// </para>
/// <para>
/// Invariant mode is not set anywhere in this repository. It is easy to acquire by accident: it is
/// the documented workaround for the startup failure a .NET install hits on a machine with no
/// <c>libicu</c>, which is a common state for slim containers and fresh WSL distributions.
/// </para>
/// <para>
/// Two probes, because neither alone is sufficient - both verified against a real runtime:
/// </para>
/// <list type="table">
/// <item>
/// <description>
/// <c>InvariantGlobalization=true</c> via MSBuild/runtimeconfig sets the AppContext switch, and
/// paired with <c>PredefinedCulturesOnly=false</c> lets <c>de-DE</c> resolve while behaving as
/// invariant. This is the configuration that passes vacuously, and only the AppContext probe
/// catches it.
/// </description>
/// </item>
/// <item>
/// <description>
/// The <c>DOTNET_SYSTEM_GLOBALIZATION_INVARIANT</c> environment variable does <em>not</em> surface
/// through <c>AppContext.TryGetSwitch</c>. It makes <c>new CultureInfo("de-DE")</c> throw, so the
/// culture tests already fail loudly rather than silently - but they fail with a
/// <c>CultureNotFoundException</c> that reads like a bug in the test. The behaviour probe catches it
/// and says what actually happened.
/// </description>
/// </item>
/// </list>
/// </remarks>
[SetUpFixture]
public class GlobalizationGuard
{
    private const string DeGerman = "de-DE";

    [OneTimeSetUp]
    public void FailIfGlobalizationIsInvariant()
    {
        var reason = DetectInvariantGlobalization();
        if (reason == null)
        {
            return;
        }

        Assert.Fail(
            "Globalization-invariant mode is active, so culture-sensitive tests cannot mean what " +
            "they claim - TokenMatrix_IsCultureInvariant in particular would compare en-US against " +
            $"de-DE when both are really the invariant culture.{Environment.NewLine}" +
            $"Detected because: {reason}{Environment.NewLine}" +
            "Fix: install ICU (on Debian/Ubuntu, `sudo apt install libicu78`) and clear " +
            "DOTNET_SYSTEM_GLOBALIZATION_INVARIANT / <InvariantGlobalization>. Do not set that flag " +
            "to work around a missing libicu here: it silences the very tests that would catch a " +
            "culture regression.");
    }

    /// <summary>
    /// Returns why invariant mode was detected, or <see langword="null"/> when globalization is real.
    /// </summary>
    internal static string DetectInvariantGlobalization()
    {
        if (AppContext.TryGetSwitch("System.Globalization.Invariant", out var viaSwitch) && viaSwitch)
        {
            return "the System.Globalization.Invariant AppContext switch is set " +
                   "(runtimeconfig.json, or <InvariantGlobalization>true</InvariantGlobalization>)";
        }

        try
        {
            var german = new CultureInfo(DeGerman);
            if (!string.Equals(german.NumberFormat.NumberDecimalSeparator, ",", StringComparison.Ordinal))
            {
                return $"{DeGerman} resolves but formats like the invariant culture " +
                       $"(decimal separator '{german.NumberFormat.NumberDecimalSeparator}', expected ',')";
            }
        }
        catch (CultureNotFoundException)
        {
            return $"{DeGerman} cannot be constructed at all, which happens only in " +
                   "globalization-invariant mode";
        }

        return null;
    }
}
