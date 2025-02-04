using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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
        // Wrap everything inside a container with the provided CSS classes plus "prompt-editor"
        output.TagName = "div";
        output.Attributes.SetAttribute("class", $"{CssClass} prompt-editor");

        var sb = new StringBuilder();
        
        

        // Render the main visible text area for the final prompt.
        // Adjust rows/cols as needed.
        var textArea = _htmlGenerator.GenerateTextArea(
            ViewContext,
            For.ModelExplorer,
            For.Name,
            rows: 5,
            columns: 50,
            htmlAttributes: new { @class = "form-control prompt-textarea" }
        );
        sb.AppendLine($"<div class='float-button-container'>{textArea.GetString()}<button type='button' class='btn btn-outline-secondary py-1 px-2 float-button open-prompt-modal' data-toggle='modal' data-target='#promptModal' title='Choose Template'><i class='fa fa-list-ul' aria-hidden='true'></i></button> </div>");

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

        // Bootstrap 4 Modal markup.
        sb.AppendLine(@"
                <div class='modal fade' id='promptModal' tabindex='-1' role='dialog' aria-hidden='true'>
                    <div class='modal-dialog modal-lg' role='document'>
                        <div class='modal-content'>
                            <div class='modal-header'>
                                <h5 class='modal-title'>Select a Prompt Template</h5>
                                <button type='button' class='close' data-dismiss='modal' aria-label='Close'>
                                    <span aria-hidden='true'>&times;</span>
                                </button>
                            </div>
                            <div class='modal-body'>");

        if (prompts.Any())
        {
            // The list group has a max-height with scrolling enabled.
            sb.AppendLine("<ul class='list-group' style='max-height:300px; overflow-y:auto;'>");

            // Regex to detect keys in the prompt template (keys delimited with curly braces, e.g. {UserName})
            var keyPattern = new Regex(@"\{(?<key>\w+)\}");

            foreach (var prompt in prompts)
            {
                // Sanitize template for attribute usage.
                var sanitizedTemplate = prompt.Template.Replace("'", "&#39;");

                // Find keys in the template.
                var matches = keyPattern.Matches(prompt.Template);
                string keysJson;

                if (matches.Count > 0)
                {
                    // Create an array of distinct keys.
                    var keys = matches
                        .Cast<Match>()
                        .Select(m => m.Groups["key"].Value)
                        .Distinct()
                        .ToArray();
                    keysJson = JsonSerializer.Serialize(keys);
                }
                else
                {
                    keysJson = "[]";
                }

                // Include both the template and its keys as data attributes.
                sb.AppendFormat(
                    "<li class='list-group-item list-group-item-action prompt-select' role='button' data-title='{0}' data-template='{1}' data-keys='{2}'>{3}</li>",
                    prompt.Name,
                    sanitizedTemplate,
                    keysJson,
                    prompt.Name
                );
            }

            sb.AppendLine("</ul>");
        }
        else
        {
            // Display a friendly message if no prompts are available.
            sb.AppendLine(@"
                    <div class='text-center'>
                        <p>No prompt templates available.</p>
                        <p><a href='/Admin/Prompt'>Click here to add a new prompt template.</a></p>
                    </div>");
        }

        // Hidden textarea for displaying the selected template (if needed for client-side processing)
        sb.AppendLine(@"
                <textarea class='form-control prompt-template-area d-none mt-2' rows='3' placeholder='Selected prompt template will appear here...'></textarea>
                <div class='dynamic-inputs mt-2'></div>
                <button type='button' class='btn btn-success apply-prompt-button mt-2 d-none'>Apply Prompt</button>
            ");

        sb.AppendLine(@"
                            </div>
                        </div>
                    </div>
                </div>");

        return sb.ToString();
    }
}