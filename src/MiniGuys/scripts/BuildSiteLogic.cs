namespace MiniGuys;

using System;

public class BuildSiteLogic
{
    public BuildingType Type { get; }
    public int LabourRequired { get; }
    public int LabourApplied { get; private set; }
    public int LabourRemaining => LabourRequired - LabourApplied;
    public bool IsComplete { get; private set; }

    public event Action? Completed;

    public BuildSiteLogic(BuildingType type, int labourRequired)
    {
        Type = type;
        LabourRequired = labourRequired;
    }

    public int ContributeLabour(int amount)
    {
        if (IsComplete)
            return 0;

        var applied = Math.Min(amount, LabourRemaining);
        LabourApplied += applied;

        if (LabourApplied >= LabourRequired)
        {
            IsComplete = true;
            Completed?.Invoke();
        }

        return applied;
    }
}
