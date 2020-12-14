using System;

namespace FlatStanley
{
    public static class CanSerialize
    {
        public delegate bool Delegate(Context context);

        public class Context
        {
            internal Context() { }
            public string ParentPath { get; internal init; }
            public string RelativeValuePath { get; internal init; }
            public string Path => ParentPath + RelativeValuePath;
            public object ParentValue { get; internal init; }
            public object Value { get; internal init; }
            public Type ValueType { get; internal init; }
        }
    }
}
