using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace FluentMigrator.Infrastructure
{
    /// <summary>
    /// A bundle of one or more Assembly instances
    /// </summary>
    public interface IAssemblyCollection
    {
        /// <summary>
        /// The Assemblies contained in this collection
        /// </summary>
        Assembly[] Assemblies { get; }

        /// <summary>
        /// The result of this method is equivalent to calling GetExportedTypes
        /// on each Assembly in Assemblies.
        /// </summary>
        /// <returns></returns>
        Type[] GetExportedTypes();

        /// <summary>
        /// Gets a array of resources defined in each of the assemblies that are 
        /// contained in this collection, plus which assembly it is defined in.
        /// </summary>
        /// <returns>An array of value pairs of resource name plus assembly.</returns>
        ManifestResourceNameWithAssembly[] GetManifestResourceNames();
    }

    /// <summary>
    /// Combines a ManifestResourceName with the assembly it belongs to
    /// </summary>
    public class ManifestResourceNameWithAssembly
    {
        public ManifestResourceNameWithAssembly(string name, Assembly assembly)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            Name = name;
            Assembly = assembly;
        }

        public Assembly Assembly { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// A simple wrapper which is equivalent to a collection with a single Assembly
    /// </summary>
    public class SingleAssembly : IAssemblyCollection
    {
        private Assembly _assembly;

        public SingleAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            _assembly = assembly;
        }

        public Type[] GetExportedTypes()
        {
            return _assembly.GetExportedTypes();
        }

        public Assembly[] Assemblies
        {
            get { return new[] { _assembly }; }
        }

        public ManifestResourceNameWithAssembly[] GetManifestResourceNames()
        {
            return _assembly.GetManifestResourceNames().Select(name => new ManifestResourceNameWithAssembly(name, _assembly)).ToArray();
        }
    }

    public class AssemblyCollection : IAssemblyCollection
    {
        private IEnumerable<Assembly> _assemblies;

        public AssemblyCollection(IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException("assemblies");
            }

            _assemblies = assemblies;
        }

        public Type[] GetExportedTypes()
        {
            return _assemblies.SelectMany(a => a.GetExportedTypes()).ToArray();
        }

        public Assembly[] Assemblies
        {
            get { return _assemblies.ToArray(); }
        }

        public ManifestResourceNameWithAssembly[] GetManifestResourceNames()
        {
            return _assemblies.SelectMany(a => a.GetManifestResourceNames().Select(name =>
                new ManifestResourceNameWithAssembly(name, a))).ToArray();
        }
    }

}
