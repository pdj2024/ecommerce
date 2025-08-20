using Microsoft.JSInterop;
namespace Services
{
    public class ScreenSizeService
    {
        private readonly IJSRuntime _jsRuntime;

        public bool IsSmallScreen { get; private set; }

        public ScreenSizeService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task RefreshScreenSizeAsync()
        {
            var width = await _jsRuntime.InvokeAsync<int>("eval", "window.innerWidth");
            IsSmallScreen = width <= pdjService.screenBreakpoint; // Adjust the breakpoint as needed
        }
    }

}