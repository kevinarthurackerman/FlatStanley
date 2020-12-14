using System;
using System.Collections.Generic;
using System.Linq;

namespace FlatStanley.DefaultMaps
{
    public class RecordMap<TRecord> : AbstractMap
    {
        public string DefaultPathSegment { get; init; } = typeof(TRecord).Name;

        public Func<TRecord, bool> IsDefaultWhen { get; init; } = record =>
             typeof(TRecord)
                 .GetProperties()
                 .All(prop => Object.Equals(
                     prop.GetValue(record),
                     prop.PropertyType.IsValueType ? Activator.CreateInstance(prop.PropertyType) : null
                ));

        public override CanSerialize.Delegate CanSerialize => context => context.ValueType == typeof(TRecord);

        public override Serialize.Delegate Serialize => context =>
        {
            return typeof(TRecord)
                .GetProperties()
                .Select(prop =>
                {
                    var path = String.IsNullOrEmpty(context.Path)
                        ? DefaultPathSegment
                        : context.Path;

                    var value = context.Value != null
                        ? prop.GetValue(context.Value)
                        : null;

                    return context.Serialize(new Serialize.Context
                    {
                        ParentPath = path,
                        RelativeValuePath = $".{prop.Name}",
                        ParentValue = context.Value,
                        Value = value,
                        ValueType = value?.GetType() ?? prop.PropertyType,
                        Serialize = context.Serialize
                    });
                })
                .SelectMany(fieldCell => fieldCell)
                .ToArray();
        };

        public override CanDeserialize.Delegate CanDeserialize => context =>
        {
            if (context.ValueType == typeof(TRecord)) return true;

            if (DefaultPathSegment == null) return false;

            var pathStart = $"{context.ParentPath}{DefaultPathSegment}.";

            return context.FieldCells.Any(x => x.Path.StartsWith(pathStart));
        };

        public override Deserialize.Delegate Deserialize => context =>
        {
            var pathStart = String.IsNullOrEmpty(context.ParentPath)
                ? DefaultPathSegment
                : context.ParentPath;

            if (!String.IsNullOrEmpty(pathStart)) pathStart = $"{pathStart}.";

            var record = Activator.CreateInstance<TRecord>();

            var props = typeof(TRecord).GetProperties();

            var ususedFieldCells = context.FieldCells.ToList();
            foreach (var prop in props)
            {
                var propPathStart = pathStart + prop.Name;

                var fieldCells = (IReadOnlyList<FieldCell>)ususedFieldCells
                    .Where(x => x.Path.StartsWith(propPathStart))
                    .ToList()
                    .AsReadOnly();

                if (!fieldCells.Any()) continue;

                foreach (var fieldCell in fieldCells) ususedFieldCells.Remove(fieldCell);

                var value = context.Deserialize(new Deserialize.Context
                {
                    ParentPath = pathStart + prop.Name,
                    ValueType = prop.PropertyType,
                    FieldCells = fieldCells,
                    Deserialize = context.Deserialize
                });

                prop.SetValue(record, value);
            }

            if (ususedFieldCells.Any()) throw new InvalidOperationException("Some fields were unused.");

            if (IsDefaultWhen(record)) 
                return typeof(TRecord).IsValueType
                    ? Activator.CreateInstance(typeof(TRecord))
                    : null;

            return record;
        };
    }
}
