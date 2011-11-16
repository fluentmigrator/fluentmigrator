using System;

namespace FluentMigrator.Runner.Initialization
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class OnAssemblyLoadAttribute : Attribute
    {
    }
}
