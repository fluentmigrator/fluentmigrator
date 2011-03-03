namespace FluentMigrator.Runner.Processors
{
	public class ProcessorOptions : IMigrationProcessorOptions
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ProcessorOptions"/> class.
        /// </summary>
        public ProcessorOptions()
        {
            Timeout = 30;    
        }

		public bool PreviewOnly { get; set; }
		public int Timeout { get; set; }
	}
}