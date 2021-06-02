using System;

using TourSnapModels.Models.Data;
using TourSnapModels.Models.DataBase;

namespace TourSnapProjects.Models.PublicModels
{
    /// <summary>
    /// Модель тура для вывода на страницу
    /// </summary>
    public class TourModel
    {
        public Int32 ID { get; set; }
        public String Otel { get; set; }
        public String Title { get; set; }
        public String Photo { get; set; }
        public Int32 Days { get; set; }
        public String Date { get; set; }
        public Double Price { get; set; }
        public String Text { get; set; }

        public Boolean IsFire { get; set; }
        public Int32 FireDays { get; set; }
        public String StartDate { get; set; }
        public String EndDate { get; set; }
        public Double FirePrice { get; set; }

        public TourModel(Tour Item)
        {
            this.ID = Item.ID;
            var Otel = Otels.SelectFirst(Global.DataBase, Otels.TableName, $"{Otels.ID} = {Item.Otel}");
            this.Otel = (Otel != null) ? Otel.Name : "";
            this.Days = Item.Days;
            this.Date = Item.Date.ToString("dd-MM-yyyy");
            this.Price = Otel.Price * Item.Days + Item.Price;
            this.Text = Item.Text;
            this.Title = Item.Title;
            this.Photo = (Item.Photo.Length > 0) ? "tours/" + Item.Photo : "hotels/" + Otel.Photos[0];

            var Fire = FireTours.SelectFirst(Global.DataBase, FireTours.TableName, $"{FireTours.Tour} = {Item.ID}");
            if(Fire != null)
            {
                this.IsFire = true;
                this.FireDays = (Fire.EndDate - Fire.StartDate).Days;
                this.StartDate = Fire.StartDate.ToString("dd.MM.yyyy");
                this.EndDate = Fire.EndDate.ToString("dd.MM.yyyy");
                this.FirePrice = Otel.Price * Item.Days + Fire.Price;
            }
        }
    }
    /// <summary>
    /// Модуль горящего тура для вывода на страницу
    /// </summary>
    public class FireTourModel
    {
        public Int32 ID { get; set; }
        public String Otel { get; set; }
        public String Title { get; set; }
        public String Photo { get; set; }
        public Int32 Days { get; set; }
        public String Date { get; set; }
        public Double TourPrice { get; set; }
        public String Text { get; set; }
        public Int32 FireDays { get; set; }
        public String StartDate { get; set; }
        public String EndDate { get; set; }
        public Double FirePrice { get; set; }

        public FireTourModel(FireTour Item, Tour Tour = null)
        {
            if(Tour == null)
                Tour = Tours.SelectFirst(Global.DataBase, Tours.TableName, $"{Tours.ID} = {Item.Tour}");
            this.ID = Tour.ID;
            var Otel = Otels.SelectFirst(Global.DataBase, Otels.TableName, $"{Otels.ID} = {Tour.Otel}");
            this.Otel = (Otel != null) ? Otel.Name : "";
            this.Days = Tour.Days;
            this.Date = Tour.Date.ToString("dd.MM.yyyy");
            this.TourPrice = Otel.Price * Tour.Days + Tour.Price;
            this.Text = Tour.Text;
            this.Title = Tour.Title;
            this.Photo = (Tour.Photo.Length > 0) ? "tours/" + Tour.Photo : "hotels/" + Otel.Photos[0];

            this.FireDays = (Item.EndDate - Item.StartDate).Days;
            this.StartDate = Item.StartDate.ToString("dd.MM.yyyy");
            this.EndDate = Item.EndDate.ToString("dd.MM.yyyy");
            this.FirePrice = Otel.Price * Tour.Days + Item.Price;
        }
        
    }
}