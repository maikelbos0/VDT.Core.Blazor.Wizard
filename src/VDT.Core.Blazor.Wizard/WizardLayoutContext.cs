using Microsoft.AspNetCore.Components;

namespace VDT.Core.Blazor.Wizard;

/// <summary>
/// Context for render fragments for a wizard layout template
/// </summary>
public class WizardLayoutContext {
    private readonly Wizard wizard;

    internal WizardLayoutContext(Wizard wizard) {
        this.wizard = wizard;
    }

    internal RenderFragment Wizard => builder => {
        if (wizard.IsActive) {
            builder.OpenComponent<CascadingValue<Wizard>>(1);
            builder.AddAttribute(2, "Value", wizard);
            builder.AddAttribute(3, "ChildContent", WizardContent);
            builder.CloseComponent();
        }
    };

    internal RenderFragment WizardContent => builder => {
        builder.AddContent(1, wizard.Steps);

        if (wizard.Layout == null) {
            builder.AddContent(2, DefaultLayout);
        }
        else {
            builder.AddContent(2, wizard.Layout(this));
        }
    };

    /// <summary>
    /// Renders the wizard using the default layout
    /// </summary>
    public RenderFragment DefaultLayout => builder => {
        builder.OpenElement(1, "div");
        builder.AddAttribute(2, "class", wizard.ContainerClass);

        {
            builder.OpenElement(3, "div");
            builder.AddAttribute(4, "class", wizard.TitleContainerClass);
            builder.AddContent(5, Title);
            builder.CloseElement();

            builder.OpenElement(7, "div");
            builder.AddAttribute(8, "class", wizard.StepTitleContainerClass);
            builder.AddContent(9, StepTitles);
            builder.CloseElement();

            builder.OpenElement(11, "div");
            builder.AddAttribute(12, "class", wizard.ButtonContainerClass);
            builder.AddContent(13, Buttons);
            builder.CloseElement();

            builder.OpenElement(15, "div");
            builder.AddAttribute(16, "class", wizard.ContentContainerClass);
            builder.AddContent(17, Content);
            builder.CloseElement();
        }

        builder.CloseElement();
    };

    /// <summary>
    /// Renders the wizard title content
    /// </summary>
    public RenderFragment Title => builder => {
        builder.AddContent(1, wizard.TitleContent);
    };

    /// <summary>
    /// Renders the wizard step titles
    /// </summary>
    public RenderFragment StepTitles => builder => {
        foreach (var step in wizard.StepsInternal) {
            builder.OpenElement(1, "div");
            builder.SetKey(step);

            if (step == wizard.ActiveStep) {
                builder.AddAttribute(2, "class", $"{wizard.StepTitleClass} {wizard.ActiveStepTitleClass}");
            }
            else {
                builder.AddAttribute(2, "class", $"{wizard.StepTitleClass}");
            }

            builder.AddContent(3, step.Title);
            builder.CloseElement();
        }
    };

    /// <summary>
    /// Renders the wizard cancel, previous, next and finish buttons
    /// </summary>
    public RenderFragment Buttons => builder => {
        builder.AddContent(1, ButtonCancel);
        builder.AddContent(2, ButtonPrevious);
        builder.AddContent(3, ButtonNext);
        builder.AddContent(4, ButtonFinish);
    };

    /// <summary>
    /// Renders the wizard cancel button
    /// </summary>
    public RenderFragment ButtonCancel => builder => {
        if (!wizard.AllowCancel) return;

        builder.OpenElement(1, "button");
        builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(wizard, wizard.Stop));
        builder.AddAttribute(3, "class", $"{wizard.ButtonClass} {wizard.ButtonCancelClass}");
        builder.AddContent(4, wizard.ButtonCancelText);
        builder.CloseElement();
    };

    /// <summary>
    /// Renders the wizard previous button
    /// </summary>
    public RenderFragment ButtonPrevious => builder => {
        if (!wizard.AllowPrevious || wizard.IsFirstStepActive) return;

        builder.OpenElement(1, "button");
        builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(wizard, wizard.GoToPreviousStep));
        builder.AddAttribute(3, "class", $"{wizard.ButtonClass} {wizard.ButtonPreviousClass}");
        builder.AddContent(4, wizard.ButtonPreviousText);
        builder.CloseElement();
    };

    /// <summary>
    /// Renders the wizard next button
    /// </summary>
    public RenderFragment ButtonNext => builder => {
        if (wizard.IsLastStepActive) return;

        builder.OpenElement(1, "button");
        builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(wizard, wizard.TryCompleteStep));
        builder.AddAttribute(3, "class", $"{wizard.ButtonClass} {wizard.ButtonNextClass}");
        builder.AddContent(4, wizard.ButtonNextText);
        builder.CloseElement();
    };

    /// <summary>
    /// Renders the wizard finish button
    /// </summary>
    public RenderFragment ButtonFinish => builder => {
        if (!wizard.IsLastStepActive) return;

        builder.OpenElement(1, "button");
        builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(wizard, wizard.TryCompleteStep));
        builder.AddAttribute(3, "class", $"{wizard.ButtonClass} {wizard.ButtonFinishClass}");
        builder.AddContent(4, wizard.ButtonFinishText);
        builder.CloseElement();
    };

    /// <summary>
    /// Renders the wizard active step content
    /// </summary>
    public RenderFragment Content => builder => {
        if (wizard.ActiveStep != null) {
            builder.AddContent(1, wizard.ActiveStep.ChildContent);
        }
    };
}
