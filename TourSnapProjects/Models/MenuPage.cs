using System;
using System.Collections.Generic;

using TourSnapModels.Models.Data;
using TourSnapModels.Models.DataBase;

namespace TourSnapProjects.Models
{
    public class MenuPage
    {
        public Int32 ID { get; set; }
        public String Title { get; set; }
        public String Href { get; set; }
        public List<MenuPage> SubItems { get; set; } = new List<MenuPage>();

        public MenuPage(MenuItem Item)
        {
            var Page = Pages.SelectFirst(Global.DataBase, Pages.TableName, $"{Pages.ID} = {Item.Page}");
            if(Page != null)
            {
                this.ID = Page.ID;
                this.Title = Page.Title;
                this.Href = Page.Href;
                List<MenuItem> Submenu = Menu.Select(Global.DataBase, Menu.TableName, $"{Menu.Upper} = {Page.ID}");
                foreach(MenuItem SubmenuItem in Submenu)
                    this.SubItems.Add(new MenuPage(SubmenuItem));
            }
        }
    }
}