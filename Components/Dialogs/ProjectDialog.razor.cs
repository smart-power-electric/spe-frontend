using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace web_app.Components.Dialogs;

public partial class ProjectDialog : ComponentBase
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;

    async void Submit()
    {
       
    }

    void Cancel() => MudDialog.Cancel();
}
