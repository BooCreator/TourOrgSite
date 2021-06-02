using System;
using System.Collections.Generic;
using System.Web.Mvc;

using TourSnapModels.Models.Data;
using TourSnapModels.Models.DataBase;

using TourSnapProjects.Models.PublicModels;

namespace TourSnapProjects.Controllers
{
    public class ToursController : Controller
    {
        // GET: Tours
        // Страница конкретного тура
        public ActionResult Get(Int32 id = -1)
        {
            this.LoadUserData();
            this.LoadMainMenu();
            TourModel Item = null;
            // Страница конкретного тура
            Tour ToursItems = Tours.SelectFirst(Global.DataBase, Tours.TableName, $"{Tours.ID} = {id}");
            if(ToursItems != null)
            {
                Item = new TourModel(ToursItems);
                // получаем фотографии тура, начиная от отелей
                List<string> Photos = new List<string>();
                Otel Hotel = Otels.SelectFirst(Global.DataBase, Otels.TableName, $"{Otels.ID} = {ToursItems.Otel}");
                if(Hotel != null)
                {
                    Photos = Hotel.Photos;
                    for(int i = 0; i < Photos.Count; i++)
                        Photos[i] = "hotels/" + Photos[i];

                    // и заканчивая курортами, связанными с туром (отелем)
                    var Resort = Resorts.SelectFirst(Global.DataBase, Resorts.TableName, $"{Resorts.ID} = {Hotel.Resort}");
                    if(Resort != null)
                        foreach(var Photo in Resort.Photos)
                            Photos.Add("resorts/" + Photo);
                }

                this.ViewBag.TourPhotos = Photos;
            }
            this.ViewBag.Item = Item;
            return this.View();
        }
    }
}