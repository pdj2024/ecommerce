using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;

using Services;

/*
namespace GridWithContextMenu.Data {

   
   
    

    public class ContextMenuItem {
       
        public string Text { get; set; }
        public bool Enabled { get; set; }
        public bool Visible { get; set; }
        public bool BeginGroup { get; set; }
        public string CssClass { get; set; }
        public string IconCssClass { get; set; }
    }

    public class GridContextMenuHelper {
      
       
        static List<ContextMenuItem> CreateRowContextMenuItems() {
            return new List<ContextMenuItem> {
                new ContextMenuItem { Text = "New", BeginGroup = true, IconCssClass="grid-context-menu-item-new-row" },
                new ContextMenuItem { Text = "Edit", IconCssClass="grid-context-menu-item-edit-row" },
                new ContextMenuItem { Text = "Delete", IconCssClass="grid-context-menu-item-delete-row" }
            };
        }

        public static bool IsContextMenuElement(GridElementType elementType) {
            return IsRowContextMenuElement(elementType);
        }
        public static bool IsRowContextMenuElement(GridElementType elementType) {
            switch(elementType) {
                case GridElementType.DataRow:
                case GridElementType.GroupRow:
                case GridElementType.EditRow:
                    return true;
            }
            return false;
        }

        public static async Task ProcessRowMenuItemClickAsync(ContextMenuItem item, int visibleIndex, IGrid grid) {
            switch(item.Text) {
                case "New":
                   
                    break;
                case "Edit":
                    
                    break;
                case "Delete":
                   
                    break;
                 
            }
        }
        public static List<ContextMenuItem> GetRowItems(GridCustomizeElementEventArgs e) {
            var items = CreateRowContextMenuItems();
            foreach(var item in items) {
                item.Visible = true; // IsRowMenuItemVisible(e, item.ItemType);
                item.Enabled = true; // IsRowMenuItemEnabled(e, item.ItemType);
            }
            return items;
        }


    }
}
*/