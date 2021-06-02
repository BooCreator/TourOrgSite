using System;

using TourSnapModels.Models.Data;

namespace TourSnapProjects.Models.PublicModels
{
    /// <summary>
    /// Модель страны для вывода на главную страницу
    /// </summary>
    public class MainCountryModel
    {
        public String Image { get; set; }
        public String Title { get; set; }

        public MainCountryModel(Country Item)
        {
            this.Image = Item.Photo;
            this.Title = Item.Name;
        }
    }
}