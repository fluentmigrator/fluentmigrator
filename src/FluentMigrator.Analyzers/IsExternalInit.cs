namespace System.Runtime.CompilerServices
{
    // Provides the missing IsExternalInit type required for C# init-only properties when building against older target frameworks.
    // This type is intentionally minimal; it only exists to satisfy the compiler.
    internal class IsExternalInit { }
}
