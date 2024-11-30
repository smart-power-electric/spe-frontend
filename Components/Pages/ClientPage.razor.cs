using Microsoft.AspNetCore.Components;
using MudBlazor;
using web_app.ApiClient;
using web_app.Components.Dialogs;
using web_app.Components.Shared;

namespace web_app.Components.Pages;

public partial class ClientPage : ComponentBase
{

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;
    [Inject] IDialogService DialogService { get; set; } = null!;

    [Inject] IApiClient Client { get; set; } = null!;


    private MudDataGrid<ClientResponse> dataGrid { get; set; } = null!;
    private string? searchString = null;

    private async Task AddClient()
    {
        ClientResponse client = new();
        var parameters = new DialogParameters<ClientDialog> { { x => x.Model, client } };

        var dialog = await DialogService.ShowAsync<ClientDialog>("Crear client", parameters);
        var result = await dialog.Result;
        if (!result.Canceled && (bool)result.Data)
        {
            Snackbar.Add($"Cliente creado satisfactoriamente", Severity.Success);
        }
        _ = dataGrid.ReloadServerData();
    }

    private async Task EditClient(string clientId)
    {
        try
        {
            Visible = true;
            var client = await Client.FindOneClientAsync(clientId);
            if (client != null)
            {
                var parameters = new DialogParameters<ClientDialog> { { x => x.Model, client } };

                var dialog = await DialogService.ShowAsync<ClientDialog>("Editar cliente", parameters);
                var result = await dialog.Result;
                if (!result.Canceled && (bool)result.Data)
                {
                    Snackbar.Add($"Cliente actualizado satisfactoriamente", Severity.Success);
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
    private async Task RemoveClient(string clientId)
    {
        var options = new DialogOptions
        {
            BackdropClick = false,
            MaxWidth = MaxWidth.Small,
            Position = DialogPosition.Center,
        };
        var dialog = await DialogService.ShowAsync<DeleteDialog>("Eliminar Cliente", options);
        var result = await dialog.Result;
        if (!result.Canceled && (bool)result.Data)
        {
            try
            {
                await Client.RemoveClientAsync(clientId);
                Snackbar.Add($"Cliente eliminado satisfactoriamente", Severity.Success);
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

    private async Task<GridData<ClientResponse>> ServerReload(GridState<ClientResponse> state)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        try
        {
            var sortDefinition = state.SortDefinitions.FirstOrDefault();
            var sortField = SortField.Name;
            var sortOrder =
                SortOrder.ASC; 
            if (sortDefinition != null)
            {
                sortOrder = sortDefinition.Descending ? SortOrder.DESC : SortOrder.ASC;
            }
            
            var result = await Client.FindAllClientAsync(state.PageSize, state.PageSize * state.Page, searchString, "", sortOrder, sortField, _cts.Token);
            if(result == null)
            {
                return new GridData<ClientResponse>
                {
                    TotalItems = 0,
                    Items = Array.Empty<ClientResponse>()
                };
            }
            var totalItems = (int)result.Total;
            IEnumerable<ClientResponse> data = result.Data;
            
            return new GridData<ClientResponse>
            {
                TotalItems = totalItems,
                Items = data
            };
        }
        catch (OperationCanceledException)
        {
            return new GridData<ClientResponse>
            {
                TotalItems = 0,
                Items = Array.Empty<ClientResponse>()
            };
        }
    }

    private IEnumerable<ClientResponse> OrderBy(GridState<ClientResponse> state, IEnumerable<ClientResponse> data)
    {
        var sortDefinition = state.SortDefinitions.FirstOrDefault();
        if (sortDefinition!.SortBy == nameof(ClientResponse.Name))
        {
            data = data.OrderByDirection(
                   sortDefinition.Descending ? SortDirection.Descending : SortDirection.Ascending,
                   o => o.Name
               );
        }
        return data;
    }

    private Task OnSearch(string text)
    {
        searchString = text;
        return dataGrid.ReloadServerData();
    }

    #endregion
}
