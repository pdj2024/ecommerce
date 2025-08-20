using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using Models;
using Services;
using DevExpress.Blazor;
using BlazorBootstrap;

namespace PacificWeb.Components.Pages
{
    public partial class CustomerDetil
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected TooltipService TooltipService { get; set; }

        [Inject]
        protected ContextMenuService ContextMenuService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        [Parameter]
        public string ParamProses { get; set; }

        [Parameter]
        public string ParamNo { get; set; }

        [Inject]
        public pdjService pdjService { get; set; }

       protected bool kode_is_disabled = true;
        protected string generated_kode = "";
        /*protected bool nm_cussup_is_disabled = false;
        protected bool kontak_is_disabled = false;
        protected bool aktif_is_disabled = false;
        protected bool keterangan_is_disabled = false;
        protected bool alamat_is_disabled = false;
        protected bool kelurahan_is_disabled = false;
        protected bool kecamatan_is_disabled = false;
        protected bool kota_is_disabled = false;
        protected bool kodepos_is_disabled = false;
        protected bool telpon_is_disabled = false;
        protected bool no_npwp_is_disabled = false;
        protected bool nama_npwp_is_disabled = false;
        protected bool alamat_npwp_is_disabled = false;
        protected bool kota_npwp_is_disabled = false;
        protected bool tempo_is_disabled = false;
        protected bool plafon_is_disabled = false;*/

        private RadzenTextBox customerDetilField;
        protected bool BtnHapusIsVisible = false;

        RadzenTextBox rTextBox;
        protected List<string> statusOptions = new List<string> { "Aktif", "Non Aktif" };
        protected override async Task OnInitializedAsync()
        {
            
            

            if (ParamProses.ToUpper() != "TAMBAH")
            {
                BtnHapusIsVisible = true;
                listOfData = await pdjService.GetMasCust(ParamNo, "");
                if (listOfData.Any())
                {
                    // Assign the first customer to tbCustomer
                    tbData = listOfData.First();

                }
                else
                {

                    // Handle the case where there are no customers
                    tbData = null; // Or another appropriate default value
                }

                // sudah tidak boleh edit kode
                kode_is_disabled = true;

            } else
            {
                tbData = new MasterCustomer();

                
                tbData.aktif_text = statusOptions[0];
                // tbData.no_cussup = "akan tercipta";

              
            }
           

        }

        


        protected bool errorVisible;
        protected MasterCustomer tbData;
        protected List<MasterCustomer> listOfData;

        protected async void isiData()
        {
           

        }
        protected async Task FormSubmit()
        {
            /*try
            {
                await TradeSolution2Service.CreateTbCustomer(tbCussup);
                DialogService.Close(tbCussup);
            }
            catch (Exception ex)
            {
                errorVisible = true;
            }*/
        }
        /*protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Wait a little for rendering
                await Task.Delay(5000); // Adjust if necessary

                // Set focus to the specific RadzenTextBox
                await pdjService.SetFocusAndSelect("CustomerDetilField1");
            }
        }*/
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {

                JSRuntime.InvokeVoidAsync("eval", $@"setTimeout(function() {{
    document.getElementById(""CustomerDetilField1"").focus();
}}, 500);");

            }

        }
        protected async Task BtnBatalClick(MouseEventArgs args)
        {
            DialogService.Close(null);
        }
        protected async Task BtnSaveClick(MouseEventArgs args)
        {
            try
            {
                if (tbData.nm_cussup == "") {
                    pdjService.ShowNotification("Nama perusahaan harus diisi", "Perhatian...");
                    return;
                }
                tbData.aktif = tbData.aktif_text == "Aktif"? true: false;
                generated_kode = await pdjService.CudMasCust(ParamProses.ToUpper(), 10, tbData);

                //   pdjService.ShowNotification("Berhasil " + ParamProses);
                // Close the dialog and pass the new No_Cust back to the parent
               
                DialogService.Close(generated_kode);
            }
            catch (Exception ex)
            {
                pdjService.ShowNotification(ex.Message, "Gagal " + ParamProses);
            }
            
        }
        protected async Task BtnHapusClick(MouseEventArgs args)
        {
            if (await DialogService.Confirm(
    pdjService.yakinHapus,      // Message
    pdjService.judulDialogKonfirmasi,       // Title of the dialog
    new ConfirmOptions()
    {
        OkButtonText = pdjService.yes,   // Text for the confirmation button
        CancelButtonText = pdjService.tidak // Text for the cancellation button
    }) == true)
            {
                try
                {
               
                   
                

                    //await pdjService.CudMasCust("HAPUS", 10, tbData);
                    await pdjService.CudMasCust("HAPUS", 10, tbData);
                    
                    //   pdjService.ShowNotification("Berhasil " + ParamProses);
                   DialogService.Close(null);
                }
                catch (Exception ex)
                {
                    pdjService.ShowNotification(ex.Message, "Gagal " + "Hapus");
                   /* NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = $"Error",
                        Detail = $"Unable to delete TbCustomer"
                    });*/

                }
            }

        }

    }
}