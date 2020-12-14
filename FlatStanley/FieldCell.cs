using System;

namespace FlatStanley
{
    public class FieldCell
    {
        internal FieldCell() { }
        public string Path { get; internal init; }
        public string Value { get; internal init; }
        public string GetRelativeValuePath(string basePath)
        {
            if (!Path.StartsWith(basePath))
                throw new InvalidOperationException($"{nameof(Path)} does not start with {nameof(basePath)}");

            return Path.Substring(basePath.Length);
        }
    }
}
