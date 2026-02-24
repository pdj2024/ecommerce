using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Text;
using WebShared.Models;
using WebShared.Services;








namespace WebClient.Sections
{
    public partial class BrandSection
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }


        [Inject]
        protected AuthenticationStateProvider AuthStateProvider { get; set; }

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
