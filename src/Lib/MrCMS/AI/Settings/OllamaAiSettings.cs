namespace MrCMS.AI.Settings;

public class OllamaAiSettings : AiSettingsBase
{
    public OllamaAiSettings()
    {
        Model = "qwen2.5-coder:14b";
        ApiUrl = "http://localhost:11434/api/generate";
    }
    public string Model { get; set; }
    public string ApiUrl { get; set; }
    
    public override bool RenderInSettings => true;
}