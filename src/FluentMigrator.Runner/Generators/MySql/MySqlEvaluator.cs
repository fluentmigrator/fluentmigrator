using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.MySql
{
    public class MySqlEvaluator : GenericEvaluator
    {
        private string ByteArrayToHexString(IEnumerable<byte> data)
        {
            return String.Concat(data.Select(value => string.Format("{0:X2}", value)).ToArray());
        }

        public override IDataValue GetDataValue(PropertyDescriptor propertyDescriptor, object data)
        {
            if (propertyDescriptor.PropertyType.IsArray && propertyDescriptor.PropertyType.GetElementType().Equals(typeof(byte)))
            {
                byte[] value = propertyDescriptor.GetValue(data) as byte[];

                if (value != null)
                {
                    return new DataValue(propertyDescriptor.Name, MySqlFunctions.Unhex(ByteArrayToHexString(value)), false);
                }
                else
                {
                    return new DataValue(propertyDescriptor.Name, null);
                }
            }
            else
            {
                return base.GetDataValue(propertyDescriptor, data);
            }
        }
    }
}
