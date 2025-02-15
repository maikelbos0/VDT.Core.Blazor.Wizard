using Microsoft.AspNetCore.Components;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace VDT.Core.Blazor.Wizard.Tests;

public class WizardTests {
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void ActiveStep(int activeStepIndex) {
        var wizard = new Wizard() {
            ActiveStepIndex = activeStepIndex
        };

        wizard.StepsInternal.Add(new WizardStep());
        wizard.StepsInternal.Add(new WizardStep());

        Assert.Equal(wizard.StepsInternal[activeStepIndex], wizard.ActiveStep);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(2)]
    public void ActiveStep_Is_Null_When_Out_Of_Bounds(int? activeStepIndex) {
        var wizard = new Wizard() {
            ActiveStepIndex = activeStepIndex
        };

        wizard.StepsInternal.Add(new WizardStep());
        wizard.StepsInternal.Add(new WizardStep());

        Assert.Null(wizard.ActiveStep);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    public void IsFirstStepActive(int? activeStepIndex, bool expectedActive) {
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
    public void IsLastStepActive(int? activeStepIndex, bool expectedActive) {
        var wizard = new Wizard() {
            ActiveStepIndex = activeStepIndex
        };

        wizard.StepsInternal.Add(new WizardStep());
        wizard.StepsInternal.Add(new WizardStep());

        Assert.Equal(expectedActive, wizard.IsLastStepActive);
    }

    [Fact]
    public async Task Start() {
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
    public async Task Start_Does_Nothing_When_Active() {
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
    public async Task Stop() {
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
    public async Task Stop_Does_Nothing_When_Inactive() {
        WizardStoppedEventArgs? arguments = null;
        var wizard = new Wizard() {
            OnStop = EventCallback.Factory.Create<WizardStoppedEventArgs>(this, args => arguments = args)
        };

        await wizard.Stop();

        Assert.Null(arguments);
    }

    [Fact]
    public async Task AddStep_First_Step() {
        WizardStepInitializedEventArgs? arguments = null;
        var wizard = new Wizard() {
            ActiveStepIndex = 0
        };
        var step = new WizardStep() {
            OnInitialize = EventCallback.Factory.Create<WizardStepInitializedEventArgs>(this, args => arguments = args)
        };

        await wizard.AddStep(step);

        Assert.Equal(step, Assert.Single(wizard.StepsInternal));
        Assert.NotNull(arguments);
    }

    [Fact]
    public async Task AddStep_Subsequent_Steps() {
        WizardStepInitializedEventArgs? arguments = null;
        var wizard = new Wizard() {
            ActiveStepIndex = 0
        };
        var step = new WizardStep() {
            OnInitialize = EventCallback.Factory.Create<WizardStepInitializedEventArgs>(this, args => arguments = args)
        };

        await wizard.AddStep(new WizardStep());
        await wizard.AddStep(step);

        Assert.Equal(2, wizard.StepsInternal.Count);
        Assert.Equal(step, wizard.StepsInternal.Last());
        Assert.Null(arguments);
    }

    [Fact]
    public async Task AddStep_Existing_Step() {
        var wizard = new Wizard() {
            ActiveStepIndex = 0
        };
        var step = new WizardStep();

        await wizard.AddStep(step);
        await wizard.AddStep(step);

        Assert.Equal(step, Assert.Single(wizard.StepsInternal));
    }

    [Fact]
    public async Task GoToPreviousStep() {
        WizardStepInitializedEventArgs? arguments = null;
        var wizard = new Wizard() {
            ActiveStepIndex = 1
        };
        var step = new WizardStep() {
            OnInitialize = EventCallback.Factory.Create<WizardStepInitializedEventArgs>(this, args => arguments = args)
        };

        wizard.StepsInternal.Add(step);

        Assert.True(await wizard.GoToPreviousStep());

        Assert.Equal(0, wizard.ActiveStepIndex);
        Assert.NotNull(arguments);
    }

    [Fact]
    public async Task GoToPreviousStep_Does_Nothing_When_Inactive() {

        WizardStepInitializedEventArgs? arguments = null;
        var wizard = new Wizard();
        var step = new WizardStep() {
            OnInitialize = EventCallback.Factory.Create<WizardStepInitializedEventArgs>(this, args => arguments = args)
        };

        wizard.StepsInternal.Add(step);

        Assert.False(await wizard.GoToPreviousStep());

        Assert.Null(wizard.ActiveStepIndex);
        Assert.Null(arguments);
    }

    [Fact]
    public async Task GoToPreviousStep_Does_Nothing_When_On_First_Step() {

        WizardStepInitializedEventArgs? arguments = null;
        var wizard = new Wizard() {
            ActiveStepIndex = 0
        };
        var step = new WizardStep() {
            OnInitialize = EventCallback.Factory.Create<WizardStepInitializedEventArgs>(this, args => arguments = args)
        };

        wizard.StepsInternal.Add(step);

        Assert.False(await wizard.GoToPreviousStep());

        Assert.Equal(0, wizard.ActiveStepIndex);
        Assert.Null(arguments);
    }

    [Fact]
    public async Task TryCompleteStep_Initializes_Next_Step() {
        WizardStepInitializedEventArgs? arguments = null;
        var wizard = new Wizard() {
            ActiveStepIndex = 0
        };
        var step = new WizardStep() {
            OnInitialize = EventCallback.Factory.Create<WizardStepInitializedEventArgs>(this, args => arguments = args)
        };

        wizard.StepsInternal.Add(new WizardStep());
        wizard.StepsInternal.Add(step);

        Assert.True(await wizard.TryCompleteStep());

        Assert.Equal(1, wizard.ActiveStepIndex);
        Assert.NotNull(arguments);
    }

    [Fact]
    public async Task TryCompleteStep_Can_Be_Cancelled() {
        var wizard = new Wizard() {
            ActiveStepIndex = 0
        };
        var step = new WizardStep() {
            OnTryComplete = EventCallback.Factory.Create<WizardStepAttemptedCompleteEventArgs>(this, args => args.IsCancelled = true)
        };

        wizard.StepsInternal.Add(step);

        Assert.False(await wizard.TryCompleteStep());

        Assert.Equal(0, wizard.ActiveStepIndex);
    }

    [Fact]
    public async Task TryCompleteStep_Resets_When_Finished() {
        var wizard = new Wizard() {
            ActiveStepIndex = 0
        };

        wizard.StepsInternal.Add(new WizardStep());

        Assert.True(await wizard.TryCompleteStep());

        Assert.Null(wizard.ActiveStepIndex);
        Assert.Empty(wizard.StepsInternal);
    }

    [Fact]
    public async Task TryCompleteStep_Does_Nothing_When_Inactive() {
        WizardStepInitializedEventArgs? wizardStepInitializedEventArgs = null;
        WizardStepAttemptedCompleteEventArgs? wizardStepAttemptedCompleteEventArgs = null;
        var wizard = new Wizard();

        wizard.StepsInternal.Add(new WizardStep() {
            OnInitialize = EventCallback.Factory.Create<WizardStepInitializedEventArgs>(this, args => wizardStepInitializedEventArgs = args),
            OnTryComplete = EventCallback.Factory.Create<WizardStepAttemptedCompleteEventArgs>(this, args => wizardStepAttemptedCompleteEventArgs = args)
        });

        Assert.False(await wizard.TryCompleteStep());

        Assert.Null(wizard.ActiveStepIndex);
        Assert.Null(wizardStepInitializedEventArgs);
        Assert.Null(wizardStepAttemptedCompleteEventArgs);
    }

    [Fact]
    public async Task GoToStep_Initializes_Next_Step() {
        WizardStepInitializedEventArgs? arguments = null;
        var wizard = new Wizard() {
            ActiveStepIndex = 0
        };
        var step = new WizardStep() {
            OnInitialize = EventCallback.Factory.Create<WizardStepInitializedEventArgs>(this, args => arguments = args)
        };

        wizard.StepsInternal.Add(new WizardStep());
        wizard.StepsInternal.Add(step);

        Assert.True(await wizard.GoToStep(1, false));

        Assert.Equal(1, wizard.ActiveStepIndex);
        Assert.NotNull(arguments);
    }

    [Fact]
    public async Task GoToStep_Resets_When_Finished() {
        var wizard = new Wizard() {
            ActiveStepIndex = 0
        };

        wizard.StepsInternal.Add(new WizardStep());

        Assert.True(await wizard.GoToStep(1, false));

        Assert.Null(wizard.ActiveStepIndex);
        Assert.Empty(wizard.StepsInternal);
    }

    [Fact]
    public async Task GoToStep_Does_Not_Trigger_OnTryComplete_Without_TryCompleteStep() {
        WizardStepAttemptedCompleteEventArgs? arguments = null;
        var wizard = new Wizard() {
            ActiveStepIndex = 0
        };
        var step = new WizardStep() {
            OnTryComplete = EventCallback.Factory.Create<WizardStepAttemptedCompleteEventArgs>(this, args => args.IsCancelled = true)
        };

        wizard.StepsInternal.Add(step);
        wizard.StepsInternal.Add(new WizardStep());

        Assert.True(await wizard.GoToStep(1, false));

        Assert.Equal(1, wizard.ActiveStepIndex);
        Assert.Null(arguments);
    }

    [Fact]
    public async Task GoToStep_Can_Be_Cancelled_With_TryCompleteStep() {
        WizardStepInitializedEventArgs? arguments = null;
        var wizard = new Wizard() {
            ActiveStepIndex = 0
        };

        wizard.StepsInternal.Add(new WizardStep() {
            OnInitialize = EventCallback.Factory.Create<WizardStepInitializedEventArgs>(this, args => arguments = args),
            OnTryComplete = EventCallback.Factory.Create<WizardStepAttemptedCompleteEventArgs>(this, args => args.IsCancelled = true)
        });
        wizard.StepsInternal.Add(new WizardStep() {
            OnInitialize = EventCallback.Factory.Create<WizardStepInitializedEventArgs>(this, args => arguments = args)
        });

        Assert.False(await wizard.GoToStep(1, true));

        Assert.Equal(0, wizard.ActiveStepIndex);
        Assert.Null(arguments);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(-1)]
    [InlineData(3)]
    public async Task GoToStep_Does_Nothing_When_StepIndex_Out_Of_Bounds(int? stepIndex) {
        WizardStepInitializedEventArgs? wizardStepInitializedEventArgs = null;
        WizardStepAttemptedCompleteEventArgs? wizardStepAttemptedCompleteEventArgs = null;
        var wizard = new Wizard() {
            ActiveStepIndex = 0
        };

        wizard.StepsInternal.Add(new WizardStep() {
            OnInitialize = EventCallback.Factory.Create<WizardStepInitializedEventArgs>(this, args => wizardStepInitializedEventArgs = args),
            OnTryComplete = EventCallback.Factory.Create<WizardStepAttemptedCompleteEventArgs>(this, args => wizardStepAttemptedCompleteEventArgs = args)
        });
        wizard.StepsInternal.Add(new WizardStep() {
            OnInitialize = EventCallback.Factory.Create<WizardStepInitializedEventArgs>(this, args => wizardStepInitializedEventArgs = args),
            OnTryComplete = EventCallback.Factory.Create<WizardStepAttemptedCompleteEventArgs>(this, args => wizardStepAttemptedCompleteEventArgs = args)
        });

        Assert.False(await wizard.GoToStep(stepIndex, true));

        Assert.Equal(0, wizard.ActiveStepIndex);
        Assert.Null(wizardStepInitializedEventArgs);
        Assert.Null(wizardStepAttemptedCompleteEventArgs);
    }

    [Fact]
    public async Task GoToStep_Does_Nothing_When_Inactive() {
        WizardStepInitializedEventArgs? wizardStepInitializedEventArgs = null;
        WizardStepAttemptedCompleteEventArgs? wizardStepAttemptedCompleteEventArgs = null;
        var wizard = new Wizard();

        wizard.StepsInternal.Add(new WizardStep() {
            OnInitialize = EventCallback.Factory.Create<WizardStepInitializedEventArgs>(this, args => wizardStepInitializedEventArgs = args),
            OnTryComplete = EventCallback.Factory.Create<WizardStepAttemptedCompleteEventArgs>(this, args => wizardStepAttemptedCompleteEventArgs = args)
        });

        Assert.False(await wizard.GoToStep(0, true));

        Assert.Null(wizard.ActiveStepIndex);
        Assert.Null(wizardStepInitializedEventArgs);
        Assert.Null(wizardStepAttemptedCompleteEventArgs);
    }
}
