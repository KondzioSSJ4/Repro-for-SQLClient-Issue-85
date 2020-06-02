using System;

namespace SQLClient_Issue85
{
    public struct ObjectDefinition<T>
    {
        public string FieldName { get; set; }

        public Func<T, object> GetValueFunc { get; set; }

        public Type PropertyType { get; set; }
    }
}
