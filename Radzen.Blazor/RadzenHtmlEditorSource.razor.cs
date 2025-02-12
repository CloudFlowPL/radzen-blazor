using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Radzen.Blazor
{
    /// <summary>
    /// A tool which switches between rendered and source views in <see cref="RadzenHtmlEditor" />.
    /// </summary>
    public partial class RadzenHtmlEditorSource
    {

        /// <summary>
        /// Specifies the title (tooltip) displayed when the user hovers the tool. Set to <c>"View source"</c> by default.
        /// </summary>
        [Parameter]
        public string Title { get; set; } = "View source";

        /// <summary>
        /// Specifies the title (tooltip) displayed when the user hovers the tool in Markdown mode. Set to <c>"View Markdown"</c> by default.
        /// </summary>
        [Parameter]
        public string MarkdownTitle { get; set; } = "View Markdown";

        protected override async Task OnClick()
        {
            if (Editor.GetMode() == HtmlEditorMode.Design)
            {
                Editor.SetMode(HtmlEditorMode.Source);
            }
            else if (Editor.GetMode() == HtmlEditorMode.Source)
            {
                Editor.SetMode(HtmlEditorMode.Markdown);
            }
            else
            {
                Editor.SetMode(HtmlEditorMode.Design);
            }

            await Task.CompletedTask;
        }
    }
}
