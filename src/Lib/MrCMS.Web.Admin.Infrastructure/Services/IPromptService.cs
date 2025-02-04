using System.Collections.Generic;
using System.Threading.Tasks;
using MrCMS.AI;
using MrCMS.AI.Entities;
using MrCMS.AI.Models;

namespace MrCMS.Web.Admin.Infrastructure.Services;

public interface IPromptService
{
    Task<List<Prompt>> GetPrompts(PromptType type);
}