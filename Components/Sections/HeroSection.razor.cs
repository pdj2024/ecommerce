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
using static Services.pdjService;








namespace PacificWeb.Components.Sections
{
    public partial class HeroSection
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
            if (firstRender)
            {
                var bannerTask = NewPdjService.GetListGlobalDataAsync<Banner>(new ParameterApi
                {
                    routeApi = "banner",
                    prosesApi = "getAll",
                    ParamUrl = new ParameterApi.ParameterUrl { pagesize = 3, orderBy = "nama_banner", orderDir = "desc", status = "aktif" }
                });
            
                if (GlobalDataService.GlobalData.ListDataBanner?.Count > 0)
                {
                    await JSRuntime.InvokeVoidAsync("initSliderFor", ".hero-slider-new-pc");
                    await JSRuntime.InvokeVoidAsync("initSliderFor", ".hero-slider-new-laptop");
                    await JSRuntime.InvokeVoidAsync("initSliderFor", ".hero-slider-new-tablet");
                    await JSRuntime.InvokeVoidAsync("initSliderFor", ".hero-slider-new-phone");
                }

            }
        }


    }
}
