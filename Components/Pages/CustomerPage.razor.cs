using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Models;
using Radzen.Blazor;
using Radzen;
using Services;
using System.Reflection;
using DevExpress.Blazor;
using System.Configuration;
using DevExpress.XtraCharts.Native;
using System.Drawing;

using DevExpress.Blazor.Internal.Grid;






namespace PacificWeb.Components.Pages
{
    public partial class CustomerPage 
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

        [Inject]
         public pdjService pdjService { get; set; }

        IList<MasterCustomer> selectedFromGrid;
        IList<MasterCustomer> selectedCustomer;
        // protected RadzenDataGrid<Models.MasterCustomer> grid0;
       
        IGrid devGrid { get; set; }

        protected IEnumerable<MasterCustomer> tbListData;

        protected IEnumerable<MasterCustomer> tbListDataFromDb;

        protected string namaMenu = "Pelanggan";
       

        protected string BtnTambahStyleEnabled = "background-color: transparent; color: #1e3a6a; opacity: 1";

        protected string BtnTambahStyleDisabled = "background-color:transparent; color: #1e3a6a; opacity: 0.5";

        protected bool BtnTambahIsDisabled = false;

        protected string search = "";
        protected bool BtnActionIsVisible = true;
        protected string selectedFilter = "Aktif";

        protected bool allowPickColumn = false;
        protected string columnToggleIcon = "toggle_off";
        protected string columnToggleText = "Pilih Kolom";

        protected bool myColumnVisible = true;
        protected string searchText = "";

        private string lastSelectedIdentifier;

        private MasterCustomer SelectedCustomer;

        GridContextMenuContainer ContextMenuContainer { get; set; }


      
        string SearchText { get; set; }
        string GridSearchText { get; set; }
      


        private List<Models.ContextMenuItem> contextMenuItems = new List<Models.ContextMenuItem>
    {
        new Models.ContextMenuItem { Text = "Piutang",  },
        new Models.ContextMenuItem { Text = "History Transaksi" },
        //new Models.ContextMenuItem { Text = "Menu 3" }
    };
        private async Task HandleRowMenuItemClick((Models.ContextMenuItem item, int visibleIndex) args)
        {
            var (item, visibleIndex) = args;

            switch (item.Text)
            {
                case "Piutang":
                    await OpenReportDialog((devGrid.GetRowValue(visibleIndex, "no_cussup")).ToString());
                    break;
                case "History Transaksi":
                 //   await OpenReportDialog2((devGrid.GetRowValue(visibleIndex, "no_cussup")).ToString());
                    break;
                /*case "Menu 3":
                    pdjService.ShowNotification($"Menu 3: {devGrid.GetRowValue(visibleIndex, "no_cussup")}");
                    break;*/
            }
        }

        
            public async Task OpenReportDialog(string kode)
        {
          

          

        }
      



       
        protected override async Task OnInitializedAsync()
        {
            await FetchData();
            performGridRefresh();
            //  tbListData = tbListDataFromDb;
        }
        private void performGridRefresh()
        {
           
            tbListData = tbListDataFromDb;

            // Optionally, you might want to trigger a UI update or refresh
            StateHasChanged();
        }

     
       
       
        private async Task FetchData()
        {
            // Fetch latest data from the server
            tbListDataFromDb = await pdjService.GetMasCust("", selectedFilter.ToUpper());

          


        }

        protected async void Refresh(string kode = "")
        {
           
            

            

            await FetchData();
            performGridRefresh();

            // Ensure the UI updates to reflect the new data
            StateHasChanged();



            // Restore the selection after data refresh
            if (!string.IsNullOrEmpty(kode))
            {
                // Convert tbListData to a list
                var customersList = tbListData.ToList();

                var selectedItem = customersList.FirstOrDefault(c => c.no_cussup == kode);
                if (selectedItem != null)
                {
                    

                    devGrid.SetFocusedDataItemAsync(selectedItem);

                    // Ensure the UI reflects the updated selection
                    StateHasChanged();
                }
            }

           
        }

        
        protected async Task OpenEditDialog(string proses, string kode)
        {
            var parameters = new Dictionary<string, object>
            {
                { "ParamProses", proses }
            };

            if (!string.IsNullOrEmpty(kode))
            {
                parameters.Add("ParamNo", kode);
            }

           var result = await DialogService.OpenAsync<CustomerDetil>(proses + " " + namaMenu, parameters, pdjService.dialogOptions);

           

            if (proses.ToUpper() != "HAPUS" &&  !string.IsNullOrEmpty(result))
            {
                // Handle the new No_Cust value, e.g., update the grid or refresh data
               
                Refresh(result);
            }
            else
            {
                Refresh(); // Call without the new No_Cust if it's not applicable
            }
        }
        
        // ON SEARCH / SELECT WIDGETS
        void OnTextChanged(string newValue)
        {
            if (newValue != null)
            {
                GridSearchText = newValue;
            }

        }
        private async void OnFilterChanged()
        {
            // await BtnRefreshClick(new MouseEventArgs());
            Refresh();

        }

        // BUTTONS ===================
        void SplitOnClick(RadzenSplitButtonItem item, string buttonName)
        {
            if (item != null)
            {
                if (item.Value == "Edit Header")
                {

                }

                if (item.Value == "Cetak")
                {

                }
            }
            else
            {
                // NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Info, Summary = "SplitButton Clicked", Detail = $"{buttonName} clicked" });
            }
        }
     
        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await OpenEditDialog("Tambah", null);
          //  await GrdViewDetBrg.Reload();


        }
       
        protected async Task BtnRefreshClick(MouseEventArgs args)
        {

            Refresh();
           
        }
        protected async Task ToggleButtonClick(MouseEventArgs args)
        {


           
            devGrid.ShowColumnChooser();


        }


   

        private async Task EditRow(GridRowClickEventArgs e)
        {
            if (e.VisibleIndex >= 0)
            {
                string cellValue;
                cellValue = devGrid.GetRowValue(e.VisibleIndex, "no_cussup").ToString();
                await OpenEditDialog("Koreksi", cellValue);


            }
        }
       
        // STYLE =====================
        void Grid_CustomizeElement(GridCustomizeElementEventArgs e)
        {
            if (e.ElementType == GridElementType.DataRow)
            {
                // e.CssClass = "alt-item";
           
                if (e.VisibleIndex % 2 == 1)
                {
                    e.Style = "background-color: #f4f5f7";
                } else
                {
                    e.Style = "background-color: #ffffff";
                }

                var rowAktif = Convert.ToBoolean(e.Grid.GetRowValue(e.VisibleIndex, "aktif"));
                if (!rowAktif)
                {
                    //e.Style = "background-color: #FFE0E0";
                    e.Style = "background-color: #FFE0E0"; // "background-color: rgba(204, 51, 51, 0.3); ";
                }
            } 
            if (e.ElementType == GridElementType.HeaderCell)
            {
                e.Style = "border-color: rgba(255,255,255, 0.5); padding-top: 0.95em; padding-bottom: 0.95em";
              
            }


           


        }



        
    }
}
