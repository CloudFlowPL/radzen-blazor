namespace Radzen.Blazor
{
    /// <summary>
    /// Provides data for the Paste event of the RadzenMarkdownEditor.
    /// </summary>
    public class MarkdownEditorPasteEventArgs
    {
        /// <summary>
        /// Gets or sets the Markdown content being pasted.
        /// </summary>
        public string Markdown { get; set; }
    }
}
