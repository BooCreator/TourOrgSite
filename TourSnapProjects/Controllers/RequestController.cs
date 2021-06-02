using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;

using TourSnapModels.Models.Data;
using TourSnapModels.Models.DataBase;

using TourSnapProjects.Models.Find;
using TourSnapProjects.Models.Request;

using TourSnapDataBase.SelectArgs;

namespace TourSnapProjects.Controllers
{
    public class RequestController : Controller
    {
        // страница отображения своих заявок
        public ActionResult View(Int32 Page = 0)
        {
            // максимальное количество элементов на странице
            int MaxItems = 12;

            this.ViewBag.MaxPage = 0;

            this.LoadUserData();
            this.LoadMainMenu();
            // получае ИД пользователя
            int UserID = Global.GetUserID(this);

            // получаем все заявки пользователя
            List<RequestModel> Result = new List<RequestModel>();
            if(UserID > -1)
            {
                var Items = Requests.Select(Global.DataBase, Requests.TableName, $"[{Requests.User}] = {UserID}");
                // выбираем только те, которые должны оборажаться на странице стр 1: 0 - 11, стр 2: 12-23
                for(int i = Page * MaxItems; i < Page * MaxItems + MaxItems && i < Items.Count; i++)
                    Result.Add(new RequestModel(Items[i]));
                this.ViewBag.MaxPage = (int)(Items.Count / (float)MaxItems + 0.5) - 1;
            }
            this.ViewBag.RequestItems = Result;
            this.ViewBag.ActivePage = Page;
            return base.View();
        }
        // страница редактирования заявок
        public ActionResult Manage(Int32 Page = 0, String JsonMessage = "")
        {
            // максимальное количество элементов на странице
            int MaxItems = 12;

            this.ViewBag.MaxPage = 0;

            this.LoadUserData();
            this.LoadMainMenu();
            int UserID = Global.GetUserID(this);
            List<RequestModel> Result = new List<RequestModel>();
            // если на страницу зашёл менеджер
            if(UserID > -1 && Global.GetUserRole(this) == Global.ManagerRoleID)
            {
                // загружаем отели для фильтра
                this.ViewBag.Hotels = Otels.Select(Global.DataBase, Otels.TableName);
                // загружаем страны для фильтра
                this.ViewBag.Countries = Countries.Select(Global.DataBase, Countries.TableName);
                // загружаем курорты для фильтра
                this.ViewBag.Resorts = Resorts.Select(Global.DataBase, Resorts.TableName);
                
                // загружаем список состояний заявок
                this.ViewBag.RequestStates = RequestStates.Select(Global.DataBase, RequestStates.TableName);

                // генерируем условие для выборки
                StringBuilder Where = new StringBuilder();
                if(JsonMessage?.Length > 0)
                {
                    Where.Append("1 = 1");
                    var model = JsonConvert.DeserializeObject<MainTourFindModel>(JsonMessage);
                    // если выбран отель
                    if(model.Hotel > -1)
                    {
                        Where.Append($" and {Requests.Tour} in " +
                            $"(select {Tours.ID} from {Tours.TableName} where {Tours.Otel} = {model.Hotel})");
                    } else
                    {
                        // если выбран курорт
                        if(model.Resort > -1)
                        {
                            Where.Append($" and {Requests.Tour} in " +
                                $"(select {Tours.ID} from {Tours.TableName} where {Tours.Otel} in " +
                                $"(select {Otels.ID} from {Otels.TableName} where {Otels.Resort} = {model.Resort}))");
                            // заменяем список отелей на список доступных отелей
                            this.ViewBag.Hotels = Otels.Select(Global.DataBase, Otels.TableName, $"{Otels.Resort} = {model.Resort}");
                        } else
                        {
                            // если выбрана страна
                            if(model.Country > -1)
                            {
                                Where.Append($" and {Requests.Tour} in " +
                                    $"(select {Tours.ID} from {Tours.TableName} where {Tours.Otel} in " +
                                    $"(select {Otels.ID} from {Otels.TableName} where {Otels.Resort} in " +
                                    $"(select {Resorts.ID} from {Resorts.TableName} where {Resorts.Country} = {model.Country})))");
                                // заменяем список курортов на список доступных курортов
                                this.ViewBag.Resorts = Resorts.Select(Global.DataBase, Resorts.TableName, $"{Resorts.Country} = {model.Category}");
                            }
                        }
                        
                    }
                    this.ViewBag.MainTourFindModel = model;
                }

                // выбираем подходящие заявки
                var RequestsData = Requests.Select(Global.DataBase, Requests.TableName, Where.ToString(), new List<ISelectArgs>() { new ORDER_BY(Requests.ID, "DESC") });
                // выбираем только те, которые должны оборажаться на странице стр 1: 0 - 11, стр 2: 12-23
                for(int i = Page * MaxItems; i < Page * MaxItems + MaxItems && i < RequestsData.Count; i++)
                    Result.Add(new RequestModel(RequestsData[i]));

                this.ViewBag.MenuType = MenuTypes.Manager;
                this.ViewBag.MaxPage = (int)Math.Ceiling(RequestsData.Count / (float)MaxItems) - 1;
            }
            // нумеруем выводимые элементы
            for(int i = 0; i < Result.Count; i++)
                Result[i].Index = (Page * MaxItems + i + 1);
            this.ViewBag.RequestItems = Result;
            this.ViewBag.ActivePage = Page;
            return base.View();
        }


        [HttpPost]
        // добавить заявку
        public JsonResult AddRequest(String Value)
        {
            // получаем данные заявки
            RequestDataModel model = JsonConvert.DeserializeObject<RequestDataModel>(Value);
            int UserID = -1;
            // проверяем тип добавления (авторизованный/ неавторизованный пользователь)
            if(model.DataType == 0)
                // если авторизованный, то запоминаем ИД пользователя
                UserID = model.UserID;
            else
            {
                // если нет, то првоеряем существует ли пользователь
                User User = Users.SelectFirst(Global.DataBase, Users.TableName, $"{Users.Phone} = {model.UserPhone}");
                if(User != null)
                {
                    // если да, то запоминаем ИД существующего пользователя
                    UserID = User.ID;
                } else
                {
                    // если нет, то регистрируем пользователя
                    int MaxID = Users.Max(Global.DataBase, Users.TableName, Requests.ID) + 1;
                    Dictionary<string, object> UserData = new Dictionary<string, object>()
                        {
                            { Users.ID, MaxID },
                            { Users.Name, model.UserName },
                            { Users.Mail, "" },
                            { Users.Phone, model.UserPhone },
                            { Users.Role, 0 },
                            { Users.Password, Global.SHA1("") },
                        };
                    // добавляем нового пользователя
                    if(Users.Insert(Global.DataBase, Users.TableName, UserData))
                    {
                        // запоминаем новый ИД
                        UserID = MaxID;
                        // авторизуемся
                        Global.LoginedUser.Add(new UserData(UserID, model.UserName, 0));
                        FormsAuthentication.SetAuthCookie(UserID.ToString(), true);
                    } else
                    {
                        string Error = Users.LastError;
                    }
                }
            }
            // если хоть какой-либо пользователь был получен
            if(UserID > -1)
            {
                // проверяем, существует ли у пользователя такая звявка
                if(Requests.Count(Global.DataBase, Requests.TableName, $"{Requests.User} = {model.UserID}") < 1)
                {
                    // если нет, то формируем поля для вставки
                    Dictionary<string, object> Fields = new Dictionary<string, object>()
                        {
                            { Requests.ID, Requests.Max(Global.DataBase, Requests.TableName, Requests.ID) + 1 },
                            { Requests.User, UserID },
                            { Requests.Tour, model.RequestID },
                            { Requests.Date, DateTime.Now.ToString("yyyy-MM-dd") },
                            { Requests.State, 0 },
                        };
                    // доабвляем данные
                    if(Requests.Insert(Global.DataBase, Requests.TableName, Fields))
                        return this.Json(new { Type = JsonTypes.Text, Text = "Заявка оформлена!" });
                    else
                    {
                        string Error = Requests.LastError;
                        return this.Json(new { Type = JsonTypes.Text, Text = "Произошла ошибка!" });
                    }
                }
            } else
                return this.Json(new { Type = JsonTypes.Text, Text = "Произршла ошибка идетификации пользователя!" });
            // если что-то пошло не так, либо заявка существует - выводим ошибку
            return this.Json(new { Type = JsonTypes.Text, Text = "Данные не обработаны!" });
        }

        [HttpPost]
        // удалить заявку
        public JsonResult RemRequest(Int32 Value)
        {
            // получаем ИД пользователя
            int UserID = Global.GetUserID(this);
            // если пользователь выбран
            if(UserID > -1)
            {
                // получаем заявку
                var Item = Requests.SelectFirst(Global.DataBase, Requests.TableName, $"{Requests.ID} = {Value}");
                // если автором заявки является пользователь
                if(Item?.User == UserID)
                {
                    // удаляем заявку
                    if(Requests.Delete(Global.DataBase, Requests.TableName, $"{Requests.ID} = {Value}"))
                    {
                        return this.Json(new { Type = JsonTypes.Reload });
                    } else
                    {
                        // если нет - выводим ошибку
                        string Error = Requests.LastError;
                        return this.Json(new { Type = JsonTypes.Text, Text = "Произошла ошибка при удалении!" });
                    } 
                }
            }
            // если пользователь не найден или не является автором - выводим ошибку
            return this.Json(new { Type = JsonTypes.Text, Text = "Доступ запрещён!" });
        }

        [HttpPost]
        // изменение состояния зявки
        public JsonResult SetState(String Value)
        {
            // получаем ИД пользователя
            int UserID = Global.GetUserID(this);
            // проверям что пользователь есть и он менеджер
            if(UserID > -1 && Global.GetUserRole(this) == Global.ManagerRoleID)
            {
                string Error = "";
                Dictionary<string, object> Fields = new Dictionary<string, object>(){};
                // получаем данные о заявке
                var model = JsonConvert.DeserializeObject<RequestStateModel>(Value);

                // удаляем заявку из таблицы отменённых заявок
                if(!RequestsRejected.Delete(Global.DataBase, RequestsRejected.TableName, $"{RequestsRejected.Request} = {model.RequestID}"))
                    Error = RequestsRejected.LastError;
                // удаляем заявку из таблицы подтверждённых заявок
                if(!RequestsAccepted.Delete(Global.DataBase, RequestsAccepted.TableName, $"{RequestsAccepted.Request} = {model.RequestID}"))
                    Error = RequestsAccepted.LastError;

                // если новое состояние "Подтверждена"
                if(model.RequestState == 1)
                {
                    // добавляем поля в список на добавление
                    Fields.Clear();
                    Fields.Add(RequestsAccepted.Request, model.RequestID);
                    Fields.Add(RequestsAccepted.Date, DateTime.Now.ToString("yyyy-MM-dd"));
                    Fields.Add(RequestsAccepted.Manager, UserID);
                    // вставляем данные в таблицу подтверждённых заявок
                    if(!RequestsAccepted.Insert(Global.DataBase, RequestsAccepted.TableName, Fields))
                        Error = RequestsAccepted.LastError;
                }
                // если новое состояние "Отменена"
                if(model.RequestState == 2)
                {
                    // добавляем поля в список на добавление
                    Fields.Clear();
                    Fields.Add(RequestsRejected.Request, model.RequestID);
                    Fields.Add(RequestsRejected.Date, DateTime.Now.ToString("yyyy-MM-dd"));
                    Fields.Add(RequestsRejected.Manager, UserID);
                    Fields.Add(RequestsRejected.Text, model.RequestText);
                    // вставляем данные в таблицу отменённых заявок
                    if(!RequestsRejected.Insert(Global.DataBase, RequestsRejected.TableName, Fields))
                        Error = RequestsRejected.LastError;
                }

                Fields.Clear();
                // добавляем поля в список на изменение
                Fields.Add(Requests.State, model.RequestState);
                // изменяем состояние в основной таблице "заявки
                if(Requests.Update(Global.DataBase, Requests.TableName, Fields, $"{Requests.ID} = {model.RequestID}"))
                    return this.Json(new { Type = JsonTypes.Nothing });
                else
                {
                    // в случае ошибки - выводим сообщение
                    Error = Requests.LastError;
                    return this.Json(new { Type = JsonTypes.Text, Text = "Произошла ошибка изменения состояния!" });
                }
            }
            // если пользватель не менеджер - выводим ошибку
            return this.Json(new { Type = JsonTypes.Text, Text = "Доступ запрещён!" });
        }
    }
}