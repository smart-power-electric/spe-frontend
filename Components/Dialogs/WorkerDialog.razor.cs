using Microsoft.AspNetCore.Components;
using MudBlazor;
using web_app.ApiClient;
using web_app.Services.Validators;

namespace web_app.Components.Dialogs;

public partial class WorkerDialog : ComponentBase
{
    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public WorkerResponse Model { get; set; } = null!;
    [Inject] IApiClient Client { get; set; } = null!;

    private WorkerValidator workerValidator = new();
    private MudForm? form;

    async void Submit()
    {
        await form!.Validate();
        if (!form.IsValid) return;

        WorkerResponse result;
        try
        {
            if (MudDialog.Title!.Contains("Crear"))
            {
                CreateWorkerRequest request = new CreateWorkerRequest
                {
                    Name = Model.Name,
                    Phone = Model.Phone,
                    Address = Model.Address,
                    Speciality = Model.Speciality,
                    Contact = Model.Contact,
                    SocialSecurity = Model.SocialSecurity,
                    StartDate = Model.StartDate,
                    EndDate = Model.EndDate,
                };
                result = await Client.CreateWorkerAsync(request);
            }
            else
            {
                UpdateWorkerRequest request = new UpdateWorkerRequest
                {
                    Name = Model.Name,
                    Phone = Model.Phone,
                    Address = Model.Address,
                    Speciality = Model.Speciality,
                    Contact = Model.Contact,
                    SocialSecurity = Model.SocialSecurity,
                    StartDate = Model.StartDate,
                    EndDate = Model.EndDate,
                };
                result = await Client.UpdateWorkerAsync(Model.Id, request);
            }
            if (result != null)
                MudDialog!.Close(DialogResult.Ok(true));
            else
                Snackbar!.Add($"Oops, there was an error adding a new worker.", Severity.Error);
        }
        catch (Exception ex)
        {
            Snackbar!.Add($"Oops, an error occurred. The error type is: {ex.Message}.", Severity.Error);
        }
    }

    void Cancel() => MudDialog.Cancel();

    #region WorkerRateRegion
    
    public WorkerRatesResponse WorkerRate { get; set; } = null!;
    public ICollection<WorkerRatesResponse> WorkerRates { get; set; } = new List<WorkerRatesResponse>();
    
    protected override async Task OnInitializedAsync()
    {
        await GetWorkerRates();
    }
    private async Task GetWorkerRates()
    {
        try
        {
            var result = await Client.FindAllWorkerRatesAsync(999, 0, null);
            if(result != null)
                WorkerRates = result.Data;
            else
                Snackbar!.Add($"Oops, there was an error getting the worker rates.", Severity.Error);
        }
        catch (Exception ex)
        {
            Snackbar!.Add($"Oops, an error occurred. The error type is: {ex.Message}.", Severity.Error);
        }
    }
    #endregion
}
