namespace VDT.Core.Blazor.Wizard;

/// <summary>
/// Signature for methods that transform the current active step index into a new step index
/// </summary>
/// <param name="currentStepIndex">Index of the currently active step in the wizard</param>
/// <returns>The new step index</returns>
public delegate int StepIndexProvider(int currentStepIndex);
