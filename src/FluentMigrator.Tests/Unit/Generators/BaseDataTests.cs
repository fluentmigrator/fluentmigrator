
namespace FluentMigrator.Tests.Unit.Generators
{
    using NUnit.Framework;
using FluentMigrator.Expressions;
    using FluentMigrator.Model;
    using System.Collections.Generic;
    using System;

    public abstract class BaseDataTests : GeneratorTestBase
    {

        public abstract void CanInsertData();
        public abstract void CanDeleteData();
        public abstract void CanDeleteDataAllRows();
        public abstract void CanDeleteDataMultipleRows();
        public abstract void CanInsertGuidData();

        public abstract void CanUpdateData();

        
    }
}
