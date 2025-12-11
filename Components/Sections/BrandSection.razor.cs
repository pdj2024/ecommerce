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
    public partial class BrandSection
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
        public GlobalDataService GlobalDataService { get; set; }

        [Inject]
        public ScopedDataServices ScopedDataServices { get; set; }

        public bool loadContent { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            var data = NewPdjService.GetListGlobalDataAsync<Brand>(new ParameterApi
            {
                routeApi = "brand",
                prosesApi = "getAll",
                ParamUrl = new ParameterApi.ParameterUrl { orderBy = "nama_brand", orderDir = "desc", status = "aktif" }
            });

            StateHasChanged();
        }

        public void SearchCombo(Brand brand)
        {
            var bra = "";
            if (brand != null)
            {
                bra = Convert.ToBase64String(Encoding.UTF8.GetBytes((brand.No_Id == null || brand.No_Id == "" ? brand.Id : brand.No_Id)));
                NavigationManager.NavigateTo($"/search-page?keyword=&produk=&kategori=&brand={Uri.EscapeDataString(bra)}", false);
                return;
            }
        }

    }
}
