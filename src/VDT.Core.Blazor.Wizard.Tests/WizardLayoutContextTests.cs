using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Xunit;

namespace VDT.Core.Blazor.Wizard.Tests;

public class WizardLayoutContextTests {
    // These tests are likely fragile; if they break when updating Microsoft.AspNetCore.Components it may be a a good idea to wrap the RenderTreeBuilder.

#pragma warning disable BL0006 // Do not use RenderTree types
    public class TestContext {
        public static TestContext CreateTestContext(Wizard wizard, Func<WizardLayoutContext, RenderFragment> getRenderMethod) {
            var context = new WizardLayoutContext(wizard);
            var builder = new RenderTreeBuilder();
            var renderFragment = getRenderMethod(context);

            renderFragment(builder);

            return new TestContext(context, builder.GetFrames());
        }

        private readonly WizardLayoutContext context;
        private readonly ArrayRange<RenderTreeFrame> frames;

        public TestContext(WizardLayoutContext context, ArrayRange<RenderTreeFrame> frames) {
            this.context = context;
            this.frames = frames;
        }

        public void AssertFrameCount(int expectedFrameCount) {
            Assert.Equal(expectedFrameCount, frames.Count);
        }

        public void AssertComponent<TComponent>(int index, int innerFrameCount) {
            Assert.Equal(RenderTreeFrameType.Component, frames.Array[index].FrameType);
            Assert.Equal(typeof(TComponent), frames.Array[index].ComponentType);
            Assert.Equal(innerFrameCount, frames.Array[index].ComponentSubtreeLength);
        }

        public void AssertAttribute(int index, string attributeName, Func<WizardLayoutContext, object> getAttributeValue) {
            AssertAttribute(index, attributeName, getAttributeValue(context));
        }

        public void AssertAttribute(int index, string attributeName, Func<Task> attributeValue) {
            AssertAttribute(index, attributeName, (object)attributeValue);
        }

        public void AssertAttribute(int index, string attributeName, object attributeValue) {
            Assert.Equal(RenderTreeFrameType.Attribute, frames.Array[index].FrameType);
            Assert.Equal(attributeName, frames.Array[index].AttributeName);
            Assert.Equal(attributeValue, frames.Array[index].AttributeValue);
        }

        public void AssertElement(int index, string elementName, int innerFrameCount) {
            Assert.Equal(RenderTreeFrameType.Element, frames.Array[index].FrameType);
            Assert.Equal(elementName, frames.Array[index].ElementName);
            Assert.Equal(innerFrameCount, frames.Array[index].ElementSubtreeLength);
        }

        public void AssertContent(int index, string content) {
            Assert.Equal(RenderTreeFrameType.Text, frames.Array[index].FrameType);
            Assert.Equal(content, frames.Array[index].TextContent);
        }
    }
#pragma warning restore BL0006 // Do not use RenderTree types

    [Fact]
    public void CascadingValue_Renders_When_Active() {
        var wizard = new Wizard() {
            ActiveStepIndex = 0
        };
        var context = TestContext.CreateTestContext(wizard, c => c.CascadingValue);

        context.AssertFrameCount(3);
        context.AssertComponent<CascadingValue<Wizard>>(0, 3);
        context.AssertAttribute(1, "Value", wizard);
        context.AssertAttribute(2, "ChildContent", c => c.Content);
    }

    [Fact]
    public void CascadingValue_Does_Not_Render_When_Inactive() {
        var wizard = new Wizard();
        var context = TestContext.CreateTestContext(wizard, c => c.CascadingValue);

        context.AssertFrameCount(0);
    }

    [Fact]
    public void Content_Renders_DefaultLayout_Correctly() {
        var wizard = new Wizard() {
            Steps = builder => builder.AddContent(1, "Step")
        };
        var context = TestContext.CreateTestContext(wizard, c => c.Content);

        context.AssertFrameCount(20);
        context.AssertContent(1, "Step");
    }

    [Fact]
    public void Content_Renders_Layout_Correctly() {
        var wizard = new Wizard() {
            Steps = builder => builder.AddContent(1, "Step"),
            Layout = context => builder => builder.AddContent(1, "Test")
        };
        var context = TestContext.CreateTestContext(wizard, c => c.Content);

        context.AssertFrameCount(4);
        context.AssertContent(1, "Step");
        context.AssertContent(3, "Test");
    }

    [Fact]
    public void DefaultLayout_Renders_Correctly() {
        var wizard = new Wizard() {
            ActiveStepIndex = 0,
            ContainerClass = "container",
            TitleContainerClass = "title-container",
            TitleContent = builder => builder.AddContent(1, "<h1>Title</h1>"),
            StepTitleContainerClass = "step-title-container",
            ButtonContainerClass = "button-container",
            ContentContainerClass = "content-container"
        };
        wizard.StepsInternal.Add(new WizardStep() {
            Title = "Step",
            ChildContent = builder => builder.AddContent(1, "Step content")
        });
        var context = TestContext.CreateTestContext(wizard, c => c.DefaultLayout);

        context.AssertFrameCount(29);
        context.AssertElement(0, "div", 29);
        context.AssertAttribute(1, "class", "container");

        context.AssertElement(2, "div", 5);
        context.AssertAttribute(3, "class", "title-container");
        context.AssertContent(6, "<h1>Title</h1>");

        context.AssertElement(7, "div", 6);
        context.AssertAttribute(8, "class", "step-title-container");
        context.AssertContent(12, "Step");

        context.AssertElement(13, "div", 11);
        context.AssertAttribute(14, "class", "button-container");
        context.AssertContent(23, "Finish");

        context.AssertElement(24, "div", 5);
        context.AssertAttribute(25, "class", "content-container");
        context.AssertContent(28, "Step content");
    }

    [Fact]
    public void Title_Renders_Correctly() {
        var wizard = new Wizard() {
            TitleContent = builder => builder.AddContent(1, "<h1>Title</h1>")
        };
        var context = TestContext.CreateTestContext(wizard, c => c.Title);

        context.AssertFrameCount(2);
        context.AssertContent(1, "<h1>Title</h1>");
    }

    [Fact]
    public void StepTitles_Renders_Correctly() {
        var wizard = new Wizard() {
            ActiveStepIndex = 1,
            StepTitleClass = "step-title",
            ActiveStepTitleClass = "active"
        };
        wizard.StepsInternal.Add(new WizardStep() {
            Title = "Step 1"
        });
        wizard.StepsInternal.Add(new WizardStep() {
            Title = "Step 2"
        });
        var context = TestContext.CreateTestContext(wizard, c => c.StepTitles);

        context.AssertFrameCount(6);

        context.AssertElement(0, "div", 3);
        context.AssertAttribute(1, "class", "step-title");
        context.AssertContent(2, "Step 1");

        context.AssertElement(3, "div", 3);
        context.AssertAttribute(4, "class", "step-title active");
        context.AssertContent(5, "Step 2");
    }

    [Fact]
    public void Buttons_Renders_Correctly() {
        var wizard = new Wizard() {
            ActiveStepIndex = 1,
            AllowCancel = true,
            AllowPrevious = true,
        };
        wizard.StepsInternal.Add(new WizardStep() {
            Title = "Step 1"
        });
        wizard.StepsInternal.Add(new WizardStep() {
            Title = "Step 2"
        });
        wizard.StepsInternal.Add(new WizardStep() {
            Title = "Step 3"
        });
        var context = TestContext.CreateTestContext(wizard, c => c.Buttons);

        context.AssertFrameCount(16);

        context.AssertElement(1, "button", 4);
        context.AssertContent(4, "Cancel");

        context.AssertElement(6, "button", 4);
        context.AssertContent(9, "Previous");

        context.AssertElement(11, "button", 4);
        context.AssertContent(14, "Next");
    }

    [Fact]
    public void ButtonCancel_Renders_When_Allowed() {
        var wizard = new Wizard() {
            AllowCancel = true,
            ButtonCancelText = "Abort",
            ButtonClass = "btn",
            ButtonCancelClass = "btn-secondary"
        };
        var context = TestContext.CreateTestContext(wizard, c => c.ButtonCancel);

        context.AssertFrameCount(4);

        context.AssertElement(0, "button", 4);
        context.AssertAttribute(1, "onclick", wizard.Stop);
        context.AssertAttribute(2, "class", "btn btn-secondary");
        context.AssertContent(3, "Abort");
    }

    [Fact]
    public void ButtonCancel_Does_Not_Render_When_Not_Allowed() {
        var wizard = new Wizard();
        var context = TestContext.CreateTestContext(wizard, c => c.ButtonCancel);

        context.AssertFrameCount(0);
    }

    [Fact]
    public void ButtonPrevious_Renders_When_Allowed() {
        var wizard = new Wizard() {
            AllowPrevious = true,
            ButtonPreviousText = "Prev",
            ButtonClass = "btn",
            ButtonPreviousClass = "btn-secondary"
        };
        var context = TestContext.CreateTestContext(wizard, c => c.ButtonPrevious);

        context.AssertFrameCount(4);

        context.AssertElement(0, "button", 4);
        context.AssertAttribute(1, "onclick", wizard.GoToPreviousStep);
        context.AssertAttribute(2, "class", "btn btn-secondary");
        context.AssertContent(3, "Prev");
    }

    [Fact]
    public void ButtonPrevious_Does_Not_Render_When_Not_Allowed() {
        var wizard = new Wizard();
        var context = TestContext.CreateTestContext(wizard, c => c.ButtonPrevious);

        context.AssertFrameCount(0);
    }

    [Fact]
    public void ButtonPrevious_Does_Not_Render_When_On_First_Step() {
        var wizard = new Wizard() {
            ActiveStepIndex = 0,
            AllowPrevious = true
        };
        wizard.StepsInternal.Add(new WizardStep() {
            Title = "Step 1"
        });
        var context = TestContext.CreateTestContext(wizard, c => c.ButtonPrevious);

        context.AssertFrameCount(0);
    }

    [Fact]
    public void ButtonNext_Renders_When_Not_On_Last_Step() {
        var wizard = new Wizard() {
            ActiveStepIndex = 0,
            ButtonNextText = "Continue",
            ButtonClass = "btn",
            ButtonNextClass = "btn-primary"
        };
        wizard.StepsInternal.Add(new WizardStep() {
            Title = "Step 1"
        });
        wizard.StepsInternal.Add(new WizardStep() {
            Title = "Step 2"
        });
        var context = TestContext.CreateTestContext(wizard, c => c.ButtonNext);

        context.AssertFrameCount(4);

        context.AssertElement(0, "button", 4);
        context.AssertAttribute(1, "onclick", wizard.TryCompleteStep);
        context.AssertAttribute(2, "class", "btn btn-primary");
        context.AssertContent(3, "Continue");
    }

    [Fact]
    public void ButtonNext_Does_Not_Render_When_On_Last_Step() {
        var wizard = new Wizard() {
            ActiveStepIndex = 0
        };
        wizard.StepsInternal.Add(new WizardStep() {
            Title = "Step 1"
        });
        var context = TestContext.CreateTestContext(wizard, c => c.ButtonNext);

        context.AssertFrameCount(0);
    }

    [Fact]
    public void ButtonFinish_Renders_When_On_Last_Step() {
        var wizard = new Wizard() {
            ActiveStepIndex = 1,
            ButtonFinishText = "Complete",
            ButtonClass = "btn",
            ButtonFinishClass = "btn-primary"
        };
        wizard.StepsInternal.Add(new WizardStep() {
            Title = "Step 1"
        });
        wizard.StepsInternal.Add(new WizardStep() {
            Title = "Step 2"
        });
        var context = TestContext.CreateTestContext(wizard, c => c.ButtonFinish);

        context.AssertFrameCount(4);

        context.AssertElement(0, "button", 4);
        context.AssertAttribute(1, "onclick", wizard.TryCompleteStep);
        context.AssertAttribute(2, "class", "btn btn-primary");
        context.AssertContent(3, "Complete");
    }

    [Fact]
    public void ButtonFinish_Does_Not_Render_When_Not_On_Last_Step() {
        var wizard = new Wizard() {
            ActiveStepIndex = 0
        };
        wizard.StepsInternal.Add(new WizardStep() {
            Title = "Step 1"
        });
        wizard.StepsInternal.Add(new WizardStep() {
            Title = "Step 2"
        });
        var context = TestContext.CreateTestContext(wizard, c => c.ButtonFinish);

        context.AssertFrameCount(0);
    }

    [Fact]
    public void ActiveStepContent_Renders_When_Active() {
        var renderFragment = Substitute.For<RenderFragment>();
        var wizard = new Wizard() {
            ActiveStepIndex = 0
        };
        wizard.StepsInternal.Add(new WizardStep() {
            ChildContent = renderFragment
        });
        var context = TestContext.CreateTestContext(wizard, c => c.ActiveStepContent);

        context.AssertFrameCount(1);
        renderFragment.Received().Invoke(Arg.Any<RenderTreeBuilder>());
    }

    [Fact]
    public void ActiveStepContent_Does_Not_Render_When_Inactive() {
        var wizard = new Wizard();
        var activeStep = new WizardStep() {
            ChildContent = builder => { }
        };
        wizard.StepsInternal.Add(activeStep);
        var context = TestContext.CreateTestContext(wizard, c => c.ActiveStepContent);

        context.AssertFrameCount(0);
    }
}
