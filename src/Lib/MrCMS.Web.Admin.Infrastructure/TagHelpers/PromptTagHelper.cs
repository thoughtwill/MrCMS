using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MrCMS.AI.Models;
using MrCMS.Shortcodes.Forms;
using MrCMS.Web.Admin.Infrastructure.Services;

namespace MrCMS.Web.Admin.Infrastructure.TagHelpers;

[HtmlTargetElement("Prompt", Attributes = "asp-for")]
public class PromptTagHelper : TagHelper
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IPromptService _promptService;

    public PromptTagHelper(IHtmlGenerator htmlGenerator, IPromptService promptService)
    {
        _htmlGenerator = htmlGenerator;
        _promptService = promptService;
    }

    [HtmlAttributeName("asp-for")] public ModelExpression For { get; set; }

    [HtmlAttributeName("class")] public string CssClass { get; set; }

    /// <summary>
    /// Filters prompt templates by type.
    /// </summary>
    [HtmlAttributeName("Type")]
    public PromptType Type { get; set; }

    [ViewContext] [HtmlAttributeNotBound] public ViewContext ViewContext { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Wrap everything inside a container with the class 'prompt-editor'
        output.TagName = "div";
        output.Attributes.SetAttribute("class", $"{CssClass} prompt-editor");

        var sb = new StringBuilder();

        // Render the main visible text area for the final prompt.
        // (You can customize rows/cols as needed.)
        var textArea = _htmlGenerator.GenerateTextArea(
            ViewContext, 
            For.ModelExplorer,  
            For.Name, 
            5,             
            50,                  
            new { @class = "form-control prompt-textarea" } // HTML attributes
        );
        sb.AppendLine(textArea.GetString());

        // Render a button to open the prompt selection modal.
        sb.AppendLine(@"
                <button type='button' class='btn btn-primary mt-2 open-prompt-modal' data-bs-toggle='modal' data-bs-target='#promptModal'>
                    Choose Template
                </button>");

        // Append the modal markup for the prompt editor.
        sb.AppendLine(await GetCustomPromptEditorUi());

        output.Content.AppendHtml(sb.ToString());
    }

    /// <summary>
    /// Builds the custom prompt editor UI (the modal).
    /// </summary>
    private async Task<string> GetCustomPromptEditorUi()
    {
        var prompts = await _promptService.GetPrompts(Type); // Retrieves prompts filtered by the provided type.
        var sb = new StringBuilder();

        // Modal markup for the prompt editor.
        sb.AppendLine(@"
                <div class='modal fade' id='promptModal' tabindex='-1' aria-hidden='true'>
                    <div class='modal-dialog modal-lg'>
                        <div class='modal-content'>
                            <div class='modal-header'>
                                <h5 class='modal-title'>Select a Prompt Template</h5>
                                <button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button>
                            </div>
                            <div class='modal-body'>");

        // If prompts exist, list them; otherwise, display a message with a link.
        if (prompts.Any())
        {
            sb.AppendLine("<ul class='list-group'>");
            foreach (var prompt in prompts)
            {
                sb.AppendFormat("<li class='list-group-item prompt-select' data-template='{0}'>{1}</li>",
                    prompt.Template.Replace("'", "&#39;"),
                    prompt.Name);
            }

            sb.AppendLine("</ul>");
        }
        else
        {
            sb.AppendLine(@"
                    <div class='text-center'>
                        <p>No prompt templates available.</p>
                        <p><a href='/Admin/Prompt'>Click here to add a new prompt template.</a></p>
                    </div>");
        }

        // Textarea for displaying the selected prompt template and container for dynamic inputs.
        sb.AppendLine(@"
                                <textarea class='form-control prompt-template-area mt-2' rows='3' placeholder='Selected prompt template will appear here...'></textarea>
                                <div class='dynamic-inputs mt-2'></div>
                                <button type='button' class='btn btn-success apply-prompt-button mt-2'>Apply Prompt</button>
                            </div>
                        </div>
                    </div>
                </div>");

        return sb.ToString();
    }
}