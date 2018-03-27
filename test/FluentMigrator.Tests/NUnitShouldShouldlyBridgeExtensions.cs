using System;
using System.Collections.Generic;
using Shouldly;

namespace NUnit.Should
{
    public static class NUnitShouldShouldlyBridgeExtensions
    {
        public static void ShouldBeTrue(this bool actual)
        {
            ShouldBeTestExtensions.ShouldBe(actual, true);
        }

        public static void ShouldBeFalse(this bool actual)
        {
            ShouldBeTestExtensions.ShouldBe(actual, false);
        }

        public static void ShouldNotBeNull<T>(this T actual) where T : class
        {
            ShouldBeTestExtensions.ShouldNotBe(actual, null);
        }

        public static void ShouldBeNull<T>(this T actual) where T : class
        {
            ShouldBeTestExtensions.ShouldBe(actual, null);
        }

        public static void ShouldBeGreaterThan<T>(this T actual, T expected) where T : IComparable<T>
        {
            ShouldBeTestExtensions.ShouldBeGreaterThan(actual, expected);
        }

        public static void ShouldBe<T>(this T actual, T expected)
        {
            ShouldBeTestExtensions.ShouldBe(actual, expected);
        }

        public static void ShouldBeSameAs<T>(this T actual, T expected)
        {
            ShouldBeTestExtensions.ShouldBeSameAs(actual, expected);
        }

        public static void ShouldContain<T>(this IEnumerable<T> collection, T item)
        {
            ShouldBeEnumerableTestExtensions.ShouldContain(collection, item);
        }

        public static void ShouldNotContain<T>(this IEnumerable<T> collection, T item)
        {
            ShouldBeEnumerableTestExtensions.ShouldNotContain(collection, item);
        }

        public static void ShouldBeOfType<T>(this object actual)
        {
            ShouldBeTestExtensions.ShouldBeOfType<T>(actual);
        }
    }
}