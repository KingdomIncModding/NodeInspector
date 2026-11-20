namespace KingdomNodeInspector.Inspection
{
    internal sealed class NodeItemStatEntry
    {
        public string Label { get; }
        public int Value { get; }
        public string FormattedLine { get; }

        public NodeItemStatEntry(string label, int value, string formattedLine)
        {
            Label = label ?? "";
            Value = value;
            FormattedLine = formattedLine ?? "";
        }
    }
}
