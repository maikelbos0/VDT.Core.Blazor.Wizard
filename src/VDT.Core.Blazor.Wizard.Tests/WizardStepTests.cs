using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using Xunit;

namespace VDT.Core.Blazor.Wizard.Tests;

public class WizardStepTests {
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public void IsActive(int activeStepIndex, bool expectedIsActive) {
        var wizard = new Wizard() {
            ActiveStepIndex = activeStepIndex
        };
        var step = new WizardStep() {
            Wizard = wizard
        };

        wizard.StepsInternal.Add(new());
        wizard.StepsInternal.Add(step);

        Assert.Equal(expectedIsActive, step.IsActive);
    }

    [Fact]
    public void IsActive_Is_False_For_No_Parent() {
        var step = new WizardStep() {
            Wizard = null
        };

        Assert.False(step.IsActive);
    }

    [Fact]
    public async Task Initialize_Invokes_OnInitialize() {
        WizardStepInitializedEventArgs? arguments = null;
        var step = new WizardStep() {
            OnInitialize = EventCallback.Factory.Create<WizardStepInitializedEventArgs>(this, args => arguments = args)
        };

        await step.Initialize();

        Assert.NotNull(arguments);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task TryComplete_Invokes_OnTryComplete(bool isCompleted, bool expectedWasPreviouslyCompleted) {
        WizardStepAttemptedCompleteEventArgs? arguments = null;
        var step = new WizardStep() {
            OnTryComplete = EventCallback.Factory.Create<WizardStepAttemptedCompleteEventArgs>(this, args => arguments = args),
            IsCompleted = isCompleted
        };

        await step.TryComplete();

        Assert.NotNull(arguments);
        Assert.Equal(expectedWasPreviouslyCompleted, arguments.WasPreviouslyCompleted);
        Assert.True(step.IsCompleted);
    }

    [Theory]
    [InlineData(false, true, false, false)]
    [InlineData(false, false, true, true)]
    [InlineData(true, true, false, true)]
    [InlineData(true, false, true, true)]
    public async Task TryComplete_Returns_Correct_ShouldComplete(bool isCompleted, bool isCancelled, bool expectedShouldComplete, bool expectedIsCompleted) {
        var step = new WizardStep() {
            OnTryComplete = EventCallback.Factory.Create<WizardStepAttemptedCompleteEventArgs>(this, args => args.IsCancelled = isCancelled),
            IsCompleted = isCompleted
        };

        Assert.Equal(expectedShouldComplete, await step.TryComplete());
        Assert.Equal(expectedIsCompleted, step.IsCompleted);
    }
}
