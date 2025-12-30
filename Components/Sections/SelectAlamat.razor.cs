using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Models;
using Services;


namespace PacificWeb.Components.Sections
{
    public partial class SelectAlamat : ComponentBase
    {

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        public pdjService pdjService { get; set; }

        [Inject]
        public GlobalDataService GlobalDataService { get; set; }

        [Inject]
        public ScopedDataServices ScopedDataServices { get; set; }

        [Inject]
        public NewPdjService NewPdjService { get; set; }

        List<WilayahItem> Provinsi = new();
        List<WilayahItem> Kota = new();
        List<WilayahItem> Kecamatan = new();
        List<WilayahItem> KodePos = new();

        private DotNetObjectReference<SelectAlamat>? _dotNetRef;

        string ProvinsiText, KotaText, KecamatanText;
        string ProvinsiID, KotaID, KecamatanID;
        string KodePosText;
        double Lat, Lng;

        protected override async Task OnInitializedAsync()
        {
            Provinsi = await NewPdjService.GetApiRandom<List<WilayahItem>>(new ParameterApi
            {
                defaultUrl = "https://ihsaninh.github.io/wilayah-indonesia/provinces.json"
            });
        }

        async Task OnProvinsi(ChangeEventArgs e)
        {
            Kota.Clear();
            Kecamatan.Clear();

            ProvinsiID = e.Value?.ToString();
            ProvinsiText = Provinsi.First(x => x.id.ToString() == ProvinsiID).value;
            Kota = await NewPdjService.GetApiRandom<List<WilayahItem>>(new ParameterApi
            {
                defaultUrl = $"https://ihsaninh.github.io/wilayah-indonesia/{ProvinsiID}/regencies.json"
            });
        }

        async Task OnKota(ChangeEventArgs e)
        {
            Kecamatan.Clear();

            KotaID = e.Value?.ToString();
            KotaText = Kota.First(x => x.id.ToString() == KotaID).value;
            Kecamatan = await NewPdjService.GetApiRandom<List<WilayahItem>>(new ParameterApi
            {
                defaultUrl = $"https://ihsaninh.github.io/wilayah-indonesia/{ProvinsiID}/{KotaID}/district.json"
            });
        }

        async Task OnKecamatan(ChangeEventArgs e)
        {
            KecamatanID = e.Value?.ToString();
            KecamatanText = Kecamatan.FirstOrDefault(x => x.id.ToString() == KecamatanID)?.value ?? "";
            var listPostalCode = await NewPdjService.GetApiRandom<List<WilayahItem>>(new ParameterApi
            {
                defaultUrl = $"https://ihsaninh.github.io/wilayah-indonesia/{ProvinsiID}/{KotaID}/{KecamatanID}/subdistrict.json"
            });

            KodePos = listPostalCode.GroupBy(x => x.postal_code).Select(g => g.First()).ToList();
        }

        async Task OnKodePos(ChangeEventArgs e)
        {
            KodePosText = e.Value?.ToString();
        }

        async Task OpenMap()
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("openMapFromAddress",
                $"{KodePosText}, {KecamatanText}, {KotaText}, {ProvinsiText}, Indonesia", _dotNetRef);
        }

        async Task openSelectAlamat()
        {
            await JSRuntime.InvokeVoidAsync("buttonClick", "#staticAlamat2");
        }

        async Task openMaps()
        {
            await JSRuntime.InvokeVoidAsync("openMap");
        }

        [JSInvokable]
        public static Task SetLatLng(double lat, double lng)
        {
            // simpan ke scoped service / state
            Console.WriteLine($"{lat}, {lng}");
            return Task.CompletedTask;
        }

    }
}
