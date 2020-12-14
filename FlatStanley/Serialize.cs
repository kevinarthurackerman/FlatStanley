using System;
using System.Collections.Generic;

namespace FlatStanley
{
    public static class Serialize
    {
        public delegate IReadOnlyList<FieldCell> Delegate(Context context);

        public class Context
        {
            internal Context() { }
            public string ParentPath { get; internal init; }
            public string RelativeValuePath { get; internal init; }
            public string Path => ParentPath + RelativeValuePath;
            public object ParentValue { get; internal init; }
            public object Value { get; internal init; }
            public Type ValueType { get; internal init; }
            public Delegate Serialize { get; internal init; }
        }
    }
}
