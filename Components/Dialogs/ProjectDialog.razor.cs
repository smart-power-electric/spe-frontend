using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using web_app.ApiClient;
using web_app.Services.Validators;

namespace web_app.Components.Dialogs;

public partial class ProjectDialog : ComponentBase
{
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [CascadingParameter] private MudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public ProjectResponse Model { get; set; } = null!;
    [Inject] IApiClient Client { get; set; } = null!;
    private IEnumerable<StageResponse> Stages { get; set; } = [];

    private ProjectValidator projectValidator = new();
    private MudForm? form;
    private CancellationTokenSource _cts = new();

    async Task Submit()
    {
        await form!.Validate();
        if (!form.IsValid) return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        ProjectResponse result;
        try
        {
            if (MudDialog.Title!.Contains("Crear"))
            {
                CreateProjectRequest request = new CreateProjectRequest
                {
                    Name = Model.Name,
                    Description = Model.Description,
                    StartDate = Model.StartDate?.ToString("MM/dd/yyyy"),
                    EndDate = Model.EndDate?.ToString("MM/dd/yyyy"),
                    ClientId = Model.ClientId,
                    Location = Model.Location,
                };
                result = await Client.CreateProjectAsync(request, _cts.Token);

                if (result != null)
                {
                    var stages = Stages.Select(s => new CreateStageRequest
                    {
                        Name = s.Name,
                        Description = s.Description,
                        StartDate = s.StartDate,
                        EndDate = s.EndDate,
                        ProjectId = result.Id,
                    }).ToList();
                    await Task.WhenAll(
                        stages.Select(s => Client.CreateStageAsync(s, _cts.Token))
                    );
                }
            }
            else
            {
                UpdateProjectRequest request = new UpdateProjectRequest
                {
                    Name = Model.Name,
                    Description = Model.Description,
                    StartDate = Model.StartDate?.ToString("MM/dd/yyyy"),
                    EndDate = Model.EndDate?.ToString("MM/dd/yyyy"),
                    ClientId = Model.ClientId,
                    Location = Model.Location,
                };

                var resultTask = Client.UpdateProjectAsync(Model.Id, request);
                await Task.WhenAll(
                    resultTask,
                    UpdateStages()
                );
                result = resultTask.Result;
            }

            if (result != null)
                MudDialog.Close(DialogResult.Ok(true));
            else
                Snackbar.Add($"Oops, there was an error adding a new project.", Severity.Error);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Oops, an error occurred. The error type is: {ex.Message}.", Severity.Error);
        }
    }

    void Cancel() => MudDialog.Cancel();

    protected override async Task OnInitializedAsync()
    {
        await _cts.CancelAsync();
        _cts = new CancellationTokenSource();
        
        if (Model.ClientId != null)
        {
            var client = await Client.FindOneClientAsync(Model.ClientId, _cts.Token);
            ClientList = new List<ClientResponse> { client };
        }

    }

    private async Task<IEnumerable<ClientResponse>> Search(string value, CancellationToken token)
    {
        var data = await Client.FindAllClientAsync(10, 0, value, null, SortOrder.ASC, SortField.Name, token);
        ClientList = data.Data;
        return data.Data;
    }

    private ICollection<ClientResponse> ClientList { get; set; } = new List<ClientResponse>();

    private ClientResponse? SelectedClient
    {
        get => ClientList.FirstOrDefault(c => c.Id == Model.ClientId);
        set => Model.ClientId = value?.Id;
    }

    string ToStringFunc(ClientResponse? client) =>
        client != null ? client.Name : SelectedClient != null ? SelectedClient.Name : string.Empty;

    private void OnChangedStages(List<StageResponse> items)
    {
        Stages = items;
    }

    private Task UpdateStages()
    {
        var stages = Stages.Select(s => new UpsertStageRequest()
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            ProjectId = Model.Id,
            Percentage = s.Percentage,
            AdjustedPercentage = 0,
        }).ToList();

        return Client.UpdateBulkStageAsync(Model.Id, stages, _cts.Token);
    }
}