using Microsoft.AspNetCore.Components;
using MudBlazor;
using web_app.ApiClient;
using web_app.Services.Validators;

namespace web_app.Components.Dialogs;

public partial class ServiceDialog : ComponentBase
{
    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public ServiceResponse Model { get; set; } = null!;
    [Inject] IApiClient Client { get; set; } = null!;

    private ServiceValidator serviceValidator = new();
    private MudForm? form;

    async void Submit()
    {
        await form!.Validate();
        if (!form.IsValid) return;

        ServiceResponse result;
        try
        {
            if (MudDialog.Title!.Contains("Crear"))
            {
                CreateServiceRequest request = new CreateServiceRequest
                {
                    Name = Model.Name,
                    UnitCost = double.Parse(Model.UnitCost),
                    Description = Model.Description,
                };
                result = await Client.CreateServiceAsync(request);
            }
            else
            {
                UpdateServiceRequest request = new UpdateServiceRequest
                {
                    Name = Model.Name,
                    UnitCost = double.Parse(Model.UnitCost),
                    Description = Model.Description,
                };
                result = await Client.UpdateServiceAsync(Model.Id, request);
            }
            if (result != null)
                MudDialog!.Close(DialogResult.Ok(true));
            else
                Snackbar!.Add($"Oops, there was an error adding a new service.", Severity.Error);
        }
        catch (Exception ex)
        {
            Snackbar!.Add($"Oops, an error occurred. The error type is: {ex.Message}.", Severity.Error);
        }
    }

    void Cancel() => MudDialog.Cancel();
}
