namespace FluentMigrator.Builders.Create.Sequence
{
    public interface ICreateSequenceSyntax
    {
        ICreateSequenceSyntax IncrementBy(long increment);

        ICreateSequenceSyntax MinValue(long minValue);

        ICreateSequenceSyntax MaxValue(long maxValue);

        ICreateSequenceSyntax StartWith(long startwith);

        ICreateSequenceSyntax Cache(long value);

        ICreateSequenceSyntax Cycle();
    }
}