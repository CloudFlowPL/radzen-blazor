namespace Radzen.Blazor
{
    /// <summary>
    /// Represents the state of a command in the RadzenMarkdownEditor.
    /// </summary>
    public class RadzenMarkdownEditorCommandState
    {
        /// <summary>
        /// Gets or sets a value indicating whether the command is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the command is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the Markdown content.
        /// </summary>
        public string Markdown { get; set; }
    }
}
