using Microsoft.AspNetCore.Components;
using MudBlazor;
using web_app.Components.Dialogs;

namespace web_app.Components.Pages;

public partial class ProjectPage : ComponentBase
{

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;
    [Inject] IDialogService DialogService { get; set; } = null!;

    private async Task AddClient()
    {

        var parameters = new DialogParameters<ClientDialog> { };

        var dialog = await DialogService.ShowAsync<ClientDialog>("Crear Cliente", parameters);
        var result = await dialog.Result;
        if (!result.Canceled && (bool)result.Data)
        {
            Snackbar.Add($"Cliente creado satisfactoriamente", Severity.Success);
        }
    }
}
