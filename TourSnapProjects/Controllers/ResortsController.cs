using System;
using System.Collections.Generic;
using System.Web.Mvc;

using TourSnapModels.Models.Data;
using TourSnapModels.Models.DataBase;

using TourSnapProjects.Models.PublicModels;

namespace TourSnapProjects.Controllers
{
    public class ResortsController : Controller
    {
        // GET: Resorts
        // Страница конкретного курорта
        public ActionResult Get(Int32 id = -1)
        {
            this.LoadUserData();
            this.LoadMainMenu();
            ResortModel Item = null;
            // получаем данные курорта
            Resort Resort = Resorts.SelectFirst(Global.DataBase, Resorts.TableName, $"{Resorts.ID} = {id}");
            if(Resort != null)
            {
                Item = new ResortModel(Resort);

                // получаем данные о отелях курорта
                List<HotelModel> Hotels = new List<HotelModel>();

                foreach(Otel Hotel in Otels.Select(Global.DataBase, Otels.TableName, $"{Otels.Resort} = {Resort.ID}"))
                    Hotels.Add(new HotelModel(Hotel));

                this.ViewBag.ResortHotels = Hotels;
            }
            this.ViewBag.Item = Item;
            return this.View();
        }
    }
}