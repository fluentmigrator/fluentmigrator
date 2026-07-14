#region License
// Copyright (c) 2025, Fluent Migrator Project
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
using System.Collections.Generic;
using System.IO;

using FluentMigrator.Runner.Processors.Oracle;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.Oracle;

public static class OracleTestUtils
{
    public static void CheckNativeOracleDataAccess(OracleProcessorBase oracleProcessor)
    {
        try
        {
            _ = oracleProcessor.Connection;
        }
        catch (Exception e) when (IsMissingNativeClientException(e))
        {
            Assert.Ignore("Oracle Data Access Client not installed");
        }
    }

    private static bool IsMissingNativeClientException(Exception exception)
    {
        foreach (var e in Flatten(exception))
        {
            if (e is FileNotFoundException or DllNotFoundException or BadImageFormatException or TypeLoadException)
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<Exception> Flatten(Exception exception)
    {
        var current = exception;
        while (current != null)
        {
            yield return current;

            if (current is AggregateException aggregate)
            {
                foreach (var inner in aggregate.InnerExceptions)
                {
                    foreach (var flattened in Flatten(inner))
                    {
                        yield return flattened;
                    }
                }

                yield break;
            }

            current = current.InnerException;
        }
    }
}
