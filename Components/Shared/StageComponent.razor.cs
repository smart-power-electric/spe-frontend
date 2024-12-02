using System.Collections.ObjectModel;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using web_app.ApiClient;

namespace web_app.Components.Shared;

public partial class StageComponent : ComponentBase
{
    [Inject] IApiClient Client { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Parameter] public string? ProjectId { get; set; }
    [Parameter] public EventCallback<List<StageResponse>> OnItemsChanged { get; set; }

    private bool Loading = false;
    private ObservableCollection<StageResponse> Stages { get; set; } = new();
    private CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            await _cts.CancelAsync();
            _cts = new CancellationTokenSource();

            if (ProjectId != null && Stages.Count == 0)
            {
                Loading = true;
                var result = await Client.FindAllStageAsync(50, 0, ProjectId, null,
                    SortOrder7.ASC,
                    SortField7.StartDate, _cts.Token);
                Stages = new ObservableCollection<StageResponse>(result.Data);
            }
        }
        catch (Exception e)
        {
            Snackbar.Add($"Oops! An error occurred while loading stages. Error description is: {e.Message}",
                Severity.Error);
        }
        finally
        {
            Loading = false;
        }
    }

    public async Task AddNewStage()
    {
        StageResponse newStage = new()
        {
            ProjectId = ProjectId
        };
        Stages.Add(newStage);
        await OnItemsChanged.InvokeAsync(Stages.ToList());
    }

    private async Task RemoveStage(StageResponse item)
    {
        Stages.Remove(item);
        await OnItemsChanged.InvokeAsync(Stages.ToList());
    }

    private int StageProgress(DateTime? startDate, DateTime? endDate)
    {
        var part = endDate.HasValue ? endDate.Value.Day - DateTime.Now.Day : 0;
        var total = endDate.HasValue && startDate.HasValue ? endDate.Value.Day - startDate.Value.Day : 1;
        return 100 - ((int)Math.Round((part / (double)total) * 100));
    }

    private Color StageProgressColor(int progress)
    {
        return progress switch
        {
            < 25 => Color.Success,
            < 50 => Color.Info,
            < 75 => Color.Warning,
            _ => Color.Error
        };
    }

    async Task CommittedItemChanges(StageResponse item)
    {
        await OnItemsChanged.InvokeAsync(Stages.ToList());
    }
}