using Microsoft.AspNetCore.Components;

namespace web_app.Components.Layout;

public partial class ContentPage : ComponentBase
{
    [Parameter]
    public required string Icon { get; set; }
    [Parameter]
    public required string Title { get; set; }
    [Parameter]
    public bool HiddenAddButton { get; set; } = true;
    [Parameter]
    public required RenderFragment ChildContent { get; set; }
    [Parameter]
    public Func<Task>? AddButtonAction { get; set; }

}