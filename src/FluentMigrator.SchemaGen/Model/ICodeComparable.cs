namespace FluentMigrator.SchemaGen.Model
{
    public interface ICodeComparable
    {
        /// <summary>
        /// Fully qualified name
        /// </summary>
        string FQName { get; }

        /// <summary>
        /// Code to create the object (typically include object's name) 
        /// </summary>
        string CreateCode { get; }

        /// <summary>
        /// Code to delete the object 
        /// </summary>
        string DeleteCode { get; }

        /// <summary>
        /// Definition code for the object that excludes object name (used to detect renaming). 
        /// Used to identify definition changes.
        /// </summary>
        string DefinitionCode { get; }

        /// <summary>
        /// Changing the fields types requires indexes and foreign keys that depend on the field to also be updated.
        /// </summary>
        bool TypeChanged { get; }
    }
}