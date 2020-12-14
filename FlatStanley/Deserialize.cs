using System;
using System.Collections.Generic;

namespace FlatStanley
{
    public static class Deserialize
    {
        public delegate object Delegate(Context context);

        public class Context
        {
            internal Context() { }
            public string ParentPath { get; internal init; }
            public Type ValueType { get; internal init; }
            public IReadOnlyList<FieldCell> FieldCells { get; internal init; }
            public Delegate Deserialize { get; internal init; }
        }
    }
}
