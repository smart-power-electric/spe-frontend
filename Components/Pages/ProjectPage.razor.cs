using Microsoft.AspNetCore.Components;
using MudBlazor;
using web_app.ApiClient;
using web_app.Components.Dialogs;
using web_app.Components.Shared;

namespace web_app.Components.Pages;

public partial class ProjectPage
{
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] IDialogService DialogService { get; set; } = null!;

    [Inject] IApiClient Client { get; set; } = null!;

    public bool _loading = false;
    private MudDataGrid<ProjectResponse> dataGrid { get; set; } = null!;
    private string? searchString = null;

    private async Task AddProject()
    {
        ProjectResponse project = new();
        var parameters = new DialogParameters<ProjectDialog> { { x => x.Model, project } };

        var dialog = await DialogService.ShowAsync<ProjectDialog>("Crear projecto", parameters);
        var result = await dialog.Result;
        if (!result.Canceled && (bool)result.Data)
        {
            Snackbar.Add($"Projecto creado satisfactoriamente", Severity.Success);
        }

        _ = dataGrid.ReloadServerData();
    }

    private async Task EditProject(string projectId)
    {
        try
        {
            Visible = true;
            var project = await Client.FindOneProjectAsync(projectId);
            if (project != null)
            {
                var parameters = new DialogParameters<ProjectDialog> { { x => x.Model, project } };

                var dialog = await DialogService.ShowAsync<ProjectDialog>("Editar proyecto", parameters);
                var result = await dialog.Result;
                if (!result.Canceled && (bool)result.Data)
                {
                    Snackbar.Add($"Proyecto actualizado satisfactoriamente", Severity.Success);
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

    private async Task CreateQuotation(string projectId)
    {
        Snackbar.Add("Create Quotation is not yet available", Severity.Info);
    }

    private async Task RemoveProject(string projectId)
    {
        var options = new DialogOptions
        {
            BackdropClick = false,
            MaxWidth = MaxWidth.Small,
            Position = DialogPosition.Center,
        };
        var dialog = await DialogService.ShowAsync<DeleteDialog>("Eliminar proyecto", options);
        var result = await dialog.Result;
        if (!result.Canceled && (bool)result.Data)
        {
            try
            {
                await Client.RemoveProjectAsync(projectId);
                Snackbar.Add($"Proyecto eliminado satisfactoriamente", Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Oops! An error occurred. The error type is: {ex.Message}.", Severity.Error);
            }

            _ = dataGrid.ReloadServerData();
        }
    }

    public bool Visible = false;
    private CancellationTokenSource _cts;

    private async Task<GridData<ProjectResponse>> ServerReload(GridState<ProjectResponse> state)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        try
        {
            var sortDefinition = state.SortDefinitions.FirstOrDefault();
            var sortField = SortField5.Name;
            var sortOrder =
                SortOrder5.ASC;
            if (sortDefinition != null)
            {
                sortOrder = sortDefinition.Descending ? SortOrder5.DESC : SortOrder5.ASC;
            }

            var result = await Client.FindAllProjectAsync(state.PageSize, state.PageSize * state.Page, searchString,
                sortOrder,
                sortField, _cts.Token);
            var totalItems = (int)result.Total;
            IEnumerable<ProjectResponse> data = result.Data;

            return new GridData<ProjectResponse>
            {
                TotalItems = totalItems,
                Items = data
            };
        }
        catch (OperationCanceledException)
        {
            return new GridData<ProjectResponse>
            {
                TotalItems = 0,
                Items = Array.Empty<ProjectResponse>()
            };
        }
    }
    
    private Task OnSearch(string text)
    {
        searchString = text;
        return dataGrid.ReloadServerData();
    }
}