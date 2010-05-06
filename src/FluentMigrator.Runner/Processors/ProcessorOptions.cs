namespace FluentMigrator.Runner.Processors
{
	public class ProcessorOptions : IMigrationProcessorOptions
	{
		#region IMigrationProcessorOptions Members

		public bool PreviewOnly { get; set; }

		#endregion
	}
}