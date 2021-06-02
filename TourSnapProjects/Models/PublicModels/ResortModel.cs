using System;

using TourSnapModels.Models.Data;
using TourSnapModels.Models.DataBase;

namespace TourSnapProjects.Models.PublicModels
{
    /// <summary>
    /// Модель курорта для вывода на страницу
    /// </summary>
    public class ResortModel
    {
        public Int32 ID { get; set; }
        public String Name { get; set; }
        public String Country { get; set; }
        public String Map { get; set; }
        public String[] Photos { get; set; }
        public String Text { get; set; }
        public ResortModel(Resort Item)
        {
            this.ID = Item.ID;
            this.Name = Item.Name;
            var Country = Countries.SelectFirst(Global.DataBase, Countries.TableName, $"{Countries.ID} = {Item.Country}");
            this.Country = (Country != null) ? Country.Name : "";
            this.Map = (Item.Map.Length > 0) ? Item.Map : Country.Map;
            this.Photos = Item.Photos.ToArray();
            this.Text = Item.Text;
        }
    }
}