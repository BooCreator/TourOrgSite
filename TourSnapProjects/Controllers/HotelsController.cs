using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using TourSnapModels.Models.Data;
using TourSnapModels.Models.DataBase;
using ToursTable = TourSnapModels.Models.DataBase.Tours;

using TourSnapProjects.Models.PublicModels;

namespace TourSnapProjects.Controllers
{
    public class HotelsController : Controller
    {
        // GET: Hotels
        // Страница конкретного отеля
        public ActionResult Get(Int32 id = -1)
        {
            this.LoadUserData();
            this.LoadMainMenu();
            HotelModel Item = null;
            // получаем данные отеля
            Otel Hotel = Otels.SelectFirst(Global.DataBase, Otels.TableName, $"{Otels.ID} = {id}");
            // если данные получены
            if(Hotel != null)
            {
                Item = new HotelModel(Hotel);

                // получаем данные о турах в отель
                List<TourModel> Tours = new List<TourModel>();

                foreach(Tour Tour in ToursTable.Select(Global.DataBase, ToursTable.TableName, $"{ToursTable.Otel} = {Hotel.ID}"))
                    Tours.Add(new TourModel(Tour));

                this.ViewBag.HotelTours = Tours;
            }
            this.ViewBag.Item = Item;
            return this.View();
        }
    }
}