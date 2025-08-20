using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;
using Services;



using System.Collections;


namespace PacificWeb.Components.Pages
{
    public partial class GridContextMenuContainer
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


        DxContextMenu ColumnContextMenu { get; set; }
        DxContextMenu RowContextMenu { get; set; }

        IEnumerable ColumnContextMenuData { get; set; }
        IEnumerable RowContextMenuData { get; set; }

        IGridColumn ContextMenuColumn { get; set; }
        int ContextMenuRowIndex { get; set; }


        [Parameter] public List<Models.ContextMenuItem> RowContextMenuItems { get; set; }




        [Parameter] public IGrid Grid { get; set; }

        /* async Task RowContextMenu_ItemClick(ContextMenuItemClickEventArgs e)
             => await ProcessRowMenuItemClickAsync((Models.ContextMenuItem)e.ItemInfo.DataItem, ContextMenuRowIndex, Grid);*/

        // Define an EventCallback to delegate the click logic to the parent
        [Parameter] public EventCallback<(Models.ContextMenuItem item, int visibleIndex)> OnRowMenuItemClick { get; set; }
        async Task RowContextMenu_ItemClick(ContextMenuItemClickEventArgs e)
        {
            var selectedItem = (Models.ContextMenuItem)e.ItemInfo.DataItem;
            await OnRowMenuItemClick.InvokeAsync((selectedItem, ContextMenuRowIndex));
        }


        public async Task Grid_ContextMenu(GridCustomizeElementEventArgs e, MouseEventArgs mouseArgs)
        {
            if (IsRowContextMenuElement(e.ElementType))
            {
                ContextMenuRowIndex = e.VisibleIndex;
                /*RowContextMenuData = RowContextMenuItems; //GetRowItems(e);

                var items = CreateRowContextMenuItems();*/
                foreach (var item in RowContextMenuItems)
                {
                    item.Visible = true; // IsRowMenuItemVisible(e, item.ItemType);
                    item.Enabled = true; // IsRowMenuItemEnabled(e, item.ItemType);
                }

                RowContextMenuData = RowContextMenuItems;
                await RowContextMenu.ShowAsync(mouseArgs);
            }
        }

        // === CONTEXT MENU =============



       /* static List<Models.ContextMenuItem> CreateRowContextMenuItems()
        {
            return new List<Models.ContextMenuItem> {
         new Models.ContextMenuItem { Text = "Piutang", BeginGroup = true, IconCssClass="grid-context-menu-item-new-row" },
         new Models.ContextMenuItem { Text = "History Transaksi", IconCssClass="grid-context-menu-item-edit-row" },
         new Models.ContextMenuItem { Text = "Menu 3", IconCssClass="grid-context-menu-item-delete-row" }
     };
        }*/

        public static bool IsContextMenuElement(GridElementType elementType)
        {
            return IsRowContextMenuElement(elementType);
        }
        public static bool IsRowContextMenuElement(GridElementType elementType)
        {
            switch (elementType)
            {
                case GridElementType.DataRow:
                case GridElementType.GroupRow:
                case GridElementType.EditRow:
                    return true;
            }
            return false;
        }

        public async Task ProcessRowMenuItemClickAsync(Models.ContextMenuItem item, int visibleIndex, IGrid grid)
        {
            switch (item.Text)
            {
                case "Piutang":

                    pdjService.ShowNotification("Piutang: " + grid.GetRowValue(visibleIndex, "no_cussup"));
                    break;
                case "History Transaksi":

                    pdjService.ShowNotification("History Transaksi: " + grid.GetRowValue(visibleIndex, "no_cussup"));

                    break;
                case "Menu 3":

                    pdjService.ShowNotification("Menu 3: " + grid.GetRowValue(visibleIndex, "no_cussup"));

                    break;

            }
        }
       /* public static List<Models.ContextMenuItem> GetRowItems(GridCustomizeElementEventArgs e)
        {
            var items = CreateRowContextMenuItems();
            foreach (var item in items)
            {
                item.Visible = true; // IsRowMenuItemVisible(e, item.ItemType);
                item.Enabled = true; // IsRowMenuItemEnabled(e, item.ItemType);
            }
            return items;
        }*/
    }
}
