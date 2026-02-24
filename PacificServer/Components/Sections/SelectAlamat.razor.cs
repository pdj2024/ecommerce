using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WebShared.Models;
using PacificWeb.Components.Pages;
using WebShared.Services;
using System.Xml.Linq;
using static WebShared.Models.Pesanan;


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

        [Parameter] public Headers Head { get; set; } = new();

        List<WilayahItem> Provinsi = new();
        List<WilayahItem> Kota = new();
        List<WilayahItem> Kecamatan = new();
        List<WilayahItem> KodePos = new();

        public AlamatMaps AlamatMaps { get; set; } = new();
        public AlamatMaps AlamatUser { get; set; } = new();

        private DotNetObjectReference<SelectAlamat>? _dotNetRef;
        //private DotNetObjectReference<SelectAlamat>? _dotNetRef;

        bool currentPosition, currentPosition3;
        bool currentPosition2 = false;
        string ProvinsiText, KotaText, KecamatanText;
        string ProvinsiID, KotaID, KecamatanID;
        string KodePosText;
        double Lat, Lng;

        protected override async Task OnInitializedAsync()
        {
            //if (Provinsi.FirstOrDefault() == null) return;

            var ProvinsiData = await NewPdjService.GetApiRandom<WilayahItem>(new ParameterApi
            {
                defaultUrl = "https://wilayah.web.id/api/provinces",
                ParamUrl = new ParameterApi.ParameterUrl()
                {
                    limit = 200,
                    page = 1
                }
            });


            Provinsi = ProvinsiData.Data;

            StateHasChanged();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await Task.WhenAll();

                _dotNetRef = DotNetObjectReference.Create(this);
                await JSRuntime.InvokeVoidAsync("openMap", AlamatUser.Postcode, _dotNetRef);
                currentPosition = false;
                await JSRuntime.InvokeVoidAsync("getLocation", false);

            }
        }

        async Task AturUlangAddres()
        {
            AlamatUser.Province = "Provinsi";
            AlamatUser.City = "Kota";
            AlamatUser.District = "Kecamatan";
            AlamatUser.Postcode = "Kode Pos";


            Kota.Clear();
            Kecamatan.Clear();
            KodePos.Clear();
        }

        async Task SugentionMark(bool show)
        {
            if(show)
            {
                await JSRuntime.InvokeVoidAsync("getManualInputGeocode");
            }

            await JSRuntime.InvokeVoidAsync("ShowMaps", show);
        }

        async Task getCurrentPosition()
        {
            await NewPdjService.CopyPropertiesAsync(AlamatMaps, AlamatUser);
            AlamatUser.StreetDetail = "Detail Alamat";

            //Provinsi.Add(new WilayahItem { name = AlamatUser.Province });
            Kota.Add(new WilayahItem { name = AlamatUser.City });
            Kecamatan.Add(new WilayahItem { name = AlamatUser.District });
            KodePos.Add(new WilayahItem { postal_code = AlamatUser.Postcode });

            currentPosition = false;

            await Task.Delay(500);
            await JSRuntime.InvokeVoidAsync("buttonClick", "#StaticCloseAlamat");
        }

        async Task OpenDropDown(string idName)
        {
            //if(idName == "#dropdown-list-kodepos")
            //{

            //}

            await JSRuntime.InvokeVoidAsync("openDropAddress", idName);
        }

        async Task OnProvinsi(string e, string idClass)
        {
            await JSRuntime.InvokeVoidAsync("closeDropAddress", idClass);

            AlamatUser.City = "Kota";
            AlamatUser.District = "Kecamatan";
            AlamatUser.Postcode = "Kode Pos";

            ProvinsiID = e.ToString();
            AlamatUser.Province = Provinsi.First(x => x.code.ToString() == ProvinsiID).name;

            var KotaData = await NewPdjService.GetApiRandom<WilayahItem>(new ParameterApi
            {
                defaultUrl = $"https://wilayah.web.id/api/regencies/{ProvinsiID}",
                ParamUrl = new ParameterApi.ParameterUrl()
                {
                    limit = 200,
                    page = 1
                }
            });

            Kota = KotaData.Data;

            Kecamatan.Clear();
            KodePos.Clear();

            StateHasChanged();
        }

        async Task OnKota(string e, string idClass)
        {
            await JSRuntime.InvokeVoidAsync("closeDropAddress", idClass);

            AlamatUser.District = "Kecamatan";
            AlamatUser.Postcode = "Kode Pos";

            KotaID = e.ToString();
            AlamatUser.City = Kota.First(x => x.code.ToString() == KotaID).name;

            var KecamatanData = await NewPdjService.GetApiRandom<WilayahItem>(new ParameterApi
            {
                defaultUrl = $"https://wilayah.web.id/api/districts/{KotaID}",
                ParamUrl = new ParameterApi.ParameterUrl()
                {
                    limit = 200,
                    page = 1
                }
            });

            Kecamatan = KecamatanData.Data;

            KodePos.Clear();

            StateHasChanged();
        }

        async Task OnKecamatan(string e, string idClass)
        {
            
            await JSRuntime.InvokeVoidAsync("closeDropAddress", idClass);

            AlamatUser.Postcode = "Kode Pos";

            KecamatanID = e.ToString();
            AlamatUser.District = Kecamatan.FirstOrDefault(x => x.code.ToString() == KecamatanID)?.name ?? "";

            var listPostalCode = await NewPdjService.GetApiRandom<WilayahItem>(new ParameterApi
            {
                defaultUrl = $"https://wilayah.web.id/api/villages/{KecamatanID}",
                ParamUrl = new ParameterApi.ParameterUrl()
                {
                    limit = 200,
                    page = 1
                }
            });

            KodePos = listPostalCode.Data;
            KodePos = KodePos?.GroupBy(x => x.postal_code).Select(g => g.First()).ToList();

            StateHasChanged();
        }

        async Task OnKodePos(string e, string idClass)
        {
            await JSRuntime.InvokeVoidAsync("closeDropAddress", idClass);
            AlamatUser.Postcode = e.ToString();

            await NewPdjService.CopyPropertiesAsync(AlamatUser, AlamatMaps);

            await Task.Delay(500);
            await JSRuntime.InvokeVoidAsync("buttonClick", "#StaticCloseAlamat");
        }

        async Task openMaps(bool status)
        {
            if (status)
            {
                await pdjService.ButtonCLick("staticAlamatButton");
                return;
            }
            else await pdjService.ButtonCLick("staticDetailButton");

            //_dotNetRef = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("openMap", AlamatMaps.Postcode, _dotNetRef);
        }

        [JSInvokable]
        public static Task SetLatLng(double lat, double lng)
        {
            // simpan ke scoped service / state
            Console.WriteLine($"{lat}, {lng}");
            return Task.CompletedTask;
        }

        [JSInvokable]
        public async Task CurrentAddress(AlamatMaps dataAddres)
        {

            NewPdjService.ShowConsoleData(dataAddres, "currentAddres");

            AlamatMaps = dataAddres;

            if (currentPosition) 
            {
                AlamatUser = dataAddres;
                AlamatUser.StreetDetail = "Detail Alamat";

                //Provinsi.Add(new WilayahItem { name = AlamatUser.Province });
                Kota.Add(new WilayahItem { name = AlamatUser.City });
                Kecamatan.Add(new WilayahItem { name = AlamatUser.District });
                KodePos.Add(new WilayahItem { postal_code = AlamatUser.Postcode });

                currentPosition = false;

                await Task.Delay(2000);
            }


            StateHasChanged();
        }
    }
}
