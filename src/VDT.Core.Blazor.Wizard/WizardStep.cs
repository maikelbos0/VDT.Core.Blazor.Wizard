using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Threading.Tasks;

namespace VDT.Core.Blazor.Wizard;

/// <summary>
/// Step as part of a wizard
/// </summary>
public class WizardStep : ComponentBase {
    [CascadingParameter] internal Wizard? Wizard { get; set; }

    /// <summary>
    /// Title of the wizard step to display in the wizard menu
    /// </summary>
    [Parameter] public string? Title { get; set; }

    /// <summary>
    /// Content of a wizard step such as an explanation or an <see cref="EditForm"/>
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// A callback that will be invoked when the step is rendered
    /// </summary>
    [Parameter] public EventCallback<WizardStepInitializedEventArgs> OnInitialize { get; set; }

    /// <summary>
    /// A callback that will be invoked when the user tries to go to the next step
    /// </summary>
    [Parameter] public EventCallback<WizardStepAttemptedCompleteEventArgs> OnTryComplete { get; set; }

    /// <summary>
    /// Indicates whether or not the wizard step is currently active
    /// </summary>
    public bool IsActive => Wizard?.ActiveStep == this;

    /// <inheritdoc/>
    protected override void OnInitialized() => Wizard?.AddStep(this);

    internal async Task Initialize() {
        var args = new WizardStepInitializedEventArgs();

        await OnInitialize.InvokeAsync(args);
    }

    internal async Task<bool> TryComplete() {
        var args = new WizardStepAttemptedCompleteEventArgs();

        await OnTryComplete.InvokeAsync(args);

        return !args.IsCancelled;
    }
}
