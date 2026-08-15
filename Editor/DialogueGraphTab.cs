namespace DialogTree.Editor
{
    public sealed class DialogueGraphTab
    {
        public string GraphId;
        public string Title = "New graph";
        public string FilePath;
        public bool IsDirty;

        public DialogueGraphView View { get; } = new();

        public string DisplayLabel => (IsDirty ? "*" : "") + Title;
    }
}