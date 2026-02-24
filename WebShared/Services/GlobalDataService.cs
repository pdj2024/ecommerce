using WebShared.Models;

namespace WebShared.Services
{
    public class GlobalDataService
    {
        public SourchData GlobalData { get; private set; } = new();

        public event Action? OnChange;

        public void GlobalStateChanged()
        {
            OnChange?.Invoke();
        }
    }
}