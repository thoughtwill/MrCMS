using MrCMS.AI.Models;
using MrCMS.Entities;

namespace MrCMS.AI.Entities;

public class Prompt : SystemEntity
{
    public virtual string Name { get; set; }
    public virtual string Template { get; set; }
    public virtual PromptType Type { get; set; }
}