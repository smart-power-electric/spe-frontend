using Microsoft.AspNetCore.Components;
using MudBlazor;
using web_app.ApiClient;
using web_app.Components.Dialogs;
using web_app.Components.Shared;

namespace web_app.Components.Pages;

public partial class WorkerPage
{
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] IDialogService DialogService { get; set; } = null!;

    [Inject] IApiClient Client { get; set; } = null!;

    private bool _loading = false;
    private IEnumerable<WorkerResponse>? Workers;

    private MudDataGrid<WorkerResponse> dataGrid { get; set; } = null!;
    private string? searchString = null;

    private async Task AddWorker()
    {
        WorkerResponse worker = new();
        var parameters = new DialogParameters<WorkerDialog> { { x => x.Model, worker } };

        var dialog = await DialogService.ShowAsync<WorkerDialog>("Crear trabajador", parameters);
        var result = await dialog.Result;
        if (result is {Canceled: false, Data: not null })
        {
            Snackbar.Add($"Trabajador creado satisfactoriamente", Severity.Success);
        }

        _ = dataGrid.ReloadServerData();
    }

    private async Task EditWorker(string workerId)
    {
        try
        {
            Visible = true;
            var worker = await Client.FindOneWorkerAsync(workerId);
            if (worker != null)
            {
                var parameters = new DialogParameters<WorkerDialog> { { x => x.Model, worker } };

                var dialog = await DialogService.ShowAsync<WorkerDialog>("Editar trabajador", parameters);
                var result = await dialog.Result;
                if (result is {Canceled: false, Data: not null })
                {
                    Snackbar.Add($"Trabajador actualizado satisfactoriamente", Severity.Success);
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

    private async Task RemoveWorker(string workerId)
    {
        var options = new DialogOptions
        {
            BackdropClick = false,
            MaxWidth = MaxWidth.Small,
            Position = DialogPosition.Center,
        };
        var dialog = await DialogService.ShowAsync<DeleteDialog>("Eliminar trabajador", options);
        var result = await dialog.Result;
        if (result is {Canceled: false, Data: not null })
        {
            try
            {
                await Client.RemoveWorkerAsync(workerId);
                Snackbar.Add($"Trabajador eliminado satisfactoriamente", Severity.Success);
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
    private CancellationTokenSource? _cts;

    private async Task<GridData<WorkerResponse>> ServerReload(GridState<WorkerResponse> state)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        try
        {
            var sortDefinition = state.SortDefinitions.FirstOrDefault();
            var sortField = SortField8.Name; // sortDefinition?.SortBy;
            var sortOrder =
                SortOrder8.ASC; // sortDefinition.Descending ? SortDirection.Descending : SortDirection.Ascending;
            if (sortDefinition != null)
            {
                sortOrder = sortDefinition.Descending ? SortOrder8.DESC : SortOrder8.ASC;
                sortField = sortDefinition.SortBy switch
                {
                    nameof(WorkerResponse.Name) => SortField8.Name,
                    nameof(WorkerResponse.StartDate) => SortField8.StartDate,
                    nameof(WorkerResponse.EndDate) => SortField8.EndDate,
                    nameof(WorkerResponse.Speciality) => SortField8.Speciality,
                    _ => SortField8.Name
                };
            }

            var result = await Client.FindAllWorkerAsync(state.PageSize, state.PageSize * state.Page, searchString,
                sortOrder, sortField, _cts.Token);
            var totalItems = (int)result.Total;
            IEnumerable<WorkerResponse> data = result.Data;

            return new GridData<WorkerResponse>
            {
                TotalItems = totalItems,
                Items = data
            };
        }
        catch (OperationCanceledException)
        {
            return new GridData<WorkerResponse>
            {
                TotalItems = 0,
                Items = Array.Empty<WorkerResponse>()
            };
        }
    }
    
    private Task OnSearch(string text)
    {
        searchString = text;
        return dataGrid.ReloadServerData();
    }

    private static string ChangeDateFormat(DateTime? value) =>
        DateTime.TryParse(value.ToString(), out var date) ? date.ToString("MM/dd/yyyy") : "";

    #endregion
}