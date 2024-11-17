using Microsoft.AspNetCore.Components;
using MudBlazor;
using web_app.ApiClient;
using web_app.Services.Validators;

namespace web_app.Components.Dialogs;

public partial class ClientDialog : ComponentBase
{
    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public ClientResponse Model { get; set; } = null!;
    [Inject] IApiClient Client { get; set; } = null!;

    private ClientValidator clientValidator = new();
    private MudForm? form;

    async void Submit()
    {
        await form!.Validate();
        if (!form.IsValid) return;

        ClientResponse result;
        try
        {
            if (MudDialog.Title!.Contains("Crear"))
            {
                CreateClientRequest request = new CreateClientRequest
                {
                    Name = Model.Name,
                    Phone = Model.Phone,
                    Address = Model.Address,
                    Contact = Model.Contact,
                    Zip = Model.Zip,
                    Email = Model.Email,
                    City = Model.City,
                    State = Model.State
                };
                result = await Client.CreateClientAsync(request);
            }
            else
            {
                UpdateClientRequest request = new UpdateClientRequest
                {
                    Name = Model.Name,
                    Phone = Model.Phone,
                    Address = Model.Address,
                    Contact = Model.Contact,
                    Zip = Model.Zip,
                    Email = Model.Email,
                    City = Model.City,
                    State = Model.State
                };
                result = await Client.UpdateClientAsync(Model.Id, request);
            }
            if (result != null)
                MudDialog!.Close(DialogResult.Ok(true));
            else
                Snackbar!.Add($"Oops, there was an error adding a new client.", Severity.Error);
        }
        catch (Exception ex)
        {
            Snackbar!.Add($"Oops, an error occurred. The error type is: {ex.Message}.", Severity.Error);
        }
    }

    void Cancel() => MudDialog.Cancel();
}
