using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace FlatStanley
{
    public interface IMapper
    {
        string Serialize<TItem>(IEnumerable<TItem> items);
        IEnumerable<TItem> Deserialize<TItem>(string value);
    }

    internal class Mapper : IMapper
    {
        private IList<AbstractMap> _maps { get; set; } = new List<AbstractMap>();

        internal Mapper(IList<AbstractMap> maps)
        {
            _maps = maps.ToList();
        }

        public string Serialize<TItem>(IEnumerable<TItem> items)
        {
            var fieldHeaders = new Dictionary<string, FieldHeader>();
            var rows = new List<IReadOnlyList<FieldCell>>();

            foreach(var item in items)
            {
                var map = GetMap(new CanSerialize.Context
                { 
                    ParentPath = String.Empty,
                    RelativeValuePath = String.Empty,
                    ParentValue = null,
                    Value = null,
                    ValueType = typeof(TItem)
                });

                var fieldCells = map.Serialize(new Serialize.Context
                {
                    ParentPath = String.Empty,
                    ParentValue = null,
                    Value = item,
                    ValueType = item?.GetType() ?? typeof(TItem),
                    Serialize = Serialize
                });

                foreach (var fieldCell in fieldCells)
                {
                    var fieldHeader = new FieldHeader { Path = fieldCell.Path, Position = fieldHeaders.Count };
                    fieldHeaders.TryAdd(fieldCell.Path, fieldHeader);
                }

                rows.Add(fieldCells);
            }

            var totalColumnCount = fieldHeaders.Count;
            var orderedFieldHeaders = fieldHeaders.Values.OrderBy(x => x.Position).ToArray();

            var stringWriter = new StringWriter();
            using var csvWriter = new CsvWriter(stringWriter, CultureInfo.InvariantCulture);

            for (var columnIndex = 0; columnIndex < fieldHeaders.Count; columnIndex++)
            {
                csvWriter.WriteField(orderedFieldHeaders[columnIndex].Path);
            }

            for (var rowNumber = 0; rowNumber < rows.Count; rowNumber++)
            {
                csvWriter.NextRecord();

                var fieldCells = rows[rowNumber];
                var row = new FieldCell[totalColumnCount];

                foreach(var fieldCell in fieldCells)
                {
                    var columnIndex = fieldHeaders[fieldCell.Path].Position;
                    row[columnIndex] = fieldCell;
                }

                foreach (var field in row)
                {
                    if (field == null)
                    {
                        csvWriter.WriteField(String.Empty);
                    }
                    else
                    {
                        csvWriter.WriteField(field.Value);
                    }
                }
            }

            csvWriter.Flush();
            return stringWriter.ToString();
        }

        public IEnumerable<TItem> Deserialize<TItem>(string value)
        {
            var stringReader = new StringReader(value);
            using var csvReader = new CsvReader(stringReader, CultureInfo.InvariantCulture);

            csvReader.Read();
            var fieldHeaders = csvReader.Context.Record
                .Select((x,i) => new FieldHeader { Path = x, Position = i })
                .ToArray();

            while (csvReader.Read())
            {
                var fieldCells = csvReader.Context.Record
                    .Select((x, i) => new FieldCell { Path = fieldHeaders[i].Path, Value = x })
                    .ToArray();

                var map = GetMap(new CanDeserialize.Context
                { 
                    ParentPath = String.Empty,
                    ValueType = typeof(TItem),
                    FieldCells = fieldCells
                });

                yield return (TItem)map.Deserialize(new Deserialize.Context
                {
                    ParentPath = String.Empty,
                    ValueType = typeof(TItem),
                    FieldCells = fieldCells,
                    Deserialize = Deserialize
                });
            }
        }

        private AbstractMap GetMap(CanDeserialize.Context canDeserializeContext) =>
            _maps.First(x => x.CanDeserialize(canDeserializeContext));

        private AbstractMap GetMap(CanSerialize.Context canSerializeContext) =>
            _maps.First(x => x.CanSerialize(canSerializeContext));

        private IReadOnlyList<FieldCell> Serialize(Serialize.Context context) =>
            GetMap(new CanSerialize.Context
            {
                ParentPath = context.ParentPath,
                RelativeValuePath = context.RelativeValuePath,
                ParentValue = context.ParentValue,
                Value = context.Value,
                ValueType = context.ValueType
            })
            .Serialize(context);

        private object Deserialize(Deserialize.Context context) =>
            GetMap(new CanDeserialize.Context
            {
                ParentPath = context.ParentPath,
                ValueType = context.ValueType,
                FieldCells = context.FieldCells
            })
            .Deserialize(context);
    }
}
