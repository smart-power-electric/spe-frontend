using Microsoft.AspNetCore.Components;
using MudBlazor;
using web_app.ApiClient;
using web_app.Components.Dialogs;

namespace web_app.Components.Pages;

public partial class WorkerPage : ComponentBase
{

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;
    [Inject] IDialogService DialogService { get; set; } = null!;

    [Inject] IApiClient Client { get; set; } = null!;

    private bool _loading = false;
    private IEnumerable<WorkerResponse>? Workers;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _loading = true;
            Workers = await Client.FindAllWorkerAsync(100, 0);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Oops, an error occurred. The error type is: {ex.Message}.", Severity.Error);
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task AddWorker()
    {
    var parameters = new DialogParameters<WorkerDialog> { };

        var dialog = await DialogService.ShowAsync<WorkerDialog>("Crear trabajador", parameters);
        var result = await dialog.Result;
        if (!result.Canceled && (bool)result.Data)
        {
            Snackbar.Add($"Trabajador creado satisfactoriamente", Severity.Success);
        }
    }
}
