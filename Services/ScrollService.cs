using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;


namespace Services
{

    public class ScrollService : IDisposable
    {
        private readonly NavigationManager _navManager;
        private readonly IJSRuntime _js;

        public ScrollService(NavigationManager navManager, IJSRuntime js)
        {
            _navManager = navManager;
            _js = js;
            _navManager.LocationChanged += OnLocationChanged;
        }

        private async void OnLocationChanged(object sender, LocationChangedEventArgs e)
        {
            await _js.InvokeVoidAsync("scrollToTop");
        }

        public void Dispose()
        {
            _navManager.LocationChanged -= OnLocationChanged;
        }
    }

}
