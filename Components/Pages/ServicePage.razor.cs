using Microsoft.AspNetCore.Components;
using MudBlazor;
using web_app.Components.Dialogs;

namespace web_app.Components.Pages;

public partial class ServicePage : ComponentBase
{

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;
    [Inject] IDialogService DialogService { get; set; } = null!;

    private async Task AddService()
    {

        var parameters = new DialogParameters<ServiceDialog> { };

        var dialog = await DialogService.ShowAsync<ServiceDialog>("Crear Servicio", parameters);
        var result = await dialog.Result;
        if (!result.Canceled && (bool)result.Data)
        {
            Snackbar.Add($"Servicio creado satisfactoriamente", Severity.Success);
        }
    }
}
