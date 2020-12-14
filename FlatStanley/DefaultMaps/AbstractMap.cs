namespace FlatStanley
{
    public abstract class AbstractMap
    {
        public abstract CanSerialize.Delegate CanSerialize { get; }
        public abstract CanDeserialize.Delegate CanDeserialize { get; }
        public abstract Serialize.Delegate Serialize { get; }
        public abstract Deserialize.Delegate Deserialize { get; }
    }
}
