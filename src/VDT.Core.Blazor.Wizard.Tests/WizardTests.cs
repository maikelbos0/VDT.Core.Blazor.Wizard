using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using Xunit;

namespace VDT.Core.Blazor.Wizard.Tests;

public class WizardTests {
    [Theory]
    [InlineData(null)]
    [InlineData(2)]
    public void Wizard_ActiveStep_Returns_Null_Correctly(int? activeStepIndex) {
        var wizard = new Wizard() {
            ActiveStepIndex = activeStepIndex
        };

        wizard.StepsInternal.Add(new WizardStep());
        wizard.StepsInternal.Add(new WizardStep());

        Assert.Null(wizard.ActiveStep);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Wizard_ActiveStep_Returns_Step_Correctly(int activeStepIndex) {
        var wizard = new Wizard() {
            ActiveStepIndex = activeStepIndex
        };

        wizard.StepsInternal.Add(new WizardStep());
        wizard.StepsInternal.Add(new WizardStep());

        Assert.Equal(wizard.StepsInternal[activeStepIndex], wizard.ActiveStep);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    public void Wizard_IsFirstStepActive_Works(int? activeStepIndex, bool expectedActive) {
        var wizard = new Wizard() {
            ActiveStepIndex = activeStepIndex
        };

        wizard.StepsInternal.Add(new WizardStep());
        wizard.StepsInternal.Add(new WizardStep());

        Assert.Equal(expectedActive, wizard.IsFirstStepActive);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    public void Wizard_IsLastStepActive_Works(int? activeStepIndex, bool expectedActive) {
        var wizard = new Wizard() {
            ActiveStepIndex = activeStepIndex
        };

        wizard.StepsInternal.Add(new WizardStep());
        wizard.StepsInternal.Add(new WizardStep());

        Assert.Equal(expectedActive, wizard.IsLastStepActive);
    }

    [Fact]
    public async Task Wizard_Start_Works() {
        WizardStartedEventArgs? arguments = null;
        bool stateHasChangedInvoked = false;
        var wizard = new Wizard() {
            OnStart = EventCallback.Factory.Create<WizardStartedEventArgs>(this, args => arguments = args),
            StateHasChangedHandler = () => stateHasChangedInvoked = true
        };

        await wizard.Start();

        Assert.Equal(0, wizard.ActiveStepIndex);
        Assert.NotNull(arguments);
        Assert.True(stateHasChangedInvoked);
    }

    [Fact]
    public async Task Wizard_Start_Does_Nothing_When_Active() {
        WizardStartedEventArgs? arguments = null;
        var wizard = new Wizard() {
            OnStart = EventCallback.Factory.Create<WizardStartedEventArgs>(this, args => arguments = args),
            ActiveStepIndex = 2
        };

        await wizard.Start();

        Assert.Equal(2, wizard.ActiveStepIndex);
        Assert.Null(arguments);
    }

    [Fact]
    public async Task Wizard_Stop_Works() {
        WizardStoppedEventArgs? arguments = null;
        var wizard = new Wizard() {
            OnStop = EventCallback.Factory.Create<WizardStoppedEventArgs>(this, args => arguments = args),
            ActiveStepIndex = 2
        };

        await wizard.Stop();

        Assert.Null(wizard.ActiveStepIndex);
        Assert.NotNull(arguments);
    }

    [Fact]
    public async Task Wizard_Stop_Does_Nothing_When_Inactive() {
        WizardStoppedEventArgs? arguments = null;
        var wizard = new Wizard() {
            OnStop = EventCallback.Factory.Create<WizardStoppedEventArgs>(this, args => arguments = args)
        };

        await wizard.Stop();

        Assert.Null(arguments);
    }

    [Fact]
    public async Task Wizard_AddStep_Works() {
        var wizard = new Wizard() {
            ActiveStepIndex = 0
        };
        var step = new WizardStep();

        await wizard.AddStep(step);

        Assert.Equal(step, Assert.Single(wizard.StepsInternal));
    }

    [Fact]
    public async Task Wizard_AddStep_Initializes_First_Step() {
        WizardStepInitializedEventArgs? arguments = null;
        var wizard = new Wizard() {
            ActiveStepIndex = 0
        };
        var step = new WizardStep() {
            OnInitialize = EventCallback.Factory.Create<WizardStepInitializedEventArgs>(this, args => arguments = args)
        };

        await wizard.AddStep(step);

        Assert.NotNull(arguments);
    }

    [Fact]
    public async Task Wizard_AddStep_Does_Not_Initialize_Subsequent_Steps() {
        WizardStepInitializedEventArgs? arguments = null;
        var wizard = new Wizard() {
            ActiveStepIndex = 0
        };
        var step = new WizardStep() {
            OnInitialize = EventCallback.Factory.Create<WizardStepInitializedEventArgs>(this, args => arguments = args)
        };

        await wizard.AddStep(new WizardStep());
        await wizard.AddStep(step);

        Assert.Null(arguments);
    }

    [Fact]
    public async Task Wizard_AddStep_Does_Nothing_For_Existing_Step() {
        var wizard = new Wizard() {
            ActiveStepIndex = 0
        };
        var step = new WizardStep();

        await wizard.AddStep(step);
        await wizard.AddStep(step);

        Assert.Equal(step, Assert.Single(wizard.StepsInternal));
    }

    [Fact]
    public async Task Wizard_GoToPreviousStep_Works() {
        WizardStepInitializedEventArgs? arguments = null;
        var wizard = new Wizard() {
            ActiveStepIndex = 1
        };
        var step = new WizardStep() {
            OnInitialize = EventCallback.Factory.Create<WizardStepInitializedEventArgs>(this, args => arguments = args)
        };

        wizard.StepsInternal.Add(step);

        await wizard.GoToPreviousStep();

        Assert.Equal(0, wizard.ActiveStepIndex);
        Assert.NotNull(arguments);
    }

    [Fact]
    public async Task Wizard_TryCompleteStep_Can_Be_Cancelled() {
        var wizard = new Wizard() {
            ActiveStepIndex = 0
        };
        var step = new WizardStep() {
            OnTryComplete = EventCallback.Factory.Create<WizardStepAttemptedCompleteEventArgs>(this, args => args.IsCancelled = true)
        };

        wizard.StepsInternal.Add(step);

        await wizard.TryCompleteStep();

        Assert.Equal(0, wizard.ActiveStepIndex);
    }

    [Fact]
    public async Task Wizard_TryCompleteStep_Initializes_Next_Step() {
        WizardStepInitializedEventArgs? arguments = null;
        var wizard = new Wizard() {
            ActiveStepIndex = 0
        };
        var step = new WizardStep() {
            OnInitialize = EventCallback.Factory.Create<WizardStepInitializedEventArgs>(this, args => arguments = args)
        };

        wizard.StepsInternal.Add(new WizardStep());
        wizard.StepsInternal.Add(step);

        await wizard.TryCompleteStep();

        Assert.Equal(1, wizard.ActiveStepIndex);
        Assert.NotNull(arguments);
    }

    [Fact]
    public async Task Wizard_TryCompleteStep_Resets_When_Finished() {
        var wizard = new Wizard() {
            ActiveStepIndex = 0
        };

        wizard.StepsInternal.Add(new WizardStep());

        await wizard.TryCompleteStep();

        Assert.Null(wizard.ActiveStepIndex);
        Assert.Empty(wizard.StepsInternal);
    }
}
