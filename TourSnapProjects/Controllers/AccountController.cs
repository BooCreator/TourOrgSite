using System;
using System.Web.Mvc;
using System.Web.Security;
using System.Collections.Generic;

using TourSnapModels.Models.Data;
using TourSnapModels.Models.DataBase;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace TourSnapProjects.Controllers
{
    public class AccountController : Controller
    {
        /// <summary>
        /// Регулярное выражение для проверки электронной почты
        /// </summary>
        String Mail = 
            @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
            @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$";

        // Функция авторизации
        [HttpPost]
        public JsonResult Login(String JsonMessage)
        {
            Models.Account.LoginModel model = JsonConvert.DeserializeObject<Models.Account.LoginModel>(JsonMessage);

            // Если модель заполнена корректно
            if (this.ModelState.IsValid)
            {
                User User = null;
                model.MailOrPhone = model.MailOrPhone.Replace("+", "");
                // если был введёт телефон
                if(Int32.TryParse(model.MailOrPhone, out int Phone))
                {
                    // получаем пользователя из базы данных
                    User = Users.SelectFirst(Global.DataBase, Users.TableName, 
                        $"{Users.Phone} = {Phone} and {Users.Password} = '{Global.SHA1(model.Password)}'");
                } else
                {
                    // если была введена электронная почта
                    if (Regex.IsMatch(model.MailOrPhone, Mail, RegexOptions.IgnoreCase))
                    {
                        // получаем пользователя из базы данных
                        User = Users.SelectFirst(Global.DataBase, Users.TableName, 
                            $"{Users.Mail} = '{model.MailOrPhone}' and {Users.Password} = '{Global.SHA1(model.Password)}'");
                    } else
                    {
                        // если ничего не подошло, то сообщаем об ошибке
                        return Json(new { Type = JsonTypes.Text, Text = "Телефон(Только цифры с кодом) или e-mail(XXX@XXX.XX) не верны!" });
                    }
                }

                // если пользователь был найден
                if(User != null)
                {
                    // авторизуем его в системе
                    Global.LoginedUser.Add(new UserData(User.ID, User.Name, User.Role));
                    FormsAuthentication.SetAuthCookie(User.ID.ToString(), true);
                    // обновляем страницу
                    return Json(new { Type = JsonTypes.Reload });
                }
                 else
                    // если не найдент - сообщаем об ошибке
                    return Json(new { Type = JsonTypes.Text, Text = "Пользователь с таким логином и паролем не найден!" });
            }
            // если модель не прошла проверку - выводим сообщение
            return Json(new { Type = JsonTypes.Text, Text = "Введены неверные данные!" });
        }

        // Функция регистрации
        [HttpPost]
        public JsonResult Register(String JsonMessage)
        {
            Models.Account.RegisterModel model = JsonConvert.DeserializeObject<Models.Account.RegisterModel>(JsonMessage);
            // Если модель заполнена корректно
            if (this.ModelState.IsValid)
            {
                // ищем пользователя в зависимости от ведённых данных
                var User = Users.SelectFirst(Global.DataBase, Users.TableName, 
                    $"{Users.Mail} Like '{model.Mail}' or {Users.Phone} = {model.Phone}");

                // если пользователь не найден
                if (User == null)
                {
                    // заполняем список данных для вставки
                    int MaxID = 1 + Users.Max(Global.DataBase, Users.TableName, Users.ID);
                    var Fields = new Dictionary<string, object>()
                    {
                        { Users.ID, MaxID },
                        { Users.Name, model.Name },
                        { Users.Mail, model.Mail },
                        { Users.Phone, model.Phone },
                        { Users.Role, 0 },
                        { Users.Password, Global.SHA1(model.Password) },
                    };
                    // если вставка прошла успешно
                    if (Users.Insert(Global.DataBase, Users.TableName, Fields))
                    {
                        // авторизуем пользователя
                        Global.LoginedUser.Add(new UserData(MaxID, model.Name, 0));
                        FormsAuthentication.SetAuthCookie(MaxID.ToString(), true);
                        return Json(new { Type = JsonTypes.Reload });
                    }
                    else
                    {
                        // если нет - сообщаем об ошибке
                        string Error = Users.LastError;
                        this.ModelState.AddModelError("", "Произошла ошибка регистрации!");
                    }
                }
                else
                    // если пользователь существует - сообщаем об ошибке
                    return Json(new { Type = JsonTypes.Text, Text = "Пользователь с таким логином или телефоном уже существует!" });
            }
            // если модель не прошла проверку - выводим сообщение
            return Json(new { Type = JsonTypes.Text, Text = "Введены неверные данные!" });
        }

        // Функция выхода из системы
        public ActionResult Logoff()
        {
            Global.LoginedUser.Remove(Global.LoginedUser.Find(x => x.UserID == Global.GetUserID(this)));
            FormsAuthentication.SignOut();
            return this.RedirectToAction("Index", "Home");
        }

        // Изменение данных пользователя
        public JsonResult Edit(String Value)
        {
            // получаем ID пользователя, изменяющего данные
            var UserID = Global.GetUserID(this);
            // если пользователь найден
            if(UserID > -1)
            {
                // в зависимости от изменяемого поля заполняем список на изменение
                Dictionary<string, object> Fields = new Dictionary<string, object>();
                if(Value.IndexOf("Name") == 0)
                {
                    Fields.Add(Users.Name, Value.Substring(5));
                } else
                if(Value.IndexOf("Mail") == 0)
                {
                    Fields.Add(Users.Mail, Value.Substring(5));
                } else
                if(Value.IndexOf("Phone") == 0)
                {
                    Fields.Add(Users.Phone, Value.Substring(6));
                } else
                if(Value.IndexOf("Password") == 0)
                {
                    string temp = Value.Substring(9);
                    Fields.Add(Users.Password, Global.SHA1(temp));
                }
                // изменяем данные
                if(Users.Update(Global.DataBase, Users.TableName, Fields, $"{Users.ID} = {UserID}"))
                    // если изменение прошло успешно - обновляем страницу
                    return this.Json(new { Type = JsonTypes.Reload });
                else
                {
                    // если нет - выводим сообщение об ошибке
                    string Error = Users.LastError;
                    this.Json(new { Type = JsonTypes.Text, Text = "Произошла ошибка!" });
                }
            }
            // если пользователь не найден - вывобдим сообщение об ошибке
            return this.Json(new { Type = JsonTypes.Text, Text = "Ошибка Доступа!" });
        }

    }
}