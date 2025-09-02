using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace FluentMigrator.Runner.Generators.Base
{
    /// <summary>
    /// Serves as the base class for database type mappings.
    /// </summary>
    /// <remarks>
    /// This abstract class provides a foundation for mapping .NET <see cref="DbType"/> values to their corresponding
    /// SQL types. It includes functionality for defining and retrieving type mappings, including support for size
    /// and precision placeholders. Derived classes are expected to implement the <see cref="SetupTypeMaps"/> method
    /// to define specific type mappings for a particular database.
    /// </remarks>
    public abstract class TypeMapBase : ITypeMap
    {
        private readonly Dictionary<DbType, SortedList<int, string>> _templates = new Dictionary<DbType, SortedList<int, string>>();
        /// <summary>
        /// Represents a placeholder for size values in SQL type templates.
        /// </summary>
        /// <remarks>
        /// This constant is used within SQL type templates to indicate where a size value should be inserted.
        /// It is replaced with the actual size value during runtime when generating SQL type definitions.
        /// </remarks>
        private const string SizePlaceholder = "$size";
        /// <summary>
        /// Represents a placeholder for precision values in SQL type templates.
        /// </summary>
        /// <remarks>
        /// This constant is used within SQL type templates to indicate where a precision value should be inserted.
        /// It is replaced with the actual precision value during runtime when generating SQL type definitions.
        /// </remarks>
        protected const string PrecisionPlaceholder = "$precision";

        /// <summary>
        /// Configures the type mappings for the database.
        /// </summary>
        /// <remarks>
        /// This method is intended to be implemented by derived classes to define the specific mappings
        /// between .NET <see cref="DbType"/> values and their corresponding SQL types for a particular database.
        /// Implementations should use the <see cref="SetTypeMap(DbType, string)"/> and 
        /// <see cref="SetTypeMap(DbType, string, int)"/> methods to register the mappings.
        /// </remarks>
        protected abstract void SetupTypeMaps();

        /// <summary>
        /// Defines a type mapping for a specific <see cref="DbType"/> with an associated SQL template.
        /// </summary>
        /// <param name="type">The <see cref="DbType"/> to map.</param>
        /// <param name="template">
        /// The SQL type template associated with the <paramref name="type"/>. 
        /// This template may include placeholders such as <c>$size</c> or <c>$precision</c> for dynamic substitution.
        /// </param>
        /// <remarks>
        /// This method ensures that a mapping exists for the specified <paramref name="type"/> and assigns the provided
        /// <paramref name="template"/> to it. If a mapping for the <paramref name="type"/> does not already exist, it will
        /// be initialized.
        /// </remarks>
        protected void SetTypeMap(DbType type, string template)
        {
            EnsureHasList(type);
            _templates[type][-1] = template;
        }

        /// <summary>
        /// Defines a type mapping for a specific <see cref="DbType"/> with a template and an optional maximum size.
        /// </summary>
        /// <param name="type">The <see cref="DbType"/> to be mapped.</param>
        /// <param name="template">
        /// The SQL type template associated with the <paramref name="type"/>. 
        /// This template may include placeholders such as <c>$size</c> or <c>$precision</c>.
        /// </param>
        /// <param name="maxSize">
        /// The maximum size associated with the <paramref name="template"/>. 
        /// This value is used to differentiate templates for the same <paramref name="type"/> with varying sizes.
        /// </param>
        /// <remarks>
        /// This method ensures that a mapping exists for the specified <paramref name="type"/> 
        /// and associates the provided <paramref name="template"/> with the given <paramref name="maxSize"/>.
        /// </remarks>
        protected void SetTypeMap(DbType type, string template, int maxSize)
        {
            EnsureHasList(type);
            _templates[type][maxSize] = template;
        }

        /// <inheritdoc />
        public virtual string GetTypeMap(DbType type, int? size, int? precision)
        {
            if (!_templates.ContainsKey(type))
                throw new NotSupportedException($"Unsupported DbType '{type}'");

            if (size == null)
                return ReplacePlaceholders(_templates[type][-1], size: 0, precision);

            var sizeValue = size.Value;
            foreach (var entry in _templates[type])
            {
                int capacity = entry.Key;
                string template = entry.Value;

                if (sizeValue <= capacity)
                    return ReplacePlaceholders(template, sizeValue, precision);
            }

            throw new NotSupportedException($"Unsupported DbType '{type}'");
        }

        private void EnsureHasList(DbType type)
        {
            if (!_templates.ContainsKey(type))
                _templates.Add(type, new SortedList<int, string>());
        }

        private string ReplacePlaceholders(string value, int? size, int? precision)
        {
            if (size != null)
            {
                value = value.Replace(SizePlaceholder, size.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (precision != null)
            {
                value = value.Replace(PrecisionPlaceholder, precision.Value.ToString(CultureInfo.InvariantCulture));
            }

            return value;
        }

    }
}
