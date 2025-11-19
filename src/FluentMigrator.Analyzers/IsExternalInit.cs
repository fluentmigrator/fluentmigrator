#if NETSTANDARD2_0
namespace System.Runtime.CompilerServices
{
    // This internal class is a polyfill to enable the 'init' keyword for older frameworks.
    internal static class IsExternalInit { }
}
#endif
