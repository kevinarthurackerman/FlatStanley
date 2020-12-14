using System;
using System.Linq;

namespace FlatStanley.DefaultMaps
{
    public class ValueMap<TValue> : AbstractMap
    {
        public string DefaultPathSegment { get; init; } = "Value";

        public string DefaultValue { get; init; } = String.Empty;

        public override CanSerialize.Delegate CanSerialize => context => context.ValueType == typeof(TValue);

        public override Serialize.Delegate Serialize => context =>
        {
            var path = String.IsNullOrEmpty(context.Path)
                ? DefaultPathSegment
                : context.Path;

            var value = String.IsNullOrEmpty(context.Value?.ToString())
                ? DefaultValue
                : context.Value?.ToString();

            return new FieldCell[]
            {
                new FieldCell { Path = path, Value = value ?? String.Empty }
            };
        };

        public override CanDeserialize.Delegate CanDeserialize => context =>
        {
            if (context.FieldCells.Count() != 1) return false;

            if (context.ValueType == typeof(TValue)) return true;

            if (context.FieldCells[0].Path == $"{context.ParentPath}{DefaultPathSegment}") return true;

            return false;
        };

        public override Deserialize.Delegate Deserialize => context =>
            context.FieldCells[0].Value == DefaultValue
                ? default(TValue)
                : Convert.ChangeType(context.FieldCells[0].Value, typeof(TValue));
    }
}
