using Microsoft.AspNetCore.Mvc;
using WebShared.Services;


namespace PacificWeb.Controllers
{
    [ApiController]
    [Route("api")]
    public partial class LoginController : Controller
    {

        private readonly GlobalDataService GlobalDataService;

        public LoginController(GlobalDataService _GlobalDataService)
        {
            GlobalDataService = _GlobalDataService;
        }

        [HttpPost("update")]
        public async Task<IActionResult> update()
        {

            GlobalDataService.GlobalData.ListDataBanner.Clear();
            GlobalDataService.GlobalData.ListDataBrand.Clear();
            GlobalDataService.GlobalData.ListDataCompany.Clear();
            GlobalDataService.GlobalData.ListDataCompanySocial.Clear();
            GlobalDataService.GlobalData.ListDataKategori.Clear();
            GlobalDataService.GlobalData.ListDataGrupKategori.Clear();
            GlobalDataService.GlobalData.ListDataProduk.Clear();
            GlobalDataService.GlobalData.ListDataNavbar.Clear();
            GlobalDataService.GlobalData.ListDataNilaiDefault.Clear();
            GlobalDataService.GlobalData.ListDataPembayaran.Clear();
            GlobalDataService.GlobalData.ListDataPengiriman.Clear();
            GlobalDataService.GlobalData.ListDataSummaryPesanan.Clear();
            GlobalDataService.GlobalData.ListDataTexkSection.Clear();

            return Ok(new
            {
                success = true,
                message = "GlobalState berhasil dikosongkan"
            });
        }

    }
}
