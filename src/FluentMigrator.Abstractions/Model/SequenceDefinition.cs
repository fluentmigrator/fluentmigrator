namespace FluentMigrator.Model
{
    using System;
    using System.Collections.Generic;
    using Infrastructure;

    public class SequenceDefinition: ICloneable, ICanBeValidated
    {
        public virtual string Name { get; set; }
        public virtual string SchemaName { get; set; }

        public virtual long? Increment { get; set; }

        public virtual long? MinValue { get; set; }
        public virtual long? MaxValue { get; set; }

        public virtual long? StartWith { get; set; }

        public virtual long? Cache { get; set; }

        public virtual bool Cycle { get; set; }

        public object Clone()
        {
            return new SequenceDefinition
                   {
                           Name = Name,
                           SchemaName = SchemaName,
                           Increment = Increment,
                           MinValue = MinValue,
                           MaxValue = MaxValue,
                           StartWith = StartWith,
                           Cache = Cache,
                           Cycle = Cycle
                   };
        }

        public void CollectValidationErrors(ICollection<string> errors)
        {
            if (String.IsNullOrEmpty(Name))
                errors.Add(ErrorMessages.SequenceNameCannotBeNullOrEmpty);
        }
    }
}