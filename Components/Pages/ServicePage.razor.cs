using Microsoft.AspNetCore.Components;
using MudBlazor;
using web_app.ApiClient;
using web_app.Components.Dialogs;
using web_app.Components.Shared;

namespace web_app.Components.Pages;

public partial class ServicePage : ComponentBase
{

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;
    [Inject] IDialogService DialogService { get; set; } = null!;

    [Inject] IApiClient Client { get; set; } = null!;

    public bool _loading = false;

    private MudDataGrid<ServiceResponse> dataGrid { get; set; } = null!;
    private string? searchString = null;

    private async Task AddService()
    {
        ServiceResponse service= new();
        var parameters = new DialogParameters<ServiceDialog> { { x => x.Model, service } };

        var dialog = await DialogService.ShowAsync<ServiceDialog>("Crear servicio", parameters);
        var result = await dialog.Result;
        if (!result.Canceled && (bool)result.Data)
        {
            Snackbar.Add($"Servicio creado satisfactoriamente", Severity.Success);
        }
        _ = dataGrid.ReloadServerData();
    }
    private async Task EditService(string serviceId)
    {
        try
        {
            Visible = true;
            var service = await Client.FindOneServiceAsync(serviceId);
            if (service != null)
            {
                var parameters = new DialogParameters<ServiceDialog> { { x => x.Model, service } };

                var dialog = await DialogService.ShowAsync<ServiceDialog>("Editar servicio", parameters);
                var result = await dialog.Result;
                if (!result.Canceled && (bool)result.Data)
                {
                    Snackbar.Add($"Servicio actualizado satisfactoriamente", Severity.Success);
                    _ = dataGrid.ReloadServerData();
                }
            }
            else
            {
                Snackbar.Add($"Oops! An error has occurred. Error loading data from server.", Severity.Error);
            }
        }
        catch
        {
            Snackbar.Add($"Oops! An error has occurred. This worker is not in the database.", Severity.Error);
        }
        finally
        {
            Visible = false;
        }
    }
    private async Task RemoveService(string workerId)
    {
        var options = new DialogOptions
        {
            BackdropClick = false,
            MaxWidth = MaxWidth.Small,
            Position = DialogPosition.Center,
        };
        var dialog = await DialogService.ShowAsync<DeleteDialog>("Eliminar servicio", options);
        var result = await dialog.Result;
        if (!result.Canceled && (bool)result.Data)
        {
            try
            {
                await Client.RemoveServiceAsync(workerId);
                Snackbar.Add($"Servicio eliminado satisfactoriamente", Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Oops! An error occurred. The error type is: {ex.Message}.", Severity.Error);
            }
            _ = dataGrid.ReloadServerData();
        }
    }

    #region Utils (load datagrid, dateformat, overlay etc...)

    public bool Visible = false;
    private CancellationTokenSource _cts;

    private async Task<GridData<ServiceResponse>> ServerReload(GridState<ServiceResponse> state)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        try
        {
            var result = await Client.FindAllServiceAsync(state.PageSize, state.PageSize * state.Page, searchString, _cts.Token);
            var totalItems = (int)result.Total;
            IEnumerable<ServiceResponse> data = result.Data;

            var sortDefinition = state.SortDefinitions.FirstOrDefault();
            if (sortDefinition != null)
            {
                data = OrderBy(state, data);
            }
            return new GridData<ServiceResponse>
            {
                TotalItems = totalItems,
                Items = data
            };
        }
        catch (OperationCanceledException)
        {
            return new GridData<ServiceResponse>
            {
                TotalItems = 0,
                Items = Array.Empty<ServiceResponse>()
            };
        }
    }

    private IEnumerable<ServiceResponse> OrderBy(GridState<ServiceResponse> state, IEnumerable<ServiceResponse> data)
    {
        var sortDefinition = state.SortDefinitions.FirstOrDefault();

        if (sortDefinition!.SortBy == nameof(ServiceResponse.Name))
            data = data.OrderByDirection(
                    sortDefinition.Descending ? SortDirection.Descending : SortDirection.Ascending,
                    o => o.Name
                );

        return data;
    }

    private Task OnSearch(string text)
    {
        searchString = text;
        return dataGrid.ReloadServerData();
    }
    #endregion
}
