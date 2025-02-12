using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Radzen.Blazor
{
    /// <summary>
    /// A component which edits Markdown content. Provides built-in upload capabilities.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;RadzenMarkdownEditor @bind-Value=@markdown /&gt;
    /// @code {
    ///   string markdown = "# Hello world!";
    /// }
    /// </code>
    /// </example>
    public partial class RadzenMarkdownEditor : FormComponent<string>
    {
        /// <summary>
        /// Specifies whether to show the toolbar. Set it to false to hide the toolbar. Default value is true.
        /// </summary>
        [Parameter]
        public bool ShowToolbar { get; set; } = true;

        /// <summary>
        /// Gets or sets the mode of the editor.
        /// </summary>
        [Parameter]
        public MarkdownEditorMode Mode { get; set; } = MarkdownEditorMode.Preview;

        private MarkdownEditorMode mode;

        /// <summary>
        /// Gets or sets the child content.
        /// </summary>
        /// <value>The child content.</value>
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        /// <summary>
        /// Specifies custom headers that will be submit during uploads.
        /// </summary>
        [Parameter]
        public IDictionary<string, string> UploadHeaders { get; set; }

        /// <summary>
        /// Gets or sets the input.
        /// </summary>
        /// <value>The input.</value>
        [Parameter]
        public EventCallback<string> Input { get; set; }

        /// <summary>
        /// A callback that will be invoked when the user pastes content in the editor. Commonly used to filter unwanted Markdown.
        /// </summary>
        /// <example>
        /// <code>
        /// &lt;RadzenMarkdownEditor @bind-Value=@markdown Paste=@OnPaste /&gt;
        /// @code {
        ///   string markdown = "# Hello world!";
        ///   void OnPaste(MarkdownEditorPasteEventArgs args)
        ///   {
        ///     // Set args.Markdown to filter unwanted tags.
        ///     args.Markdown = args.Markdown.Replace("<!--", "").Replace("-->", "");
        ///   }
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<MarkdownEditorPasteEventArgs> Paste { get; set; }

        /// <summary>
        /// A callback that will be invoked when there is an error during upload.
        /// </summary>
        [Parameter]
        public EventCallback<UploadErrorEventArgs> UploadError { get; set; }

        /// <summary>
        /// Called on upload error.
        /// </summary>
        /// <param name="error">The error.</param>
        [JSInvokable("OnError")]
        public async Task OnError(string error)
        {
            await UploadError.InvokeAsync(new UploadErrorEventArgs { Message = error });
        }

        /// <summary>
        /// A callback that will be invoked when the user executes a command of the editor (e.g. by clicking one of the tools).
        /// </summary>
        /// <example>
        /// <code>
        /// &lt;RadzenMarkdownEditor Execute=@OnExecute&gt;
        ///   &lt;RadzenMarkdownEditorCustomTool CommandName="InsertToday" Icon="today" Title="Insert today" /&gt;
        /// &lt;/RadzenMarkdownEditor&gt;
        /// @code {
        ///   string markdown = "# Hello world!";
        ///   async Task OnExecute(MarkdownEditorExecuteEventArgs args)
        ///   {
        ///     if (args.CommandName == "InsertToday")
        ///     {
        ///       await args.Editor.ExecuteCommandAsync(MarkdownEditorCommands.InsertText, DateTime.Today.ToLongDateString());
        ///     }
        ///  }
        /// </code>
        /// </example>
        [Parameter]
        public EventCallback<MarkdownEditorExecuteEventArgs> Execute { get; set; }

        /// <summary>
        /// Specifies the URL to which RadzenMarkdownEditor will submit files.
        /// </summary>
        [Parameter]
        public string UploadUrl { get; set; }

        ElementReference ContentEditable { get; set; }
        RadzenTextArea TextArea { get; set; }

        /// <summary>
        /// Focuses the editor.
        /// </summary>
        public override ValueTask FocusAsync()
        {

            if (mode == MarkdownEditorMode.Preview)
            {
                return ContentEditable.FocusAsync();
            }
            else
            {
                return TextArea.Element.FocusAsync();
            }
        }

        internal RadzenMarkdownEditorCommandState State { get; set; } = new RadzenMarkdownEditorCommandState();

        async Task OnFocus()
        {
            await UpdateCommandState();
        }

        private readonly IDictionary<string, Func<Task>> shortcuts = new Dictionary<string, Func<Task>>();

        /// <summary>
        /// Registers a shortcut for the specified action.
        /// </summary>
        /// <param name="key">The shortcut. Can be combination of keys such as <c>CTRL+B</c>.</param>
        /// <param name="action">The action to execute.</param>
        public void RegisterShortcut(string key, Func<Task> action)
        {
            shortcuts[key] = action;
        }

        /// <summary>
        /// Unregisters the specified shortcut.
        /// </summary>
        /// <param name="key"></param>
        public void UnregisterShortcut(string key)
        {
            shortcuts.Remove(key);
        }

        /// <summary>
        /// Invoked by interop when the RadzenMarkdownEditor selection changes.
        /// </summary>
        [JSInvokable]
        public async Task OnSelectionChange()
        {
            await UpdateCommandState();
        }

        /// <summary>
        /// Invoked by interop during uploads. Provides the custom headers.
        /// </summary>
        [JSInvokable("GetHeaders")]
        public IDictionary<string, string> GetHeaders()
        {
            return UploadHeaders ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Executes the requested command with the provided value. Check <see cref="MarkdownEditorCommands" /> for the list of supported commands.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public async Task ExecuteCommandAsync(string name, string value = null)
        {
            State = await JSRuntime.InvokeAsync<RadzenMarkdownEditorCommandState>("Radzen.execCommand", ContentEditable, name, value);

            await OnExecuteAsync(name);

            if (Markdown != State.Markdown)
            {
                Markdown = State.Markdown;

                markdownChanged = true;

                await OnChange();
            }
        }

        /// <summary>
        /// Executes the action associated with the specified shortcut. Used internally by RadzenMarkdownEditor.
        /// </summary>
        /// <param name="shortcut"></param>
        /// <returns></returns>
        [JSInvokable("ExecuteShortcutAsync")]
        public async Task ExecuteShortcutAsync(string shortcut)
        {
            if (shortcuts.TryGetValue(shortcut, out var action))
            {
                await action();
            }
        }

        private async Task SourceChanged(string markdown)
        {
            if (Markdown != markdown)
            {
                Markdown = markdown;
                markdownChanged = true;
            }
            await JSRuntime.InvokeVoidAsync("Radzen.innerHTML", ContentEditable, Markdown);
            await OnChange();
            StateHasChanged();
        }

        async Task OnChange()
        {
            if (markdownChanged)
            {
                markdownChanged = false;

                _value = Markdown;

                await ValueChanged.InvokeAsync(Markdown);

                if (FieldIdentifier.FieldName != null)
                {
                    EditContext?.NotifyFieldChanged(FieldIdentifier);
                }

                await Change.InvokeAsync(Markdown);
            }
        }

        internal async Task OnExecuteAsync(string name)
        {
            await Execute.InvokeAsync(new MarkdownEditorExecuteEventArgs(this) { CommandName = name });

            StateHasChanged();
        }

        /// <summary>
        /// Saves the current selection. RadzenMarkdownEditor will lose its selection when it loses focus. Use this method to persist the current selection.
        /// </summary>
        public async Task SaveSelectionAsync()
        {
            await JSRuntime.InvokeVoidAsync("Radzen.saveSelection", ContentEditable);
        }

        /// <summary>
        /// Restores the last saved selection.
        /// </summary>
        public async Task RestoreSelectionAsync()
        {
            await JSRuntime.InvokeVoidAsync("Radzen.restoreSelection", ContentEditable);
        }

        async Task UpdateCommandState()
        {
            State = await JSRuntime.InvokeAsync<RadzenMarkdownEditorCommandState>("Radzen.queryCommands", ContentEditable);

            StateHasChanged();
        }

        async Task OnBlur()
        {
            await OnChange();
        }

        bool markdownChanged = false;

        bool visibleChanged = false;
        bool firstRender = true;

        internal ValueTask<T> GetSelectionAttributes<T>(string selector, string[] attributes)
        {
            return JSRuntime.InvokeAsync<T>("Radzen.selectionAttributes", selector, attributes, ContentEditable);
        }

        /// <inheritdoc />
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            this.firstRender = firstRender;

            if (firstRender || visibleChanged)
            {
                if (Visible)
                {
                    await JSRuntime.InvokeVoidAsync("Radzen.createEditor", ContentEditable, UploadUrl, Paste.HasDelegate, Reference, shortcuts.Keys);
                }
            }

            if (valueChanged || visibleChanged)
            {
                valueChanged = false;
                visibleChanged = false;

                Markdown = Value;

                if (Visible)
                {
                    await JSRuntime.InvokeVoidAsync("Radzen.innerHTML", ContentEditable, Value);
                }
            }
        }

        internal void SetMode(MarkdownEditorMode value)
        {
            mode = value;

            StateHasChanged();
        }

        /// <summary>
        /// Returns the current mode of the editor.
        /// </summary>
        public MarkdownEditorMode GetMode()
        {
            return mode;
        }

        string Markdown { get; set; }

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            Markdown = Value;
            mode = Mode;

            base.OnInitialized();
        }

        /// <summary>
        /// Invoked via interop when the value of RadzenMarkdownEditor changes.
        /// </summary>
        /// <param name="markdown">The Markdown.</param>
        [JSInvokable]
        public void OnChange(string markdown)
        {
            if (Markdown != markdown)
            {
                Markdown = markdown;
                markdownChanged = true;
            }
            Input.InvokeAsync(markdown);
        }

        /// <summary>
        /// Invoked via interop when the user pastes content in RadzenMarkdownEditor. Invokes <see cref="Paste" />.
        /// </summary>
        /// <param name="markdown">The Markdown.</param>
        [JSInvokable]
        public async Task<string> OnPaste(string markdown)
        {
            var args = new MarkdownEditorPasteEventArgs { Markdown = markdown };

            await Paste.InvokeAsync(args);

            return args.Markdown;
        }

        bool valueChanged = false;

        /// <inheritdoc />
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            if (parameters.DidParameterChange(nameof(Value), Value))
            {
                valueChanged = Markdown != parameters.GetValueOrDefault<string>(nameof(Value));
            }

            if (parameters.DidParameterChange(nameof(Mode), Mode))
            {
                mode = Mode;
            }

            visibleChanged = parameters.DidParameterChange(nameof(Visible), Visible);

            await base.SetParametersAsync(parameters);

            if (visibleChanged && !firstRender && !Visible)
            {
                await JSRuntime.InvokeVoidAsync("Radzen.destroyEditor", ContentEditable);
            }
        }

        /// <inheritdoc />
        protected override string GetComponentCssClass()
        {
            return GetClassList("rz-markdown-editor").ToString();
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            base.Dispose();

            if (Visible && IsJSRuntimeAvailable)
            {
                JSRuntime.InvokeVoidAsync("Radzen.destroyEditor", ContentEditable);
            }
        }

        /// <summary>
        /// Gets or sets the callback which when a file is uploaded.
        /// </summary>
        /// <value>The complete callback.</value>
        [Parameter]
        public EventCallback<UploadCompleteEventArgs> UploadComplete { get; set; }


        internal async Task RaiseUploadComplete(UploadCompleteEventArgs args)
        {
            await UploadComplete.InvokeAsync(args);
        }

        /// <summary>
        /// Invoked by interop when the upload is complete.
        /// </summary>
        [JSInvokable("OnUploadComplete")]
        public async Task OnUploadComplete(string response)
        {
            System.Text.Json.JsonDocument doc = null;

            if (!string.IsNullOrEmpty(response))
            {
                try
                {
                    doc = System.Text.Json.JsonDocument.Parse(response);
                }
                catch (System.Text.Json.JsonException)
                {
                    //
                }
            }

            await UploadComplete.InvokeAsync(new UploadCompleteEventArgs() { RawResponse = response, JsonResponse = doc });
        }
    }
}
