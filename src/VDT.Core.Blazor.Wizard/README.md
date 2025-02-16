# VDT.Core.Blazor.Wizard

Blazor component that helps you create wizard components with sequential steps, forward/back navigation, conditional completion of steps, step- and wizard-level
events, and more

## Features

- Fully customizable layout
- Events for starting, stopping and completing the wizard
- Steps completed only on your own conditions
- Events for initializing and attempted competion of steps
- Optional back navigation and cancelling

## Styling

The wizard can be provided with the following CSS classes:

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

## Interactivity

Aside from the Blazor properties shown in the above example, the wizard provides the following readonly properties to access state information:

- `IsActive` indicates whether or not the wizard is currently active
- `ActiveStepIndex` is the index of the currently active step if the wizard is active; otherwise `null`
- `ActiveStep` is the currently active step if the wizard is active; otherwise `null`
- `IsFirstStepActive` indicates if the first step in the wizard is the currently active one
- `IsLastStepActive` indicates if the last step in the wizard is the currently active one
- `AllSteps` returns all available wizard steps in order of display

The wizard provides the following methods to manipulate its state. These methods can also be accessed from the layout context using `context.Wizard`.

- `Start()` opens the wizard at the first step if it's not currently active
- `Stop()` closes and reset the wizard if it's currently active
- `GoToPreviousStep()` goes to the previous step in this wizard if it is active and it's not on the first step
- `TryCompleteStep()` attempts to complete the current step, then either move to the next step or complete the wizard
- `GoToStep(...)` navigates to a specific step in the wizard if it is active, after optionally attempting to complete the currently active step
  - Navigating to the step after the last available step completes and resets the wizard
  - Available overloads:  
    `GoToStep(WizardStep step, bool tryCompleteStep)`  
    `GoToStep(int stepIndex, bool tryCompleteStep)`  
    `GoToStep(StepIndexProvider stepIndexProvider, bool tryCompleteStep)`  

Aside from the Blazor properties shown in the above example, the currently active wizard step provides the following readonly properties to access state
information:

- `IsActive` indicates whether or not the wizard step is currently active
- `IsCompleted` indicates whether or not this step has been previously completed; resets to `false` when the wizard is closed

## Layout

A default layout is provided without any styling, but it's easy to supply your own layout using the layout context of a wizard. This gives you fine-grained
control over which elements you want to render and which markup to use around them.

### Available layout elements

- `DefaultLayout` renders the full default layout
- `Title` renders the `RenderFragment` `TitleContent`
- `StepTitles` renders the titles of all steps
- `Buttons` renders all buttons as enabled and needed in the following order:
  - Cancel
  - Previous
  - Next / Finish
- `ButtonCancel` renders the cancel button only if enabled
- `ButtonPrevious` renders the previous button only if enabled and a previous step is available
- `ButtonNext` renders the next button if a next step is available
- `ButtonFinish` renders the finish button if no next step is available
- `ActiveStepContent` renders the content of the currently active step

### Example

<Wizard ButtonClass="wizard-button"
        ButtonPreviousClass="wizard-button-secondary"
        ButtonPreviousText="<< Prev"
        ButtonFinishClass="wizard-button-primary"
        ButtonFinishText="Complete"
        ButtonNextClass="wizard-button-primary"
        ButtonNextText="Next >>">
    <Layout>
        <div class="wizard">
            <div class="wizard-title">
                <h1>My wizard</h1>
            </div>

            <div class="wizard-body">
                @context.ActiveStepContent
            </div>

            <div class="wizard-buttons">
                @context.ButtonNext
                @context.ButtonFinish
            </div>
        </div>
    </Layout>
    <Steps>
        <WizardStep Title="The first step">
            Test step 1
        </WizardStep>
        <WizardStep Title="Another">
            Test step 2
        </WizardStep>
        <WizardStep Title="Summary">
            Test step 3
        </WizardStep>
    </Steps>
</Wizard>
```