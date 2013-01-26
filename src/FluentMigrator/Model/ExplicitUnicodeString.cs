namespace FluentMigrator.Model
{
    public class ExplicitUnicodeString
    {
        public string Text { get; set; }

        public ExplicitUnicodeString(string text)
        {
            Text = text;
        }
        
        public override string ToString()
        {
            return Text;
        }
    }
}
