using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using Xunit;

namespace VDT.Core.Blazor.Wizard.Tests;

public class WizardStepTests {
    [Fact]
    public async Task WizardStep_Initialize_Invokes_OnInitialize() {
        WizardStepInitializedEventArgs? arguments = null;
        var step = new WizardStep() {
            OnInitialize = EventCallback.Factory.Create<WizardStepInitializedEventArgs>(this, args => arguments = args)
        };

        await step.Initialize();

        Assert.NotNull(arguments);
    }

    [Fact]
    public async Task WizardStep_TryComplete_Invokes_OnTryComplete() {
        WizardStepAttemptedCompleteEventArgs? arguments = null;
        var step = new WizardStep() {
            OnTryComplete = EventCallback.Factory.Create<WizardStepAttemptedCompleteEventArgs>(this, args => arguments = args)
        };

        await step.TryComplete();

        Assert.NotNull(arguments);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task WizardStep_TryComplete_Returns_Correct_ShouldComplete(bool isCancelled, bool expectedResult) {
        var step = new WizardStep() {
            OnTryComplete = EventCallback.Factory.Create<WizardStepAttemptedCompleteEventArgs>(this, args => args.IsCancelled = isCancelled)
        };

        Assert.Equal(expectedResult, await step.TryComplete());
    }

    [Fact]
    public void WizardStep_IsActive_Is_False_For_No_Parent() {
        var step = new WizardStep() {
            Wizard = null
        };

        Assert.False(step.IsActive);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public void WizardStep_IsActive_Is_Correct(int activeStepIndex, bool expectedIsActive) {
        var wizard = new Wizard() {
            ActiveStepIndex = activeStepIndex
        };
        var step = new WizardStep() {
            Wizard = wizard
        };

        wizard.StepsInternal.Add(new WizardStep());
        wizard.StepsInternal.Add(step);

        Assert.Equal(expectedIsActive, step.IsActive);
    }
}
