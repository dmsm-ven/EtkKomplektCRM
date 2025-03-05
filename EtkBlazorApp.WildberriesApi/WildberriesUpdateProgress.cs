namespace EtkBlazorApp.WildberriesApi;

public class WildberriesUpdateProgress
{
    public int CurrentStep { get; }
    public int TotalSteps { get; }
    public string CurrentStepDescription { get; }

    public bool IsCompleted => CurrentStep == TotalSteps;

    public WildberriesUpdateProgress(int step, int totalSteps, string description)
    {
        CurrentStep = step;
        TotalSteps = totalSteps;
        CurrentStepDescription = description;
    }
}
