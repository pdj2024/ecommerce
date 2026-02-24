using WebShared.Models;

namespace WebShared.Services
{
    public class ScopedDataServices
    {
        public ScopedData ScopedData { get; private set; } = new();

        public event Action? OnChange;

        public void ScopedStateChanged()
        {
            OnChange?.Invoke();
        }
    }
}