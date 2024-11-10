using Microsoft.AspNetCore.Components;
using MudBlazor;
using web_app.Components.Dialogs;

namespace web_app.Components.Pages;

public partial class MaterialPage : ComponentBase
{

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;
    [Inject] IDialogService DialogService { get; set; } = null!;

    private async Task AddMaterial()
    {

        var parameters = new DialogParameters<ServiceDialog> { };

        var dialog = await DialogService.ShowAsync<ServiceDialog>("Crear material", parameters);
        var result = await dialog.Result;
        if (!result.Canceled && (bool)result.Data)
        {
            Snackbar.Add($"Material creado satisfactoriamente", Severity.Success);
        }
    }
}
