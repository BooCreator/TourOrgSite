using System;

using TourSnapModels.Models.Data;
using TourSnapModels.Models.DataBase;

namespace TourSnapProjects.Models.PublicModels
{
    /// <summary>
    ///  Модель отеля для вывода на страницу
    /// </summary>
    public class HotelModel
    {
        public Int32 ID { get; set; }
        public String Name { get; set; }
        public String Resort { get; set; }
        public Double Price { get; set; }
        public String Category { get; set; }
        public String Eating { get; set; }
        public String[] Photos { get; set; }
        public String Text { get; set; }

        public HotelModel(Otel Item)
        {
            this.ID = Item.ID;
            this.Name = Item.Name;
            var Resort = Resorts.SelectFirst(Global.DataBase, Resorts.TableName, $"{Resorts.ID} = {Item.Resort}");
            this.Resort = (Resort != null) ? Resort.Name : "";
            this.Price = Item.Price;
            var Category = OtelCategories.SelectFirst(Global.DataBase, OtelCategories.TableName, $"{OtelCategories.ID} = {Item.Category}");
            this.Category = (Category != null) ? Category.Title : "";
            var Eating = OtelEatings.SelectFirst(Global.DataBase, OtelEatings.TableName, $"{OtelEatings.ID} = {Item.Eating}");
            this.Eating = (Eating != null) ? Eating.Title : "";
            this.Photos = Item.Photos.ToArray();
            this.Text = Item.Text;
        }
    }
}