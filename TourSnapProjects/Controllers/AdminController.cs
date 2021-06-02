using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Web.Mvc;

using TourSnapModels.Models.Data;
using TourSnapModels.Models.DataBase;

using TourSnapProjects.Models.Admin;

using CountriesTable = TourSnapModels.Models.DataBase.Countries;
using ToursTable = TourSnapModels.Models.DataBase.Tours;
using FireToursTable = TourSnapModels.Models.DataBase.FireTours;

namespace TourSnapProjects.Controllers
{
    public class AdminController : Controller
    {
        // страница изменения стран
        public ActionResult Countries()
        {
            this.LoadUserData();
            this.LoadMainMenu();
            // получаем данные всех стран
            this.ViewBag.Items = CountriesTable.Select(Global.DataBase, CountriesTable.TableName);
            return this.View();
        }
        // страница изменения курортов
        public ActionResult Curorts()
        {
            this.LoadUserData();
            this.LoadMainMenu();
            // получаем данные всех стран
            this.ViewBag.Countries = CountriesTable.Select(Global.DataBase, CountriesTable.TableName);
            // получаем данные всех курортов
            this.ViewBag.Items = Resorts.Select(Global.DataBase, Resorts.TableName);
            return this.View();
        }
        // страница изменения отелей
        public ActionResult Hotels()
        {
            this.LoadUserData();
            this.LoadMainMenu();
            // получаем данные всех курортов
            this.ViewBag.Resorts = Resorts.Select(Global.DataBase, Resorts.TableName);
            // получаем данные всех категорий отелей
            this.ViewBag.Categories = OtelCategories.Select(Global.DataBase, OtelCategories.TableName);
            // получаем данные всех категорий питания
            this.ViewBag.Eatings = OtelEatings.Select(Global.DataBase, OtelEatings.TableName);
            // получаем данные всех отелей
            this.ViewBag.Items = Otels.Select(Global.DataBase, Otels.TableName);
            return this.View();
        }
        // страница изменения туров
        public ActionResult Tours()
        {
            this.LoadUserData();
            this.LoadMainMenu();
            // получаем данные всех отелей
            this.ViewBag.Otels = Otels.Select(Global.DataBase, Otels.TableName);
            // получаем данные всех туров
            List<Tour> Items = ToursTable.Select(Global.DataBase, ToursTable.TableName);
            // преобразуем все Туры в ДанныеТуров
            List <TourData> Result = new List<TourData>();
            foreach(Tour Item in Items)
            {
                TourData tmp = Item.ToTourData();
                // устанавливаем горящий ли тур
                tmp.IsFire = (FireToursTable.Count(Global.DataBase, FireToursTable.TableName, $"{FireToursTable.Tour} = {Item.ID}") > 0);
                Result.Add(tmp);
            }
            this.ViewBag.Items = Result;
            return this.View();
        }
        // страница изменения горящих туров
        public ActionResult FireTours()
        {
            this.LoadUserData();
            this.LoadMainMenu();
            // получаем данные всех туров
            List<Tour> Items = ToursTable.Select(Global.DataBase, ToursTable.TableName);
            // преобразуем все Туры в ДанныеТуров
            List <TourData> Result = new List<TourData>();
            foreach(Tour Item in Items)
            {
                TourData tmp = Item.ToTourData();
                // устанавливаем горящий ли тур
                tmp.IsFire = (FireToursTable.Count(Global.DataBase, FireToursTable.TableName, $"{FireToursTable.Tour} = {Item.ID}") > 0);
                Result.Add(tmp);
            }
            this.ViewBag.Items = Result;
            return this.View();
        }

        // ---------------- Страны -------------------
        [HttpPost]
        // сохранить
        public JsonResult SaveCountry(String JsonMessage)
        {
            // проверяем что редактирует администратор
            if(Global.GetUserRole(this) == Global.AdminRoleID)
            {
                // получаем объект с данными
                Country model = JsonConvert.DeserializeObject<Country>(JsonMessage);
                // проверяем что он не пустой
                if(model.Name != null && model.Map != null)
                {
                    // формируем поля для изменений
                    Dictionary<string, object> Fields = new Dictionary<string, object>()
                    {
                        { CountriesTable.ID, model.ID },
                        { CountriesTable.Name, model.Name },
                        { CountriesTable.Map, model.Map },
                        { CountriesTable.Photo, model.Photo },
                    };
                    // если объект существует, то изменить
                    if(model.ID > -1)
                    {
                        Fields.Remove(CountriesTable.ID);
                        if(CountriesTable.Update(Global.DataBase, CountriesTable.TableName, Fields, $"{CountriesTable.ID} = {model.ID}"))
                            return this.Json(new { Type = JsonTypes.Reload });
                        else
                        {
                            string Error = CountriesTable.LastError;
                            return this.Json(new { Type = JsonTypes.Text, Text = "Ошибка изменения записи!" });
                        }
                    } else
                    {
                        // если нет - добавить
                        Fields[CountriesTable.ID] = CountriesTable.Max(Global.DataBase, CountriesTable.TableName, CountriesTable.ID) + 1;
                        if(CountriesTable.Insert(Global.DataBase, CountriesTable.TableName, Fields))
                            return this.Json(new { Type = JsonTypes.Reload });
                        else
                        {
                            string Error = CountriesTable.LastError;
                            return this.Json(new { Type = JsonTypes.Text, Text = "Ошибка добавления новой записи!" });
                        }
                    }
                }
                return this.Json(new { Type = JsonTypes.Text, Text = "Ошибка получения данных!" });
            }
            return this.Json(new { Type = JsonTypes.Text, Text = "Доступ запрещён!" });
        }

        [HttpPost]
        // получить данные о одном объекте
        public JsonResult GetCountry(Int32 Value)
        {
            // получаем данные из базы данных
            Country Item = CountriesTable.SelectFirst(Global.DataBase, CountriesTable.TableName, $"{CountriesTable.ID} = {Value}");
            if(Item == null)
                Item = new Country() { ID = -1 };
            return this.Json(new { Type = JsonTypes.Object, Text = JsonConvert.SerializeObject(Item) });
        }

        [HttpPost]
        // удалить
        public JsonResult RemoveCountry(Int32 Value)
        {
            // проверяем что удаляет администратор
            if(Global.GetUserRole(this) == Global.AdminRoleID)
            {
                // удаляем данные из базы данных
                if(CountriesTable.Delete(Global.DataBase, CountriesTable.TableName, $"{CountriesTable.ID} = {Value}"))
                    return this.Json(new { Type = JsonTypes.Reload });
                else
                {
                    string Error = CountriesTable.LastError;
                    return this.Json(new { Type = JsonTypes.Text, Text = "Ошибка удаления записи!" });
                } 
            }
            return this.Json(new { Type = JsonTypes.Text, Text = "Доступ запрещён!" });
        }

        // ---------------- Курорты -------------------
        [HttpPost]
        // сохранить
        public JsonResult SaveResort(String JsonMessage)
        {
            // проверяем что редактирует администратор
            if(Global.GetUserRole(this) == Global.AdminRoleID)
            {
                // получаем объект с данными
                ResortData model = JsonConvert.DeserializeObject<ResortData>(JsonMessage);
                // проверяем что он не пустой
                if(model.Name != null && model.Map != null)
                {
                    // формируем поля для изменений
                    Dictionary<string, object> Fields = new Dictionary<string, object>()
                    {
                        { Resorts.ID, model.ID },
                        { Resorts.Name, model.Name },
                        { Resorts.Country, model.Country },
                        { Resorts.Map, model.Map },
                        { Resorts.Photos, model.Photos },
                        { Resorts.Text, model.Text },
                    };
                    // если объект существует, то изменить
                    if(model.ID > -1)
                    {
                        Fields.Remove(Resorts.ID);
                        if(Resorts.Update(Global.DataBase, Resorts.TableName, Fields, $"{Resorts.ID} = {model.ID}"))
                            return this.Json(new { Type = JsonTypes.Reload });
                        else
                        {
                            string Error = Resorts.LastError;
                            return this.Json(new { Type = JsonTypes.Text, Text = "Ошибка изменения записи!" });
                        }
                    } else
                    {
                        // если нет - добавить
                        Fields[Resorts.ID] = Resorts.Max(Global.DataBase, Resorts.TableName, Resorts.ID) + 1;
                        if(Resorts.Insert(Global.DataBase, Resorts.TableName, Fields))
                            return this.Json(new { Type = JsonTypes.Reload });
                        else
                        {
                            string Error = Resorts.LastError;
                            return this.Json(new { Type = JsonTypes.Text, Text = "Ошибка добавления новой записи!" });
                        }
                    }
                }
                return this.Json(new { Type = JsonTypes.Text, Text = "Ошибка получения данных!" });
            }
            return this.Json(new { Type = JsonTypes.Text, Text = "Доступ запрещён!" });
        }

        [HttpPost]
        // получить данные о одном объекте
        public JsonResult GetResort(Int32 Value)
        {
            // получаем данные из базы данных
            ResortData Item = Resorts.SelectFirst(Global.DataBase, Resorts.TableName, $"{Resorts.ID} = {Value}").ToResortData();
            if(Item == null)
                Item = new ResortData() { ID = -1 };
            return this.Json(new { Type = JsonTypes.Object, Text = JsonConvert.SerializeObject(Item) });
        }

        [HttpPost]
        // удалить
        public JsonResult RemoveResort(Int32 Value)
        {
            // проверяем что удаляет администратор
            if(Global.GetUserRole(this) == Global.AdminRoleID)
            {
                // удаляем данные из базы данных
                if(Resorts.Delete(Global.DataBase, Resorts.TableName, $"{Resorts.ID} = {Value}"))
                    return this.Json(new { Type = JsonTypes.Reload });
                else
                {
                    string Error = Resorts.LastError;
                    return this.Json(new { Type = JsonTypes.Text, Text = "Ошибка удаления записи!" });
                }
            }
            return this.Json(new { Type = JsonTypes.Text, Text = "Доступ запрещён!" });
        }

        // ---------------- Отели -------------------
        [HttpPost]
        // сохранить
        public JsonResult SaveHotel(String JsonMessage)
        {
            // проверяем что редактирует администратор
            if(Global.GetUserRole(this) == Global.AdminRoleID)
            {
                // получаем объект с данными
                OtelData model = JsonConvert.DeserializeObject<OtelData>(JsonMessage);
                // проверяем что он не пустой
                if(model.Name != null)
                {
                    // формируем поля для изменений
                    Dictionary<string, object> Fields = new Dictionary<string, object>()
                    {
                        { Otels.ID, model.ID },
                        { Otels.Name, model.Name },
                        { Otels.Resort, model.Resort },
                        { Otels.Price, model.Price },
                        { Otels.Category, model.Category },
                        { Otels.Eating, model.Eating },
                        { Otels.Photos, model.Photos.Replace(", ", ",") },
                        { Otels.Text, model.Text },
                    };
                    // если объект существует, то изменить
                    if(model.ID > -1)
                    {
                        Fields.Remove(Otels.ID);
                        if(Otels.Update(Global.DataBase, Otels.TableName, Fields, $"{Otels.ID} = {model.ID}"))
                            return this.Json(new { Type = JsonTypes.Reload });
                        else
                        {
                            string Error = Otels.LastError;
                            return this.Json(new { Type = JsonTypes.Text, Text = "Ошибка изменения записи!" });
                        }
                    } else
                    {
                        // если нет - добавить
                        Fields[Otels.ID] = Otels.Max(Global.DataBase, Otels.TableName, Otels.ID) + 1;
                        if(Otels.Insert(Global.DataBase, Otels.TableName, Fields))
                            return this.Json(new { Type = JsonTypes.Reload });
                        else
                        {
                            string Error = Otels.LastError;
                            return this.Json(new { Type = JsonTypes.Text, Text = "Ошибка добавления новой записи!" });
                        }
                    }
                }
                return this.Json(new { Type = JsonTypes.Text, Text = "Ошибка получения данных!" });
            }
            return this.Json(new { Type = JsonTypes.Text, Text = "Доступ запрещён!" });
        }

        [HttpPost]
        // получить данные о одном объекте
        public JsonResult GetHotel(Int32 Value)
        {
            // получаем данные из базы данных
            OtelData Item = Otels.SelectFirst(Global.DataBase, Otels.TableName, $"{Otels.ID} = {Value}").ToOtelData();
            if(Item == null)
                Item = new OtelData() { ID = -1 };
            return this.Json(new { Type = JsonTypes.Object, Text = JsonConvert.SerializeObject(Item) });
        }

        [HttpPost]
        // удалить
        public JsonResult RemoveHotel(Int32 Value)
        {
            // проверяем что удаляет администратор
            if(Global.GetUserRole(this) == Global.AdminRoleID)
            {
                // удаляем данные из базы данных
                if(Otels.Delete(Global.DataBase, Otels.TableName, $"{Otels.ID} = {Value}"))
                    return this.Json(new { Type = JsonTypes.Reload });
                else
                {
                    string Error = Otels.LastError;
                    return this.Json(new { Type = JsonTypes.Text, Text = "Ошибка удаления записи!" });
                }
            }
            return this.Json(new { Type = JsonTypes.Text, Text = "Доступ запрещён!" });
        }

        // ---------------- Туры -------------------
        [HttpPost]
        // сохранить
        public JsonResult SaveTour(String JsonMessage)
        {
            // проверяем что редактирует администратор
            if(Global.GetUserRole(this) == Global.AdminRoleID)
            {
                // получаем объект с данными
                Tour model = JsonConvert.DeserializeObject<Tour>(JsonMessage);
                // проверяем что он не пустой
                if(model.Otel > -1)
                {
                    // формируем поля для изменений
                    Dictionary<string, object> Fields = new Dictionary<string, object>()
                    {
                        { ToursTable.ID, model.ID },
                        { ToursTable.Otel, model.Otel },
                        { ToursTable.Days, model.Days },
                        { ToursTable.Date, model.Date },
                        { ToursTable.Price, model.Price },
                        { ToursTable.Text, model.Text },
                        { ToursTable.Title, model.Title },
                        { ToursTable.Photo, model.Photo.Replace(", ", ",") },
                    };
                    // если объект существует, то изменить
                    if(model.ID > -1)
                    {
                        Fields.Remove(ToursTable.ID);
                        if(ToursTable.Update(Global.DataBase, ToursTable.TableName, Fields, $"{ToursTable.ID} = {model.ID}"))
                            return this.Json(new { Type = JsonTypes.Reload });
                        else
                        {
                            string Error = ToursTable.LastError;
                            return this.Json(new { Type = JsonTypes.Text, Text = "Ошибка изменения записи!" });
                        }
                    } else
                    {
                        // если нет - добавить
                        Fields[ToursTable.ID] = ToursTable.Max(Global.DataBase, ToursTable.TableName, ToursTable.ID) + 1;
                        if(ToursTable.Insert(Global.DataBase, ToursTable.TableName, Fields))
                            return this.Json(new { Type = JsonTypes.Reload });
                        else
                        {
                            string Error = ToursTable.LastError;
                            return this.Json(new { Type = JsonTypes.Text, Text = "Ошибка добавления новой записи!" });
                        }
                    }
                }
                return this.Json(new { Type = JsonTypes.Text, Text = "Ошибка получения данных!" });
            }
            return this.Json(new { Type = JsonTypes.Text, Text = "Доступ запрещён!" });
        }

        [HttpPost]
        // получить данные о одном объекте
        public JsonResult GetTour(Int32 Value)
        {
            // получаем данные из базы данных
            TourData Item = ToursTable.SelectFirst(Global.DataBase, ToursTable.TableName, $"{ToursTable.ID} = {Value}").ToTourData();
            if(Item == null)
                Item = new TourData() { ID = -1, Date = DateTime.Now.ToString("yyyy-MM-dd") };
            return this.Json(new { Type = JsonTypes.Object, Text = JsonConvert.SerializeObject(Item) });
        }

        [HttpPost]
        // удалить
        public JsonResult RemoveTour(Int32 Value)
        {
            // проверяем что удаляет администратор
            if(Global.GetUserRole(this) == Global.AdminRoleID)
            {
                // удаляем данные из базы данных
                if(ToursTable.Delete(Global.DataBase, ToursTable.TableName, $"{ToursTable.ID} = {Value}"))
                {
                    // удаляем тур из горящих туров
                    FireToursTable.Delete(Global.DataBase, ToursTable.TableName, $"{FireToursTable.Tour} = {Value}");
                    return this.Json(new { Type = JsonTypes.Reload });
                } else
                {
                    string Error = ToursTable.LastError;
                    return this.Json(new { Type = JsonTypes.Text, Text = "Ошибка удаления записи!" });
                }
            }
            return this.Json(new { Type = JsonTypes.Text, Text = "Доступ запрещён!" });
        }

        // ---------------- Горящие туры -------------------
        [HttpPost]
        // сохранить
        public JsonResult SaveFireTour(String JsonMessage)
        {
            // проверяем что редактирует администратор
            if(Global.GetUserRole(this) == Global.AdminRoleID)
            {
                // получаем объект с данными
                FireTourData model = JsonConvert.DeserializeObject<FireTourData>(JsonMessage);
                // проверяем что он не пустой
                if(model != null)
                {
                    // формируем поля для изменений
                    Dictionary<string, object> Fields = new Dictionary<string, object>()
                    {
                        { FireToursTable.Tour, model.TourID },
                        { FireToursTable.StartDate, model.StartDate },
                        { FireToursTable.EndDate, model.EndDate },
                        { FireToursTable.Price, model.Price },
                    };
                    // если объект существует, то изменить
                    if(model.Tour > -1)
                    {
                        if(FireToursTable.Update(Global.DataBase, FireToursTable.TableName, Fields, $"{FireToursTable.Tour} = {model.Tour}"))
                            return this.Json(new { Type = JsonTypes.Reload });
                        else
                        {
                            string Error = FireToursTable.LastError;
                            return this.Json(new { Type = JsonTypes.Text, Text = "Ошибка изменения записи!" });
                        }
                    } else
                    {
                        // если нет - добавить
                        if(FireToursTable.Insert(Global.DataBase, FireToursTable.TableName, Fields))
                            return this.Json(new { Type = JsonTypes.Reload });
                        else
                        {
                            string Error = FireToursTable.LastError;
                            return this.Json(new { Type = JsonTypes.Text, Text = "Ошибка добавления новой записи!" });
                        }
                    }
                }
                return this.Json(new { Type = JsonTypes.Text, Text = "Ошибка получения данных!" });
            }
            return this.Json(new { Type = JsonTypes.Text, Text = "Доступ запрещён!" });
        }

        [HttpPost]
        // получить данные о одном объекте
        public JsonResult GetFireTour(Int32 Value)
        {
            // получаем данные из базы данных
            FireTourData Item = FireToursTable.SelectFirst(Global.DataBase, FireToursTable.TableName, $"{FireToursTable.Tour} = {Value}").ToFireTourData(Value);
            if(Item == null)
                Item = new FireTourData() { Tour = -1, StartDate = DateTime.Now.ToString("yyyy-MM-dd"), EndDate = DateTime.Now.ToString("yyyy-MM-dd") };
            return this.Json(new { Type = JsonTypes.Object, Text = JsonConvert.SerializeObject(Item) });
        }
       
        [HttpPost]
        // удалить
        public JsonResult RemoveFireTour(Int32 Value)
        {
            // проверяем что удаляет администратор
            if(Global.GetUserRole(this) == Global.AdminRoleID)
            {
                // удаляем данные из базы данных
                if(FireToursTable.Delete(Global.DataBase, FireToursTable.TableName, $"{FireToursTable.Tour} = {Value}"))
                    return this.Json(new { Type = JsonTypes.Reload });
                else
                {
                    string Error = FireToursTable.LastError;
                    return this.Json(new { Type = JsonTypes.Text, Text = "Ошибка удаления записи!" });
                }
            }
            return this.Json(new { Type = JsonTypes.Text, Text = "Доступ запрещён!" });
        }
    
    }
}