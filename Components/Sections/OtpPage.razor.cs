using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Models;
using PacificWeb.Components.Layout;
using Radzen;
using Radzen.Blazor;
using Services;
using System.Configuration;
using System.Drawing;
using System.Reflection;
using System.Text;



namespace PacificWeb.Components.Sections
{
    public partial class OtpPage : ComponentBase
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected AuthenticationStateProvider AuthStateProvider { get; set; }

        [Inject]
        protected TooltipService TooltipService { get; set; }

        [Inject]
        protected ContextMenuService ContextMenuService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        [Inject]
        public pdjService pdjService { get; set; }

        [Inject]
        public NewPdjService NewPdjService { get; set; }

        [Inject]
        public ScopedDataServices ScopedDataServices { get; set; }

        public bool loadProses { get; set; } = false;
        public bool auth { get; set; } = false;

        [Parameter]
        public string? UId { get; set; }

        [Parameter]
        public string? NoTlp { get; set; }

        [Parameter]
        public string? Pass { get; set; }

        public string? TokenJwt { get; set; }

        private async Task LoginStart()
        {
            LoginViewModel Model = new();

            var param = new ParameterApi
            {
                routeApi = "auth",
                prosesApi = "login",
                ParamUrl = new ParameterApi.ParameterUrl { }
            };

            Model.NoTlp = NoTlp;
            Model.Password = Pass;

            var loginStatus = await NewPdjService.CheckLoginApi(param, Model);

            if (loginStatus)
            {
                auth = false;
                StateHasChanged();

                var dataDecodeA = await pdjService.DecodeEncode(true, new List<string> { ScopedDataServices.ScopedData.DataLogin.No_Id });
                NavigationManager.NavigateTo($"/auth-user-session/session/{dataDecodeA[0]}", forceLoad: true);
            } 
            else
            {
                auth = true;
                StateHasChanged();
                return;
            }


        }

        public async Task sendOtp()
        {
            var verifyCode = await JSRuntime.InvokeAsync<string>("verifyCode");

            if (verifyCode != null || verifyCode != "")
            {
                TokenJwt = verifyCode;

                var data = await pdjService.RegistUserApi(new ParameterApi
                {
                    routeApi = "auth",
                    prosesApi = "register",
                }, new DataLogin { Telepon = NoTlp , tokenJwt = TokenJwt, Password = Pass });

                if (data)
                {
                    await Task.Delay(1000);
                    await LoginStart();

                    loadProses = false;
                    return;
                } 
                
                else
                {
                    loadProses = true;
                    return;
                }
            } 
            
            else
            {
                loadProses = true;
            }
        }
    }
}
