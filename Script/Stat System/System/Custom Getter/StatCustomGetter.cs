using GeneralGameDevKit.StatSystem;

public abstract class StatCustomGetter
{
    public bool IgnoreModifier;
    public bool UseModifierHooks;
    public string StatIdToHookModifier;

    public abstract float GetProcessedValue(StatSystemCore processingSystem, string currentProcessingStatKey, float currentValue);
}
