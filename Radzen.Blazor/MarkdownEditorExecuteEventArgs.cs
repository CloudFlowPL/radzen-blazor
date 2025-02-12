namespace Radzen.Blazor
{
    /// <summary>
    /// Provides data for the Execute event of the RadzenMarkdownEditor.
    /// </summary>
    public class MarkdownEditorExecuteEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownEditorExecuteEventArgs"/> class.
        /// </summary>
        /// <param name="editor">The editor.</param>
        public MarkdownEditorExecuteEventArgs(RadzenMarkdownEditor editor)
        {
            Editor = editor;
        }

        /// <summary>
        /// Gets the editor.
        /// </summary>
        public RadzenMarkdownEditor Editor { get; }

        /// <summary>
        /// Gets or sets the name of the command.
        /// </summary>
        public string CommandName { get; set; }
    }
}
