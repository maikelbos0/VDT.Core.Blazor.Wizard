using Microsoft.AspNetCore.Components;

namespace VDT.Core.Blazor.Wizard;

/// <summary>
/// Context for render fragments for a wizard layout template
/// </summary>
public class WizardLayoutContext {
    /// <summary>
    /// Wizard that owns this context
    /// </summary>
    public Wizard Wizard { get; }

    internal WizardLayoutContext(Wizard wizard) {
        this.Wizard = wizard;
    }

    internal RenderFragment CascadingValue => builder => {
        if (Wizard.IsActive) {
            builder.OpenComponent<CascadingValue<Wizard>>(1);
            builder.AddAttribute(2, "Value", Wizard);
            builder.AddAttribute(3, "ChildContent", Content);
            builder.CloseComponent();
        }
    };

    internal RenderFragment Content => builder => {
        builder.AddContent(1, Wizard.Steps);

        if (Wizard.Layout == null) {
            builder.AddContent(2, DefaultLayout);
        }
        else {
            builder.AddContent(2, Wizard.Layout(this));
        }
    };

    /// <summary>
    /// Renders the wizard using the default layout
    /// </summary>
    public RenderFragment DefaultLayout => builder => {
        builder.OpenElement(1, "div");
        builder.AddAttribute(2, "class", Wizard.ContainerClass);

        {
            builder.OpenElement(3, "div");
            builder.AddAttribute(4, "class", Wizard.TitleContainerClass);
            builder.AddContent(5, Title);
            builder.CloseElement();

            builder.OpenElement(7, "div");
            builder.AddAttribute(8, "class", Wizard.StepTitleContainerClass);
            builder.AddContent(9, StepTitles);
            builder.CloseElement();

            builder.OpenElement(11, "div");
            builder.AddAttribute(12, "class", Wizard.ButtonContainerClass);
            builder.AddContent(13, Buttons);
            builder.CloseElement();

            builder.OpenElement(15, "div");
            builder.AddAttribute(16, "class", Wizard.ContentContainerClass);
            builder.AddContent(17, ActiveStepContent);
            builder.CloseElement();
        }

        builder.CloseElement();
    };

    /// <summary>
    /// Renders the wizard title content
    /// </summary>
    public RenderFragment Title => builder => {
        builder.AddContent(1, Wizard.TitleContent);
    };

    /// <summary>
    /// Renders the wizard step titles
    /// </summary>
    public RenderFragment StepTitles => builder => {
        foreach (var step in Wizard.StepsInternal) {
            builder.OpenElement(1, "div");
            builder.SetKey(step);

            if (step == Wizard.ActiveStep) {
                builder.AddAttribute(2, "class", $"{Wizard.StepTitleClass} {Wizard.ActiveStepTitleClass}");
            }
            else {
                builder.AddAttribute(2, "class", $"{Wizard.StepTitleClass}");
            }

            builder.AddContent(3, step.Title);
            builder.CloseElement();
        }
    };

    /// <summary>
    /// Renders the wizard cancel, previous, next and finish buttons if they are available and enabled
    /// </summary>
    public RenderFragment Buttons => builder => {
        builder.AddContent(1, ButtonCancel);
        builder.AddContent(2, ButtonPrevious);
        builder.AddContent(3, ButtonNext);
        builder.AddContent(4, ButtonFinish);
    };

    /// <summary>
    /// Renders the wizard cancel button if enabled
    /// </summary>
    public RenderFragment ButtonCancel => builder => {
        if (!Wizard.AllowCancel) return;

        builder.OpenElement(1, "button");
        builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(Wizard, Wizard.Stop));
        builder.AddAttribute(3, "class", $"{Wizard.ButtonClass} {Wizard.ButtonCancelClass}");
        builder.AddContent(4, Wizard.ButtonCancelText);
        builder.CloseElement();
    };

    /// <summary>
    /// Renders the wizard previous button if enabled and there is a previous step
    /// </summary>
    public RenderFragment ButtonPrevious => builder => {
        if (!Wizard.AllowPrevious || Wizard.IsFirstStepActive) return;

        builder.OpenElement(1, "button");
        builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(Wizard, Wizard.GoToPreviousStep));
        builder.AddAttribute(3, "class", $"{Wizard.ButtonClass} {Wizard.ButtonPreviousClass}");
        builder.AddContent(4, Wizard.ButtonPreviousText);
        builder.CloseElement();
    };

    /// <summary>
    /// Renders the wizard next button if there is a next step
    /// </summary>
    public RenderFragment ButtonNext => builder => {
        if (Wizard.IsLastStepActive) return;

        builder.OpenElement(1, "button");
        builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(Wizard, Wizard.TryCompleteStep));
        builder.AddAttribute(3, "class", $"{Wizard.ButtonClass} {Wizard.ButtonNextClass}");
        builder.AddContent(4, Wizard.ButtonNextText);
        builder.CloseElement();
    };

    /// <summary>
    /// Renders the wizard finish button if this is the final step
    /// </summary>
    public RenderFragment ButtonFinish => builder => {
        if (!Wizard.IsLastStepActive) return;

        builder.OpenElement(1, "button");
        builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(Wizard, Wizard.TryCompleteStep));
        builder.AddAttribute(3, "class", $"{Wizard.ButtonClass} {Wizard.ButtonFinishClass}");
        builder.AddContent(4, Wizard.ButtonFinishText);
        builder.CloseElement();
    };

    /// <summary>
    /// Renders the wizard active step content
    /// </summary>
    public RenderFragment ActiveStepContent => builder => {
        if (Wizard.ActiveStep != null) {
            builder.AddContent(1, Wizard.ActiveStep.ChildContent);
        }
    };
}
