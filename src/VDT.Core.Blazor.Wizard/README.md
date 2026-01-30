# VDT.Core.Blazor.Wizard

Blazor component that helps you create wizard components with sequential steps, forward/back navigation, conditional completion of steps, step- and wizard-level
events, and more

## Features

- Fully customizable layout
- Events for starting, stopping and completing the wizard
- Steps completed only on your own conditions
- Events for initializing and attempted competion of steps
- Optional back navigation and cancelling

## Example

```
<div>
    <button @onclick="async () => await Wizard.Start()" class="btn btn-primary">Start wizard</button>
    <button @onclick="() => Wizard.Stop()" class="btn btn-secondary">Stop wizard</button>
</div>

<Wizard @ref="Wizard"
        OnStart="Start"
        OnStop="Stop"
        OnFinish="Finish"
        ContainerClass="wizard"
        TitleContainerClass="wizard-title"
        StepTitleContainerClass="wizard-steps"
        StepTitleClass="wizard-step"
        ActiveStepTitleClass="active"
        AllowCancel="true"
        AllowPrevious="true"
        ButtonContainerClass="wizard-buttons"
        ButtonClass="wizard-button"
        ButtonCancelClass="wizard-button-secondary"
        ButtonCancelText="Stop"
        ButtonPreviousClass="wizard-button-secondary"
        ButtonPreviousText="<< Prev"
        ButtonFinishClass="wizard-button-primary"
        ButtonFinishText="Complete"
        ButtonNextClass="wizard-button-primary"
        ButtonNextText="Next >>"
        ContentContainerClass="wizard-body">
    <TitleContent>
        <h3>Wizard title</h3>
    </TitleContent>
    <Steps>
        <WizardStep OnInitialize="args => InitializeStep(args, 1)" OnTryComplete="args => TryCompleteStep(args, 1)" Title="The first step">
            Test step 1
        </WizardStep>
        <WizardStep OnInitialize="args => InitializeStep(args, 2)" OnTryComplete="args => TryCompleteStep(args, 2)" Title="Another">
            <div class="form-check">
                <input id="goNext" type="checkbox" @bind="GoNext" class="form-check-input" />
                <label for="goNext" class="form-check-label">Go next?</label>
            </div>
            <div>
                Test step 2
            </div>
        </WizardStep>
        <WizardStep OnInitialize="args => InitializeStep(args, 3)" OnTryComplete="args => TryCompleteStep(args, 3)" Title="Summary">
            Test step 3
        </WizardStep>
    </Steps>
</Wizard>

<pre>
    @StepData
</pre>

@code {
    public Wizard Wizard { get; set; }

    public bool GoNext { get; set; } = true;
    public string StepData = string.Empty;

    public void Start(WizardStartedEventArgs args) {
        StepData += $"{nameof(Start)} wizard{Environment.NewLine}";
    }

    public void Stop(WizardStoppedEventArgs args) {
        StepData += $"{nameof(Stop)} wizard{Environment.NewLine}";
    }

    public void Finish(WizardFinishedEventArgs args) {
        StepData += $"{nameof(Finish)} wizard{Environment.NewLine}";
    }

    public void InitializeStep(WizardStepInitializedEventArgs args, int step) {
        StepData += $"{nameof(InitializeStep)} step {step}{Environment.NewLine}";

        GoNext = true;
    }

    public void TryCompleteStep(WizardStepAttemptedCompleteEventArgs args, int step) {
        StepData += $"{nameof(TryCompleteStep)} step {step}{Environment.NewLine}";

        args.IsCancelled = !GoNext;
    }

}
```

## CSS class properties

- `ContainerClass` gets applied to a `div` surrounding the entire wizard when using the default layout
- `TitleContainerClass` gets applied to a `div` surrounding the title content when using the default layout
- `StepTitleContainerClass` gets applied to a `div` surrounding the step titles when using the default layout
- `StepTitleClass` gets applied to the step title `div`s
- `ActiveStepTitleClass` gets applied to the active step title `div`
- `ButtonContainerClass` gets applied to a `div` surrounding the navigation buttons when using the default layout
- `ButtonClass` gets applied to all navigation buttons
- `ButtonCancelClass` gets applied to the cancel button
- `ButtonPreviousClass` gets applied to the previous button
- `ButtonNextClass` gets applied to the next button
- `ButtonFinishClass` gets applied to the finish button
- `ContentContainerClass` gets applied to a `div` surrounding the content of the currently active step when using the default layout

## Events

You can subscribe to several events during the wizard's life cycle; most are informational only but some allow you to manipulate the wizard's behaviour.

- The `Wizard.OnStart` event fires when the wizard is started.
- The `Wizard.OnStop` event fires when the wizard is stopped.
- The `Wizard.OnFinish` event fires when the wizard is finished because all steps of the wizard have been completed.
- The `WizardStep.OnInitialize` event fires when a step is rendered.
- The `WizardStep.OnTryComplete` event fires when a step is completed by clicking the Next button. The provided `WizardStepAttemptedCompleteEventArgs` lets you
  see if the step has been previously completed and lets you cancel completion of a step.

## Custom layout

If the default wizard layout does not suffice, it's easy to customize the layout by using the `Layout` renderfragment and the various renderfragments found on
the `context` provided by it. If you provide a layout template the default container elements will not be used and their CSS classes will not be applied, but
any properties for specific renderfragments for buttons or other common elements, such as CSS, titles and the various allow options will still be applied.

### Renderfragments

- `context.DefaultLayout` renders the wizard title content
- `context.Title` renders the wizard title content
- `context.StepTitles` renders the wizard step titles
- `context.Buttons` renders the wizard cancel, previous, next and finish buttons if they are available and enabled
- `context.ButtonCancel` renders the wizard cancel button if enabled
- `context.ButtonPrevious` renders the wizard previous button if enabled and there is a previous step
- `context.ButtonNext` renders the wizard next button if there is a next step
- `context.ButtonFinish` renders the wizard finish button if this is the final step
- `context.ActiveStepContent` renders the wizard active step content

For further customization, you can create a layout without using any renderfragments, instead using only properties and methods of the wizard that can be
accessed via <code>context.Wizard</code>.

### Wizard properties

This does not list Blazor parameters such as button texts or any of the CSS classes.

- `IsActive` indicates whether or not the wizard is currently active
- `ActiveStepIndex` is the index of the currently active step if the wizard is active, otherwise `null`
- `ActiveStep` is the currently active step if the wizard is active, otherwise `null`
- `IsFirstStepActive` indicates if the first step in the wizard is the currently active one
- `IsLastStepActive` indicates if the last step in the wizard is the currently active one
- `AllSteps` returns all available wizard steps in order of display

### Wizard methods

- `Start()` opens the wizard at the first step if it's not currently active
- `Stop()` closes and reset the wizard if it's currently active
- `GoToPreviousStep()` goes to the previous step in this wizard if it is active and it's not on the first step
- `TryCompleteStep()` attempts to complete the current step, then either move to the next step or complete the wizard
- `GoToStep(...)` navigates to a specific step in the wizard if it is active, after optionally attempting to complete the currently active step
  - Navigating to the step after the last available step completes and resets the wizard
  - Available overloads:
    - `GoToStep(WizardStep step, bool tryCompleteStep)`
    - `GoToStep(int stepIndex, bool tryCompleteStep)`
    - `GoToStep(StepIndexProvider stepIndexProvider, bool tryCompleteStep)`

### Wizard step properties

This does not list Blazor parameters such as `Title`.

- `IsActive` indicates whether or not the wizard step is currently active
- `IsCompleted` indicates whether or not this step has been previously completed; resets to `false` when the wizard is closed

### Example

This example wizard does not use any of the renderfragments, instead showing how to create a completely new layout with the same functionality and added
navigation and indication for completed steps.

```
<Wizard @ref="Wizard">
    <TitleContent>
        <h2>Wizard title</h2>
    </TitleContent>
    <Layout>
        <div class="card">
            <div class="card-header">
                @context.Title
            </div>
            <div>
                <div class="d-flex align-items-stretch">
                    <div class="flex-grow-0 border-end bg-light px-3 py-2">
                        @foreach (var step in context.Wizard.AllSteps) {
                            <div class="me-3">
                                @if (step.IsActive) {
                                    <span class="fw-bold">@step.Title</span>
                                }
                                else if (step.IsCompleted) {
                                    <button class="btn btn-link p-0 align-baseline" @onclick="() => context.Wizard.GoToStep(step, false)">@step.Title</button>
                                }
                                else {
                                    <span>@step.Title</span>
                                }

                                @if (step.IsCompleted) {
                                    <span class="text-success fw-bold ps-1">&check;</span>
                                }
                            </div>
                        }
                    </div>
                    <div class="flex-grow-1 px-3 py-2">
                        @context.ActiveStepContent
                    </div>
                </div>
            </div>
            <div class="card-footer d-flex">
                <div class="flex-grow-1">
                    <button class="btn btn-secondary" @onclick="context.Wizard.Stop">Stop</button>
                </div>

                <div class="flex-grow-0 btn-group">
                    @if (!context.Wizard.IsFirstStepActive) {
                        <button class="btn btn-secondary" @onclick="context.Wizard.GoToPreviousStep">&lt;&lt; Prev</button>
                    }

                    @if (context.Wizard.IsLastStepActive){
                        <button class="btn btn-primary" @onclick="context.Wizard.TryCompleteStep">Complete</button>
                    }
                    else {
                        <button class="btn btn-primary" @onclick="context.Wizard.TryCompleteStep">Next &gt;&gt;</button>
                    }
                </div>
            </div>
        </div>
    </Layout>
    <Steps>
        <WizardStep Title="Introduction">
            <p>
                This is an example wizard with a custom layout. Please click Next to continue.
            </p>
        </WizardStep>
        <WizardStep Title="Your step here">
            <p>
                This is the second step in this wizard. Please click Next to continue.
            </p>
        </WizardStep>
        <WizardStep Title="Summary">
            <p>
                Please click Complete to finish the wizard.
            </p>
        </WizardStep>
    </Steps>
</Wizard>

@code {
    private Wizard? Wizard { get; set; }
}
```
