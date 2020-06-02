using System;
using System.Collections.Generic;

namespace SQLClient_Issue85
{

    public abstract class BulkCopyTableDefinition<T>
    {
        public string TableName { get; protected set; }

        public IReadOnlyList<ObjectDefinition<T>> Schema { get; protected set; }

        protected static ObjectDefinition<T> GetObjectValue<TOutput>(Func<T, TOutput> getValueFunc, string columnName)
        {
            var type = typeof(TOutput);
            return new ObjectDefinition<T>()
            {
                FieldName = columnName,
                GetValueFunc = (d) => getValueFunc(d),
                PropertyType = Nullable.GetUnderlyingType(type) ?? type
            };
        }
    }
}
