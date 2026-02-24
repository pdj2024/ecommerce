using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using WebClient.Layout;
using WebShared.Models;
using WebShared.Services;








namespace WebClient.Sections
{
    public partial class CrashPage : ComponentBase
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }


        [Inject]
        protected AuthenticationStateProvider AuthStateProvider { get; set; }

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
