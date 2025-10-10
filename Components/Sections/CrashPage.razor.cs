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
using static Services.pdjService;








namespace PacificWeb.Components.Sections
{
    public partial class CrashPage : ComponentBase
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

        public bool loadProses { get; set; } = false;
        public bool auth { get; set; } = false;


        [Parameter] public Produk DataProduk { get; set; } = new Produk();
        [Parameter] public MyHeader header_page { get; set; } = new MyHeader();
        [Parameter] public EventCallback<string> selectProduk { get; set; }

        protected override async Task OnInitializedAsync()
        {

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            
        }

        public async void Back()
        {
            await pdjService.GoBack();
        }

    }
}
