using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;

using TourSnapModels.Models.Data;
using TourSnapModels.Models.DataBase;

using TourSnapProjects.Models.Find;
using TourSnapProjects.Models.PublicModels;

using TourSnapDataBase.SelectArgs;

using ResortsTable = TourSnapModels.Models.DataBase.Resorts;
using ToursTable = TourSnapModels.Models.DataBase.Tours;

namespace TourSnapProjects.Controllers
{
    public class HomeController : Controller
    {
        // главная страница сайта
        public ActionResult Index()
        {
            this.LoadUserData();
            this.LoadMainMenu();
            this.ViewBag.MenuActivePage = 0;
            // включаем слайдер
            this.ViewBag.Slider = true;
            // загружаем список стран для отображения
            List<MainCountryModel> ListCountries = new List<MainCountryModel>();
            foreach(Country Country in Countries.Select(Global.DataBase, Countries.TableName))
                ListCountries.Add(new MainCountryModel(Country));
            // загружаем список горящих туров для отображения
            List<FireTourModel> ListFireTours = new List<FireTourModel>();
            List<FireTour> FireTourModels = FireTours.Select(Global.DataBase, FireTours.TableName, Args: new List<ISelectArgs>() { new TOP(12) });
            foreach(FireTour Item in FireTourModels)
                ListFireTours.Add(new FireTourModel(Item));

            // утсанавливаем тип меню поиска как "Главная"
            this.ViewBag.MenuType = MenuTypes.Main;
            this.ViewBag.MainCountries = ListCountries;
            this.ViewBag.MainFireTours = ListFireTours;

            // загружаем отели для поиска
            this.ViewBag.Hotels = Otels.Select(Global.DataBase, Otels.TableName);
            // загружаем страны для поиска
            this.ViewBag.Countries = Countries.Select(Global.DataBase, Countries.TableName);
            // загружаем курорты для поиска
            this.ViewBag.Resorts = ResortsTable.Select(Global.DataBase, ResortsTable.TableName);
            // загружаем категории отелей для поиска
            this.ViewBag.Categories = OtelCategories.Select(Global.DataBase, OtelCategories.TableName);
            // загружаем категории питания для поиска
            this.ViewBag.Eatings = OtelEatings.Select(Global.DataBase, OtelEatings.TableName);

            return this.View();
        }
        // страница профиля
        public ActionResult Profile()
        {
            this.LoadUserData();
            this.LoadMainMenu();
            return this.View();
        }
        // страница контактов
        public ActionResult Contacts()
        {
            this.LoadUserData();
            this.LoadMainMenu();
            this.ViewBag.MenuActivePage = 14;
            return this.View();
        }
        // страница "Партнёрам и организациям"
        public ActionResult Partners(Int32 ID = 1)
        {
            this.LoadUserData();
            this.LoadMainMenu();
            // устанавливаем активный пункт главного меню
            // где ID - подпункт меню "Партнёрам и организациям"
            this.ViewBag.MenuActivePage = 6 + ID;
            this.ViewBag.ContentID = ID;
            this.ViewBag.PartnersMenu = null;
            return this.View();
        }

        // страница горящих туров
        public ActionResult Actions(Int32 Page = 0, String JsonMessage = "")
        {
            // максимальное колчиество элементов на странице
            int MaxItems = 8;

            this.LoadUserData();
            this.LoadMainMenu();
            this.ViewBag.MenuActivePage = 1;
            // загружаем страны для поиска
            this.ViewBag.Countries = Countries.Select(Global.DataBase, Countries.TableName);
            // загружаем курорты для поиска
            this.ViewBag.Resorts = ResortsTable.Select(Global.DataBase, ResortsTable.TableName);
            // загружаем категории отелей для поиска
            this.ViewBag.Categories = OtelCategories.Select(Global.DataBase, OtelCategories.TableName);
            // загружаем категории питания для поиска
            this.ViewBag.Eatings = OtelEatings.Select(Global.DataBase, OtelEatings.TableName);

            List<FireTourModel> Items = new List<FireTourModel>();
            // генерируем условие для выборки
            StringBuilder Where = new StringBuilder();
            if(JsonMessage?.Length > 0)
            {
                Where.Append("1 = 1");
                var model = JsonConvert.DeserializeObject<MainTourFindModel>(JsonMessage);
                // если указана максимальная стоимость
                if(model.MaxPrice > 0)
                    Where.Append($" and {ToursTable.Price} < {(model.MaxPrice + 1)}");
                // если указаны данные отеля и курорт
                if(model.Category > -1 && model.Eating > -1 && model.Resort > -1)
                {
                    Where.Append($" and {ToursTable.Otel} in (select {Otels.ID} from {Otels.TableName} where {Otels.Category} = {model.Category} and {Otels.Eating} = {model.Eating} and {Otels.Resort} = {model.Resort})");
                } else
                {
                    // если указана категория отеля
                    if(model.Category > -1)
                        Where.Append($" and {ToursTable.Otel} in (select {Otels.ID} from {Otels.TableName} where {Otels.Category} = {model.Category})");
                    // если указана категория питания
                    if(model.Eating > -1)
                        Where.Append($" and {ToursTable.Otel} in (select {Otels.ID} from {Otels.TableName} where {Otels.Eating} = {model.Eating})");
                    // если указан курорт
                    if(model.Resort > -1)
                        Where.Append($" and {ToursTable.Otel} in (select {Otels.ID} from {Otels.TableName} where {Otels.Resort} = {model.Resort})");
                }
                // если курорт не указан, но указана страна
                if(model.Resort < 0 && model.Country > -1)
                {
                    Where.Append($" and {ToursTable.Otel} in (select {Otels.ID} from {Otels.TableName} where {Otels.Resort} in " +
                        $"(select {ResortsTable.ID} from {ResortsTable.TableName} where {ResortsTable.Country} = {model.Country}))");
                    // заменяем список курортов на список доступных курортов
                    this.ViewBag.Resorts = ResortsTable.Select(Global.DataBase, ResortsTable.TableName, $"{ResortsTable.Country} = {model.Country}");
                }
                this.ViewBag.MainTourFindModel = model;
            }

            // выбираем подходящие горящие туры
            var FireTourModels = FireTours.Select(Global.DataBase, FireTours.TableName);
            // выбираем только те, которые должны оборажаться на странице стр 1: 0 - 5, стр 2: 6-9
            for(int i = Page * MaxItems; i < FireTourModels.Count && i < Page * MaxItems + MaxItems; i++)
            {
                var Tour = ToursTable.SelectFirst(Global.DataBase, ToursTable.TableName,$"{Tours.ID} = {FireTourModels[i].Tour} and {Where}" );
                if(Where.Length == 0 || Tour != null)
                    Items.Add(new FireTourModel(FireTourModels[i], Tour));
            }

            this.ViewBag.Items = Items;
            this.ViewBag.MenuType = MenuTypes.FireTour;
            this.ViewBag.ActivePage = Page;
            this.ViewBag.MaxPage = (int)Math.Ceiling(FireTourModels.Count / (float)MaxItems) - 1;

            return this.View();
        }
        // страница курортов
        public ActionResult Resorts(Int32 Page = 0, String JsonMessage = "")
        {
            int MaxItems = 8;

            this.LoadUserData();
            this.LoadMainMenu();
            this.ViewBag.MenuActivePage = 4;
            // загружаем страны для поиска
            this.ViewBag.Countries = Countries.Select(Global.DataBase, Countries.TableName);

            List<ResortModel> Items = new List<ResortModel>();
            // генерируем условие для выборки
            StringBuilder Where = new StringBuilder();
            if(JsonMessage?.Length > 0)
            {
                ResortFindModel model = JsonConvert.DeserializeObject<ResortFindModel>(JsonMessage);
                // если указана страна
                if(model.Country > -1)
                    Where.Append($"{ResortsTable.Country} = {model.Country}");
                this.ViewBag.ResortFindModel = model;
            }

            // выбираем подходящие курорты
            List<Resort> Resorts = ResortsTable.Select(Global.DataBase, ResortsTable.TableName, Where.ToString());
            // выбираем только те, которые должны оборажаться на странице стр 1: 0 - 5, стр 2: 6-9
            for(int i = Page * MaxItems; i < Resorts.Count && i < Page * MaxItems + MaxItems; i++)
                Items.Add(new ResortModel(Resorts[i]));

            this.ViewBag.Items = Items;
            this.ViewBag.MenuType = MenuTypes.Resort;
            this.ViewBag.ActivePage = Page;
            this.ViewBag.MaxPage = (int)Math.Ceiling(Resorts.Count / (float)MaxItems) - 1;
            return this.View();
        }
        // страница отелей
        public ActionResult Hotels(Int32 Page = 0, String JsonMessage = "")
        {
            int MaxItems = 8;

            this.LoadUserData();
            this.LoadMainMenu();
            this.ViewBag.MenuActivePage = 5;
            // загружаем страны для поиска
            this.ViewBag.Countries = Countries.Select(Global.DataBase, Countries.TableName);
            // загружаем курорты для поиска
            this.ViewBag.Resorts = ResortsTable.Select(Global.DataBase, ResortsTable.TableName);
            // загружаем категории отелей для поиска
            this.ViewBag.Categories = OtelCategories.Select(Global.DataBase, OtelCategories.TableName);
            // загружаем категории питания для поиска
            this.ViewBag.Eatings = OtelEatings.Select(Global.DataBase, OtelEatings.TableName);

            List<HotelModel> Items = new List<HotelModel>();
            // генерируем условие для выборки
            StringBuilder Where = new StringBuilder();
            if(JsonMessage?.Length > 0)
            {
                Where.Append("1 = 1");
                var model = JsonConvert.DeserializeObject<MainTourFindModel>(JsonMessage);
                // если указана категория отеля
                if(model.Category > -1)
                    Where.Append($" and {Otels.Category} = {model.Category}");
                // есл иуказана категория питания
                if(model.Eating > -1)
                    Where.Append($" and {Otels.Eating} = {model.Eating}");
                // есл иуказана максимальная цена
                if(model.MaxPrice > 0)
                    Where.Append($" and {Otels.Price} < {(model.MaxPrice + 1)}");
                // если указан курорт
                if(model.Resort > -1)
                    Where.Append($" and {Otels.Resort} = {model.Resort}");
                else
                // если указана страна
                if(model.Country > -1)
                {
                    Where.Append($" and {Otels.Resort} in (select {ResortsTable.ID} from {ResortsTable.TableName} where {ResortsTable.Country} = {model.Country})");
                    // заменяем список курортов на список доступных курортов
                    this.ViewBag.Resorts = ResortsTable.Select(Global.DataBase, ResortsTable.TableName, $"{ResortsTable.Country} = {model.Country}");
                }
                this.ViewBag.MainTourFindModel = model;
            }

            // выбираем подходящие отели
            var Hotels = Otels.Select(Global.DataBase, Otels.TableName, Where.ToString());
            // выбираем только те, которые должны оборажаться на странице стр 1: 0 - 5, стр 2: 6-9
            for(int i = Page * MaxItems; i < Hotels.Count && i < Page * MaxItems + MaxItems; i++)
                Items.Add(new HotelModel(Hotels[i]));

            this.ViewBag.Items = Items;
            this.ViewBag.MenuType = MenuTypes.Hotel;
            this.ViewBag.ActivePage = Page;
            this.ViewBag.MaxPage = (int)Math.Ceiling(Hotels.Count / (float)MaxItems) - 1;

            return this.View();
        }
        // главная страница поиска
        public ActionResult Find(Int32 Page = 0, String JsonMessage = "")
        {
            int MaxItems = 6;

            this.LoadUserData();
            this.LoadMainMenu();
            this.ViewBag.MenuActivePage = 3;
            // загружаем отели для поиска
            this.ViewBag.Hotels = Otels.Select(Global.DataBase, Otels.TableName);
            // загружаем страны для поиска
            this.ViewBag.Countries = Countries.Select(Global.DataBase, Countries.TableName);
            // загружаем категории отелей для поиска
            this.ViewBag.Resorts = ResortsTable.Select(Global.DataBase, ResortsTable.TableName);
            // загружаем категории отелей для поиска
            this.ViewBag.Categories = OtelCategories.Select(Global.DataBase, OtelCategories.TableName);
            // загружаем категории питания для поиска
            this.ViewBag.Eatings = OtelEatings.Select(Global.DataBase, OtelEatings.TableName);

            List<TourModel> Items = new List<TourModel>();
            // генерируем условие для выборки
            StringBuilder Where = new StringBuilder();
            if(JsonMessage?.Length > 0)
            {
                Where.Append("1 = 1");
                var model = JsonConvert.DeserializeObject<MainTourFindModel>(JsonMessage);
                // если указана максимальная стоимость
                if(model.MaxPrice > 0)
                    Where.Append($" and {ToursTable.Price} < {(model.MaxPrice + 1)}");
                // есл иуказана отель
                if(model.Hotel > -1)
                {
                    Where.Append($" and {ToursTable.Otel} = {model.Hotel}");
                } else
                {
                    // если указаны данные отеля и курорт
                    if(model.Category > -1 && model.Eating > -1 && model.Resort > -1)
                    {
                        Where.Append($" and {ToursTable.Otel} in (select {Otels.ID} from {Otels.TableName} where {Otels.Category} = {model.Category} and {Otels.Eating} = {model.Eating} and {Otels.Resort} = {model.Resort})");
                    } else
                    {
                        // если укзаана категория отеля
                        if(model.Category > -1)
                            Where.Append($" and {ToursTable.Otel} in (select {Otels.ID} from {Otels.TableName} where {Otels.Category} = {model.Category})");
                        // если указана категория питания
                        if(model.Eating > -1)
                            Where.Append($" and {ToursTable.Otel} in (select {Otels.ID} from {Otels.TableName} where {Otels.Eating} = {model.Eating})");
                        // если указан курорт
                        if(model.Resort > -1)
                            Where.Append($" and {ToursTable.Otel} in (select {Otels.ID} from {Otels.TableName} where {Otels.Resort} = {model.Resort})");
                    }
                    // если курорт не указан, но указана страна
                    if(model.Resort < 0 && model.Country > -1)
                    {
                        Where.Append($" and {ToursTable.Otel} in (select {Otels.ID} from {Otels.TableName} where {Otels.Resort} in " +
                            $"(select {ResortsTable.ID} from {ResortsTable.TableName} where {ResortsTable.Country} = {model.Country}))");
                        // заменяем список курортов на список доступных курортов
                        this.ViewBag.Resorts = ResortsTable.Select(Global.DataBase, ResortsTable.TableName, $"{ResortsTable.Country} = {model.Category}");
                    }
                }
                this.ViewBag.MainTourFindModel = model;
                // если указана или категория отеля, или категория питания, или курорт
                if(model.Category > -1 || model.Eating > -1 || model.Resort > -1)
                {
                    List<Otel> ResOtels = new List<Otel>();
                    // получаем данные для поиска
                    HotelSimpleFindModel ModelToOtel = new HotelSimpleFindModel() { Category = model.Category, Eating = model.Eating, Resort = model.Resort };
                    HotelsListSimpleModel OtelsModel = JsonConvert.DeserializeObject<HotelsListSimpleModel>(JsonConvert.SerializeObject(ModelToOtel));
                    foreach(int Item in OtelsModel.IDs)
                    {
                        ResOtels.Add(Otels.SelectFirst(Global.DataBase, Otels.TableName, $"{Otels.ID} = {Item}"));
                    }
                    // заменяем отелей на список доступных отелей
                    this.ViewBag.Hotels = ResOtels;
                }
            }

            // выбираем подходящие заявки
            var Tours = ToursTable.Select(Global.DataBase, ToursTable.TableName, Where.ToString());
            // выбираем только те, которые должны оборажаться на странице стр 1: 0 - 5, стр 2: 6-9
            for(int i = Page * MaxItems; i < Tours.Count && i < Page * MaxItems + MaxItems; i++)
                Items.Add(new TourModel(Tours[i]));

            this.ViewBag.Items = Items;

            this.ViewBag.MenuType = MenuTypes.Main;
            this.ViewBag.ActivePage = Page;
            this.ViewBag.MaxPage = (int)Math.Ceiling(Tours.Count / (float)MaxItems) - 1;
            return this.View();
        }

        // -------------------
        // получить данные о доступных курортах (для поиска)
        public JsonResult GetResorts(String Value)
        {
            // получаем необходимые данные о странах
            ResortFindModel model = JsonConvert.DeserializeObject<ResortFindModel>(Value);
            string Where = "";
            // если страна указана
            if(model.Country > -1)
                Where = $"{ResortsTable.Country} = {model.Country}";
            // выбираем подходящий список курортов
            var Items = ResortsTable.Select(Global.DataBase, ResortsTable.TableName, Where);
            ResortsListSimpleModel Result = new ResortsListSimpleModel(Items);
            // преобразуем в строку
            string val = JsonConvert.SerializeObject(Result);
            // возвращаем на страницу
            return this.Json(new { Type = JsonTypes.Object, Text = val });
        }
        // получить данные о доступных отелях (для поиска)
        public JsonResult GetHotels(String Value)
        {
            // получаем необходимые данные
            HotelSimpleFindModel model = JsonConvert.DeserializeObject<HotelSimpleFindModel>(Value);
            // генерируем условие выборки
            StringBuilder Where = new StringBuilder();
            Where.Append("1 = 1");
            // есл иуказан курорт
            if(model.Resort > -1)
                Where.Append($" and {Otels.Resort} = {model.Resort}");
            // если указана категория отеля
            if(model.Category > -1)
                Where.Append($" and {Otels.Category} = {model.Category}");
            // есл иуказана категоряи питания
            if(model.Eating > -1)
                Where.Append($" and {Otels.Eating} = {model.Eating}");
            // выбираем подходящие отели
            var Items = Otels.Select(Global.DataBase, Otels.TableName, Where.ToString());
            HotelsListSimpleModel Result = new HotelsListSimpleModel(Items);
            // преобразуем в строку
            string val = JsonConvert.SerializeObject(Result);
            // возвращаем на страницу
            return this.Json(new { Type = JsonTypes.Object, Text = val });
        }
    }
}