using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FlatStanley.DefaultMaps
{
    public class EnumerableMap<TItem> : AbstractMap
    {
        public string DefaultPathSegment { get; init; } = "Items";

        public bool RemoveDefault { get; init; } = true;

        public override CanSerialize.Delegate CanSerialize => context => context.ValueType.IsAssignableTo(typeof(IEnumerable<TItem>));

        public override Serialize.Delegate Serialize => context =>
            {
                var path = String.IsNullOrEmpty(context.Path)
                    ? DefaultPathSegment
                    : context.Path;

                var value = context.Value as IEnumerable<TItem> ?? (context.Value as IEnumerable).Cast<TItem>();
                var items = value.Cast<object>().ToArray();
                var fieldCells = new List<FieldCell>();

                for (var index = 0; index < items.Length; index++)
                {
                    var fieldCellsForItem = context.Serialize(new Serialize.Context
                    {
                        ParentPath = path,
                        RelativeValuePath = $"[{index}]",
                        ParentValue = value,
                        Value = items[index],
                        ValueType = items[index]?.GetType() ?? typeof(TItem),
                        Serialize = context.Serialize
                    });

                    foreach (var fieldCell in fieldCellsForItem)
                    {
                        fieldCells.Add(fieldCell);
                    }
                }

                return fieldCells.ToArray();
            };

        public override CanDeserialize.Delegate CanDeserialize => context =>
        {
            if (context.ValueType == typeof(IEnumerable<TItem>)) return true;
            
            var pathStart = String.IsNullOrEmpty(context.ParentPath)
                ? DefaultPathSegment
                : context.ParentPath;
            var escapedPathStart = Regex.Escape(pathStart);
            var pathRegex = $@"^(?:{escapedPathStart}\[)(\d+)(?:\])";

            return context.FieldCells.Any(x => Regex.IsMatch(x.Path, pathRegex));
        };

        public override Deserialize.Delegate Deserialize => context =>
        {
            var pathStart = String.IsNullOrEmpty(context.ParentPath)
                ? DefaultPathSegment
                : context.ParentPath;
            var escapedPathStart = Regex.Escape(pathStart);
            var pathRegex = $@"^(?:{escapedPathStart}\[)(\d+)(?:\])";

            var itemGroups = context.FieldCells
                .GroupBy(x => Int32.Parse(Regex.Match(x.Path, pathRegex).Groups[1].Value))
                .ToArray();

            var items = new List<TItem>();

            foreach(var itemGroup in itemGroups)
            {
                var item = (TItem)context.Deserialize(new Deserialize.Context
                {
                    ParentPath = $"{pathStart}[{itemGroup.Key}]",
                    ValueType = typeof(TItem),
                    FieldCells = itemGroup.ToArray(),
                    Deserialize = context.Deserialize
                });

                if (RemoveDefault && Object.Equals(item, default)) continue;

                items.Add(item);
            }

            return items.ToArray();
        };
    }
}
