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
    public partial class KategoriSection
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

        [Parameter]
        public MyHeader header_page { get; set; }

        public bool loadContent { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            var kategoriTask = NewPdjService.GetListGlobalDataAsync<Kategori>(new ParameterApi
            {
                routeApi = "kategori",
                prosesApi = "getAll",
                ParamUrl = new ParameterApi.ParameterUrl { pagesize = 3, status = "aktif" }
            });

            await Task.WhenAll(kategoriTask);

            await JSRuntime.InvokeVoidAsync("initKate");

            StateHasChanged();
        }

        public void searchCombo(Navbar Navbar = null, Brand brand = null, Kategori kategori = null)
        {
            var bra = "";

            var kate = "";

            if (Navbar == null)
            {
                if (brand != null)
                {
                    bra = Convert.ToBase64String(Encoding.UTF8.GetBytes((brand.No_Id == null || brand.No_Id == "" ? brand.Id : brand.No_Id)));
                    NavigationManager.NavigateTo($"/search-page?keyword=&produk=&kategori={Uri.EscapeDataString(kate)}&brand={Uri.EscapeDataString(bra)}", true);
                    return;
                }
                else if (kategori != null)
                {
                    kate = Convert.ToBase64String(Encoding.UTF8.GetBytes((kategori.No_Id == null || kategori.No_Id == "" ? kategori.Id : kategori.No_Id)));
                    NavigationManager.NavigateTo($"/search-page?keyword=&produk=&kategori={Uri.EscapeDataString(kate)}&brand=", true);
                    return;
                }
            }


            kate = string.Join(",", Navbar.Filter.Select(x => x));


            if (brand != null)
            {

                bra = Convert.ToBase64String(Encoding.UTF8.GetBytes((brand.No_Id == null || brand.No_Id == "" ? brand.Id : brand.No_Id)));

            }
            else if (kategori != null)
            {
                kate = kate + $",{(kategori.No_Id == null || kategori.No_Id == "" ? kategori.Id : kategori.No_Id)}";
            }

            kate = Convert.ToBase64String(Encoding.UTF8.GetBytes(kate));

            NavigationManager.NavigateTo($"/search-page?keyword=&produk=&kategori={Uri.EscapeDataString(kate)}&brand={Uri.EscapeDataString(bra)}", true);
        }
    }
}
