using System;

namespace VDT.Core.Blazor.Wizard;

/// <summary>
/// Supplies information about a wizard attempting to complete a step event that is being raised
/// </summary>
public class WizardStepAttemptedCompleteEventArgs : EventArgs {
    /// <summary>
    /// Indicates if this step was previously completed and navigation has gone back to this step
    /// </summary>
    public bool WasPreviouslyCompleted { get; init; }

    /// <summary>
    /// Indicates if completion of the step should be cancelled; set this to <see langword="true"/> if the wizard should not continue
    /// </summary>
    public bool IsCancelled { get; set; }
}
