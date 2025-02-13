using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VDT.Core.Blazor.Wizard;

/// <summary>
/// Component that renders a series of steps to be completed by the user
/// </summary>
public class Wizard : ComponentBase {
    /// <summary>
    /// Constructs an instance of a wizard
    /// </summary>
    public Wizard() {
        LayoutContext = new WizardLayoutContext(this);
    }

    /// <summary>
    /// CSS class to apply to the wizard container; this value will not be used if a layout is specified
    /// </summary>
    [Parameter] public string? ContainerClass { get; set; }

    /// <summary>
    /// CSS class to apply to the wizard title section; this value will not be used if a layout is specified
    /// </summary>
    [Parameter] public string? TitleContainerClass { get; set; }

    /// <summary>
    /// Wizard title content
    /// </summary>
    [Parameter] public RenderFragment? TitleContent { get; set; }

    /// <summary>
    /// CSS class to apply to the wizard step title section; this value will not be used if a layout is specified
    /// </summary>
    [Parameter] public string? StepTitleContainerClass { get; set; }

    /// <summary>
    /// CSS class to apply to the wizard step titles
    /// </summary>
    [Parameter] public string? StepTitleClass { get; set; }

    /// <summary>
    /// CSS class to apply to the title of the active wizard step
    /// </summary>
    [Parameter] public string? ActiveStepTitleClass { get; set; }

    /// <summary>
    /// CSS class to apply to the button section; this value will not be used if a layout is specified
    /// </summary>
    [Parameter] public string? ButtonContainerClass { get; set; }

    /// <summary>
    /// CSS class to apply to all buttons
    /// </summary>
    [Parameter] public string? ButtonClass { get; set; }

    /// <summary>
    /// Show a button for stopping the wizard when unfinished
    /// </summary>
    [Parameter] public bool AllowCancel { get; set; }

    /// <summary>
    /// CSS class to apply to the button for stopping the wizad
    /// </summary>
    [Parameter] public string? ButtonCancelClass { get; set; }

    /// <summary>
    /// Text that is displayed on the button for stopping the wizard
    /// </summary>
    [Parameter] public string ButtonCancelText { get; set; } = "Cancel";

    /// <summary>
    /// Show a button for navigating to the previous step
    /// </summary>
    [Parameter] public bool AllowPrevious { get; set; }

    /// <summary>
    /// CSS class to apply to the button for the previous step
    /// </summary>
    [Parameter] public string? ButtonPreviousClass { get; set; }

    /// <summary>
    /// Text that is displayed on the button for the previous step
    /// </summary>
    [Parameter] public string ButtonPreviousText { get; set; } = "Previous";

    /// <summary>
    /// CSS class to apply to the button for the next step
    /// </summary>
    [Parameter] public string? ButtonNextClass { get; set; }

    /// <summary>
    /// Text that is displayed on the button for the next step
    /// </summary>
    [Parameter] public string ButtonNextText { get; set; } = "Next";

    /// <summary>
    /// CSS class to apply to the button for completing the wizard
    /// </summary>
    [Parameter] public string? ButtonFinishClass { get; set; }

    /// <summary>
    /// Text that is displayed on the button for completing the wizard
    /// </summary>
    [Parameter] public string ButtonFinishText { get; set; } = "Finish";

    /// <summary>
    /// CSS class to apply to the active step content section; this value will not be used if a layout is specified
    /// </summary>
    [Parameter] public string? ContentContainerClass { get; set; }

    /// <summary>
    /// Content containing the wizard steps in order
    /// </summary>
    [Parameter] public RenderFragment? Steps { get; set; }

    /// <summary>
    /// Layout for the wizard; a default layout will be used if this is not provided
    /// </summary>
    [Parameter] public RenderFragment<WizardLayoutContext>? Layout { get; set; }

    /// <summary>
    /// Indicates whether or not the wizard is currently active
    /// </summary>
    public bool IsActive => ActiveStepIndex.HasValue;

    /// <summary>
    /// A callback that will be invoked when the wizard is started
    /// </summary>
    [Parameter]
    public EventCallback<WizardStartedEventArgs> OnStart { get; set; }

    /// <summary>
    /// A callback that will be invoked when the wizard is stopped
    /// </summary>
    [Parameter]
    public EventCallback<WizardStoppedEventArgs> OnStop { get; set; }

    /// <summary>
    /// A callback that will be invoked when the wizard is finished because all steps of the wizard have been completed
    /// </summary>
    [Parameter]
    public EventCallback<WizardFinishedEventArgs> OnFinish { get; set; }

    internal List<WizardStep> StepsInternal { get; private init; } = [];

    internal WizardLayoutContext LayoutContext { get; private init; }

    internal int? ActiveStepIndex { get; set; }

    internal WizardStep? ActiveStep => ActiveStepIndex.HasValue && StepsInternal.Count > ActiveStepIndex.Value ? StepsInternal[ActiveStepIndex.Value] : null;

    internal bool IsFirstStepActive => ActiveStepIndex.HasValue && ActiveStepIndex.Value == 0;

    internal bool IsLastStepActive => ActiveStepIndex.HasValue && ActiveStepIndex.Value == StepsInternal.Count - 1;

    internal Action? StateHasChangedHandler { get; init; }

    /// <summary>
    /// Open the wizard at the first step if it's not currently active
    /// </summary>
    public async Task Start() {
        if (IsActive) {
            return;
        }

        ActiveStepIndex = 0;
        await OnStart.InvokeAsync(new WizardStartedEventArgs());
        (StateHasChangedHandler ?? StateHasChanged)();
    }

    /// <summary>
    /// Close and reset the wizard if it's currently active
    /// </summary>
    public async Task Stop() {
        if (!IsActive) {
            return;
        }

        Reset();
        await OnStop.InvokeAsync(new WizardStoppedEventArgs());
    }

    internal async Task AddStep(WizardStep step) {
        if (!StepsInternal.Contains(step)) {
            StepsInternal.Add(step);

            if (StepsInternal.Count == 1) {
                await ActiveStep!.Initialize();
            }
        }
    }

    /// <summary>
    /// Go to the previous step in this wizard if it is active and it's not on the first step
    /// </summary>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    public async Task<bool> GoToPreviousStep() {
        if (IsActive && !IsFirstStepActive) {
            ActiveStepIndex--;
            await ActiveStep!.Initialize();

            return true;
        }
        else {
            return false;
        }
    }

    /// <summary>
    /// Attempt to complete the current step, then either move to the next step or complete the wizard
    /// </summary>
    /// <returns><see langword="true"/> if successful, otherwise <see langword="false"/></returns>
    public async Task<bool> TryCompleteStep() {
        if (IsActive && await ActiveStep!.TryComplete()) {
            ActiveStepIndex++;

            if (ActiveStep == null) {
                Reset();
                await OnFinish.InvokeAsync(new WizardFinishedEventArgs());
            }
            else {
                await ActiveStep!.Initialize();
            }

            return true;
        }
        else {
            return false;
        }
    }

    private void Reset() {
        ActiveStepIndex = null;
        StepsInternal.Clear();
    }

    /// <inheritdoc/>
    protected override void BuildRenderTree(RenderTreeBuilder builder) => LayoutContext.CascadingValue(builder);
}
