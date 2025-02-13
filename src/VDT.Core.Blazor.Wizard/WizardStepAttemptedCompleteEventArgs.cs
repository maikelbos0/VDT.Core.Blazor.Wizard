using System;

namespace VDT.Core.Blazor.Wizard;

/// <summary>
/// Supplies information about a wizard attempting to complete a step event that is being raised
/// </summary>
public class WizardStepAttemptedCompleteEventArgs : EventArgs {
    /// <summary>
    /// Indicates if completion of the step should be cancelled; set to true if the wizard should not continue
    /// </summary>
    public bool IsCancelled { get; set; }
}
