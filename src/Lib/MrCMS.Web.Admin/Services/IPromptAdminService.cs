using System.Threading.Tasks;
using MrCMS.AI;
using MrCMS.AI.Entities;
using MrCMS.AI.Models;
using MrCMS.Web.Admin.Models.Prompts;
using X.PagedList;

namespace MrCMS.Web.Admin.Services;

public interface IPromptAdminService
{
    Task<IPagedList<Prompt>> GetPrompts(PromptSearchModel search);
    Task<UpdatePromptModel> AddPrompt(AddPromptModel model);
    Task<UpdatePromptModel> GetEditModel(int id);
    Task<UpdatePromptModel> UpdatePrompt(UpdatePromptModel model);
    Task DeletePrompt(int id);
    Task<bool> IsUniqueName(string name, PromptType type, int? id);
}