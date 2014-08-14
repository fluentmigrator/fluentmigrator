namespace FluentMigrator.Builders.Create.Sequence
{
    using Expressions;

    public class CreateSequenceExpressionBuilder : ExpressionBuilderBase<CreateSequenceExpression>, ICreateSequenceSyntax, ICreateSequenceInSchemaSyntax
    {
        public CreateSequenceExpressionBuilder(CreateSequenceExpression expression)
            : base(expression)
        {
        }

        public ICreateSequenceSyntax IncrementBy(long increment)
        {
            Expression.Sequence.Increment = increment;
            return this;
        }

        public ICreateSequenceSyntax MinValue(long minValue)
        {
            Expression.Sequence.MinValue = minValue;
            return this;
        }

        public ICreateSequenceSyntax MaxValue(long maxValue)
        {
            Expression.Sequence.MaxValue = maxValue;
            return this;
        }

        public ICreateSequenceSyntax StartWith(long startwith)
        {
            Expression.Sequence.StartWith = startwith;
            return this;
        }

        public ICreateSequenceSyntax Cache(long value)
        {
            Expression.Sequence.Cache = value;
            return this;
        }

        public ICreateSequenceSyntax Cycle()
        {
            Expression.Sequence.Cycle = true;
            return this;
        }

        public ICreateSequenceSyntax InSchema(string schemaName)
        {
            Expression.Sequence.SchemaName = schemaName;
            return this;
        }
    }
}