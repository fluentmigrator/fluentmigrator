using System.Collections.Generic;
using System.IO;

namespace FluentMigrator.SchemaDump.SchemaWriters {
	public abstract class SchemaWriterBase : ISchemaWriter {
		public abstract void WriteToStream( ICollection<Model.TableDefinition> tables, System.IO.StreamWriter output );

		public virtual void WriteToFile( ICollection<Model.TableDefinition> tables, string file ) {
			using ( var fs = new FileStream( file, FileMode.Create ) ) {
				using ( var sr = new StreamWriter( fs ) ) {
					WriteToStream( tables, sr );
					sr.Flush();
				}
			}
		}
	}
}
