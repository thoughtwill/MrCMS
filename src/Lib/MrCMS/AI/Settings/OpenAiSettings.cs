namespace MrCMS.AI.Settings;

public class OpenAiSettings : AiSettingsBase
{
    public OpenAiSettings()
    {
        Model = "gpt-4o";
        ApiKey = string.Empty;
    }

    public string Model { get; set; }
    public string ApiKey { get; set; }

    public override bool RenderInSettings => true;
}