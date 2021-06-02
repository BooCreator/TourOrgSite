using System;
using System.Text;
using System.Web.Mvc;
using System.Web.Configuration;
using System.Collections.Generic;

using TourSnapDataBase.DataBase;
using TourSnapModels.Models.Data;
using TourSnapModels.Models.DataBase;
using TourSnapProjects.Models.Admin;
using TourSnapProjects.Models;

namespace TourSnapProjects
{
    /// <summary>
    /// Типы возвращаемой JSON-строки
    /// </summary>
    public static class JsonTypes
    {
        /// <summary>
        /// Текст
        /// </summary>
        public static String Text => "Text";
        /// <summary>
        /// Перенаправление
        /// </summary>
        public static String Redirect => "Redirect";
        /// <summary>
        /// Перезагрузка
        /// </summary>
        public static String Reload => "Reload";
        /// <summary>
        /// Объект
        /// </summary>
        public static String Object => "Object";
        /// <summary>
        /// Ничего
        /// </summary>
        public static String Nothing => "None";
    }
    /// <summary>
    /// Типы меню поиска
    /// </summary>
    public enum MenuTypes
    {
        None = -1,
        Resort,
        Hotel,
        FireTour,
        Main,
        Manager
    }
    /// <summary>
    /// Класс расширения
    /// </summary>
    public static class ExtController
    {
        /// <summary>
        /// Загрузка данных о пользователе и подготовка их для отправки на страницы сайта
        /// </summary>
        /// <param name="Controller"></param>
        public static void LoadUserData(this Controller Controller)
        {
            Controller.ViewBag.UserID = Global.GetUserID(Controller);
            Controller.ViewBag.UserRole = Global.GetUserRole(Controller);
            var User = Users.SelectFirst(Global.DataBase, Users.TableName, $"{Users.ID} = {Global.GetUserID(Controller)}");
            Controller.ViewBag.User = User;
        }
        /// <summary>
        /// Загрузка главного меню сайта
        /// </summary>
        /// <param name="Controller"></param>
        public static void LoadMainMenu(this Controller Controller)
        {
            List<MenuPage> Result = new List<MenuPage>();
            List<MenuItem> Items = Menu.Select(Global.DataBase, Menu.TableName, $"{Menu.Upper} is NULL");
            foreach(MenuItem Item in Items)
                Result.Add(new MenuPage(Item));
            Controller.ViewBag.MainMenu = Result;
        }
        /// <summary>
        /// Метод преобразования из класса Курорты в класса ДанныеКурорта для вывода на страницу
        /// </summary>
        /// <param name="Item"></param>
        /// <returns></returns>
        public static ResortData ToResortData(this Resort Item)
        {
            ResortData Result = new ResortData() { ID = -1 };
            if(Item != null)
            {
                Result.ID = Item.ID;
                Result.Name = Item.Name;
                Result.Country = Item.Country;
                Result.Map = Item.Map;
                Result.Photos = Item.Photos.ToOneString(", ");
                Result.Text = Item.Text;
            }
            return Result;
        }
        /// <summary>
        /// Метод преобразования из класса Отели в класса ДанныеОтеля для вывода на страницу
        /// </summary>
        /// <param name="Item"></param>
        /// <returns></returns>
        public static OtelData ToOtelData(this Otel Item)
        {
            OtelData Result = new OtelData() { ID = -1 };
            if(Item != null)
            {
                Result.ID = Item.ID;
                Result.Name = Item.Name;
                Result.Resort = Item.Resort;
                Result.Price = Item.Price;
                Result.Category = Item.Category;
                Result.Eating = Item.Eating;
                Result.Photos = Item.Photos.ToOneString(", ");
                Result.Text = Item.Text;
            }
            return Result;
        }
        /// <summary>
        /// Метод преобразования из класса Туры в класса ДанныеТура для вывода на страницу
        /// </summary>
        /// <param name="Item"></param>
        /// <returns></returns>
        public static TourData ToTourData(this Tour Item)
        {
            TourData Result = new TourData() { ID = -1, Date = DateTime.Now.ToString("yyyy-MM-dd") };
            if(Item != null)
            {
                Result.ID = Item.ID;
                Result.Otel = Item.Otel;
                Result.Days = Item.Days;
                Result.Date = Item.Date.ToString("yyyy-MM-dd");
                Result.Price = Item.Price;
                Result.Text = Item.Text;
                Result.Title = Item.Title;
                Result.Photo = Item.Photo;
            }
            return Result;
        }
        /// <summary>
        /// Метод преобразования из класса Горящие туры в класса ДанныеГорящихтуров для вывода на страницу
        /// </summary>
        /// <param name="Item"></param>
        /// <returns></returns>
        public static FireTourData ToFireTourData(this FireTour Item, Int32 TourID)
        {
            FireTourData Result = new FireTourData() { TourID = TourID, Tour = -1, StartDate = DateTime.Now.ToString("yyyy-MM-dd"), EndDate = DateTime.Now.ToString("yyyy-MM-dd"), };
            if(Item != null)
            {
                Result.Tour = Item.Tour;
                Result.StartDate = Item.StartDate.ToString("yyyy-MM-dd");
                Result.EndDate = Item.EndDate.ToString("yyyy-MM-dd");
                Result.Price = Item.Price;
            }
            return Result;
        }
        /// <summary>
        /// Метод преобразования списка строк в одну строку
        /// </summary>
        /// <param name="Item"></param>
        /// <returns></returns>
        public static String ToOneString(this List<String> Items, String Splitter)
        {
            StringBuilder Result = new StringBuilder();
            foreach(String Item in Items)
            {
                Result.Append(Item);
                Result.Append(Splitter);
            }
            if(Items.Count > 0)
                Result.Remove(Result.Length - Splitter.Length, Splitter.Length);
            return Result.ToString();
        }
    
    }
    /// <summary>
    /// Данные вошедшего пользователя
    /// </summary>
    public class UserData
    {
        public UserData(Int32 ID, String Name, Int32 Role)
        {
            this.UserID = ID;
            this.UserName = Name;
            this.UserRole = Role;
            this.Date = DateTime.Now;
        }
        public Int32 UserID { get; set; } = -1;
        public Int32 UserRole { get; set; } = -1;
        public String UserName { get; set; }
        /// <summary>
        /// Дата и время входа в систему
        /// </summary>
        public DateTime Date { get; set; }
        public void Update()
            => this.Date = DateTime.Now;
    }
    /// <summary>
    /// Глобальный класс статических данных
    /// </summary>
    public static class Global
    {
        /// <summary>
        /// Редактор базы данных
        /// </summary>
        public static MSSQLEngine DataBase { get; } 
            = new MSSQLEngine(WebConfigurationManager.AppSettings["DataBase"]);
        /// <summary>
        /// Массив вошедших пользователей
        /// </summary>
        public static List<UserData> LoginedUser { get; }
            = new List<UserData>();
        /// <summary>
        /// Получить ID пользователя
        /// </summary>
        /// <param name="Controller"></param>
        /// <returns></returns>
        public static Int32 GetUserID(Controller Controller)
        {
            int result = -1;
            if (Controller.User.Identity.IsAuthenticated)
                result = Global.GetLoginedUserID(Controller.User.Identity.Name);
            return result;
        }
        /// <summary>
        /// Получить роль пользователя
        /// </summary>
        /// <param name="Controller"></param>
        /// <returns></returns>
        public static Int32 GetUserRole(Controller Controller)
        {
            int result = -254;
            if (Controller.User.Identity.IsAuthenticated)
                result = Global.GetLoginedUserRole(Controller.User.Identity.Name);
            return result;
        }
        /// <summary>
        /// Получить ID авторизованного пользователя по данным из контроллера
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        private static Int32 GetLoginedUserID(String UserID)
        {
            return 
                Int32.TryParse(UserID, out int res) ? res : -1;
        }
        /// <summary>
        /// Получить Роль активного пользователя по данным из контроллера
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        private static Int32 GetLoginedUserRole(String UserID)
        {
            var User = Users.SelectFirst(Global.DataBase, Users.TableName, $"[{Users.ID}] = {UserID}");
            return
                (User != null) ? User.Role : -1;
        }
        /// <summary>
        /// Идентификатор роли администратора
        /// </summary>
        public static Int32 AdminRoleID { get; } = -8;
        /// <summary>
        /// Идентификатор роти менеджера
        /// </summary>
        public static Int32 ManagerRoleID { get; } = -7;

        // Функция генерации SHA1-хэша для строки.
        public static String SHA1(String str)
            => Encoding.UTF8.GetString(getHash(Encoding.UTF8.GetBytes(str))).Replace("'", "\"");
        // Функция генерации SHA1-хэша для массива байт
        private static byte[] getHash(byte[] bytes)
        {
            using (var sha = System.Security.Cryptography.SHA1.Create())
            {
                byte[] hash = sha.ComputeHash(sha.ComputeHash(bytes));
                return hash;
            }
        }
    
    }

}