function log(message) {
    console.log(message);
}
// открыть лайтбокс с фото
function lightbox(image) {
    let elem = $("#lightbox_image").attr("src", $(image).attr("src"));
    Show(".lightbox");
}

// -------------------------------------- функции работы с всплывающими окнами

// показаль элемент по селектору
function Show(select) {
    let elem = $(select);
    $(elem).addClass("show");
}
// скрыть элемент по селектору
function Hide(select) {
    let elem = $(select);
    $(elem).removeClass("show");
}

// вывести сообщение на экран
function msg(message) {
    switch (message.Type) {
        case 'Text':
            alert(message.Text);
            break;
        case 'Redirect':
            location.href(message.Location)
            break;
        case 'Reload':
            location.reload();
            break;
        case 'Object':
            return JSON.parse(message.Text);
            break;
        case 'None':
            return "";
            break;
        default:
            alert("Тип возвращаемого объекта не определён!");
    }
    return null;
}

// -------------------------------------- функции отправки данных

// отправить форму на севрвер методом post
// id - идентификатор формы
// action - адрес контроллера и метода
function send_form(id, action, createObject) {
    let res = true;
    let elem = document.getElementById(id);
    if (elem != null) {
        let items = $(elem).find("input");
        for (let i = 0; i < items.length; i++) {
            if (!check(null, items[i]))
                res = false;
        }
        if (res) {
            let item = createObject(elem);
            $.post(action, { JsonMessage: JSON.stringify(item) }).done(
                function (data) {
                    msg(data);
                }
            );
        }
    }
}

// отправить post запрос с callback функцией
function send(action, value, callback) {
    $.post(action, { Value: value }).done(
        function (data) {
            callback(data);
        });
}

// отправить post запрос с ответом
function post(action, value) {
    $.post(action, { Value: value }).done(
        function (data) {
            msg(data);
        });
}

// -------------------------------------- функции проверки полей

// проверка поле на корректность ввода в зависимости от checktype
function check(eventObject, item) {
    let res = true;
    let elem = (item === undefined) ? this : item;
    let attr = elem.getAttribute("checktype");
    if (attr != null && attr.length > 0) {
        res = false;
        if (attr.indexOf(",") > -1) {
            let arr = attr.split(",");
            for (let i = 0; i < arr.length; i++) {
                if (check_for_attr(arr[i], elem.value)) {
                    res = true;
                }
            }
        } else {
            res = check_for_attr(attr, elem.value);
        }
        check_set(elem, res);
    }
    return res;
}

// типы полей форм и их проверка
let oldpassword = "";
function check_for_attr(attr, value) {
    switch (attr) {
        case "text":
            return (value.length > 0);
        case "phone":
            return isNumeric(value);
        case "float":
            return isNumeric(value);
        case "number":
            return isNumeric(value);
        case "mail":
            let ind1 = value.indexOf("@");
            let ind2 = value.indexOf(".", ind1);
            return (ind1 > 0 && ind2 > 0);
        case "password":
            oldpassword = value;
            return (value.length > 0);
        case "checkpassword":
            return (value == oldpassword);
        default:
            return true;
    }
}
// естановить состояние поля - верно/неверно
function check_set(elem, value) {
    if (!value)
        elem.classList.add("invalid");
    else
        elem.classList.remove("invalid");
}

// -------------------------------------- функции проверок и работ с типами данных

// проверка на число
function isNumeric(num) {
    num = "" + num;
    return !isNaN(num) && !isNaN(parseFloat(num));
}


// -------------------------------------- функции создания объектов

function createLogin(elem) {
    let res = new Object();
    res.MailOrPhone = $(elem).find("input[name='MailOrPhone']").val();
    res.Password = $(elem).find("input[name='Password']").val();
    return res;
}

function createRegister(elem) {
    let res = new Object();
    res.Name = $(elem).find("input[name='Name']").val();
    res.Mail = $(elem).find("input[name='Mail']").val();
    res.Phone = $(elem).find("input[name='Phone']").val();
    res.Password = $(elem).find("input[name='Password']").val();
    res.ConfirmPassword = $(elem).find("input[name='ConfirmPassword']").val();
    return res;
}


// -------------------------------------- функции редактирования данных профиля

// пр инажатии на "Редактировать"
function edit(id_value, name, checktype) {
    let base_elem = $("#" + id_value);
    let value_elem = $("#" + id_value).find(".value");
    let edit_elem = $("#" + id_value).find(".edit");
    let value = $(value_elem).text();
    if (!base_elem.hasClass("editing")) { 
        base_elem.addClass("editing");
        value_elem.empty();
        value_elem.append('<input type="text" class="form-control" name="' + name + '" checktype="' + checktype + '" value="' + value + '" onblur="check(this, this)">');
        value_elem.append('<input type="hidden" name="' + name + '_basic" value="' + value + '">');
        edit_elem.empty();
        edit_elem.append('<button class="accept" onclick="save(\'' + id_value + '\',\'' + name + '\', \'' + checktype + '\')"><i class="fas fa-check"></i></button>');
        edit_elem.append('<button class="cancel" onclick="cancel(\'' + id_value + '\',\'' + name + '\', \'' + checktype + '\')"><i class="fas fa-times"></i></button>');
    }
}

// при нажатии на "Отмена"
function cancel(id_value, name, checktype) {
    let base_elem = $("#" + id_value);
    let value_elem = $("#" + id_value).find(".value");
    let edit_elem = $("#" + id_value).find(".edit");
    if (base_elem.hasClass("editing")) {
        let value = $(value_elem).find('input[type="hidden"]').val();
        base_elem.removeClass("editing");
        value_elem.empty();
        value_elem.text(value);
        edit_elem.empty();
        edit_elem.append('<button class="edit-on" onclick="edit(\'' + id_value + '\', \'' + name + '\', \'' + checktype + '\')"><i class="fas fa-pen"></i></button>');
    }
}

// при нажатии "Сохранить"
function save(id_value, name, checktype) {
    let base_elem = $("#" + id_value);
    let value_elem = $("#" + id_value).find(".value");
    let edit_elem = $("#" + id_value).find(".edit");
    if (base_elem.hasClass("editing")) {
        let value = $(value_elem).find('input[name="' + name + '"]').val();
        base_elem.removeClass("editing");
        value_elem.empty();
        value_elem.text(value);
        edit_elem.empty();
        edit_elem.append('<button class="edit-on" onclick="edit(\'' + id_value + '\', \'' + name + '\', \'' + checktype + '\')"><i class="fas fa-pen"></i></button>');
        post('/Account/Edit', name + ":" + value);
    }
}


// -------------------------------------- функции для форм

// запрос при удалении любого элемента
function RemoveElem(elem_id, action) {
    let id = $('#' + elem_id).val();
    if (confirm('Вы действительно хотите удалить элемент?'))
        post(action, id);
}

// -------------------------------------- функции для форм: "Страны"

// создать объект для отправки на сервер
function createCountry(elem) {
    let res = new Object();
    res.ID = $(elem).find("#IdField").val();
    res.Name = $(elem).find("#NameField").val();
    res.Map = $(elem).find("#MapField").val();
    res.Photo = $(elem).find("#PhotoField").val();
    return res;
}

// загрузить данные 
function LoadCountry(Item_ID, elem) {
    send("/Admin/GetCountry", Item_ID, UpdateCountry);
    $('.list-group .active').removeClass("active");
    $(elem).addClass("active");
}

// в случае получения данных - обновить данные на сайте
function UpdateCountry(data) {
    data = msg(data);
    let elem = $(".work-body .fields");
    if (elem != null) {
        $(elem).find("#IdField").val(data.ID);
        $(elem).find("#NameField").val(data.Name);
        $(elem).find("#MapField").val(data.Map);
        $(elem).find("#PhotoField").val(data.Photo);
    }
}

// -------------------------------------- функции для форм: "Курорты"

// создать объект для отправки на сервер
function createResort(elem) {
    let res = new Object();
    res.ID = $(elem).find("#IdField").val();
    res.Name = $(elem).find("#NameField").val();
    res.Country = $(elem).find("#CountryField").val();
    res.Map = $(elem).find("#MapField").val();
    res.Photos = $(elem).find("#PhotosField").val();
    res.Text = $(elem).find("#TextField").val();
    return res;
}

// загрузить данные 
function LoadResort(Item_ID, elem) {
    send("/Admin/GetResort", Item_ID, UpdateResort);
    $('.list-group .active').removeClass("active");
    $(elem).addClass("active");
}

// в случае получения данных - обновить данные на сайте
function UpdateResort(data) {
    data = msg(data);
    let elem = $(".work-body .fields");
    if (elem != null) {
        $(elem).find("#IdField").val(data.ID);
        $(elem).find("#NameField").val(data.Name);
        $(elem).find("#CountryField").val(data.Country);
        $(elem).find("#MapField").val(data.Map);
        $(elem).find("#PhotosField").val(data.Photos);
        $(elem).find("#TextField").val(data.Text);
    }
}

// -------------------------------------- функции для форм: "Отели"

// создать объект для отправки на сервер
function createHotel(elem) {
    let res = new Object();
    res.ID = $(elem).find("#IdField").val();
    res.Name = $(elem).find("#NameField").val();
    res.Resort = $(elem).find("#ResortField").val();
    res.Price = $(elem).find("#PriceField").val();
    res.Category = $(elem).find("#CategoryField").val();
    res.Eating = $(elem).find("#EatingField").val();
    res.Photos = $(elem).find("#PhotosField").val();
    res.Text = $(elem).find("#TextField").val();
    return res;
}

// загрузить данные 
function LoadHotel(Item_ID, elem) {
    send("/Admin/GetHotel", Item_ID, UpdateHotel);
    $('.list-group .active').removeClass("active");
    $(elem).addClass("active");
}

// в случае получения данных - обновить данные на сайте
function UpdateHotel(data) {
    data = msg(data);
    let elem = $(".work-body .fields");
    if (elem != null) {
        $(elem).find("#IdField").val(data.ID);
        $(elem).find("#NameField").val(data.Name);
        $(elem).find("#ResortField").val(data.Resort);
        $(elem).find("#PriceField").val(data.Price);
        $(elem).find("#CategoryField").val(data.Category);
        $(elem).find("#EatingField").val(data.Eating);
        $(elem).find("#PhotosField").val(data.Photos);
        $(elem).find("#TextField").val(data.Text);
    }
}

// -------------------------------------- функции для форм: "Туры"

// создать объект для отправки на сервер
function createTour(elem) {
    let res = new Object();
    res.ID = $(elem).find("#IdField").val();
    res.Otel = $(elem).find("#HotelField").val();
    res.Days = $(elem).find("#DaysField").val();
    res.Date = $(elem).find("#DateField").val();
    res.Price = $(elem).find("#PriceField").val();
    res.Text = $(elem).find("#TextField").val();
    res.Title = $(elem).find("#TitleField").val();
    res.Photo = $(elem).find("#PhotoField").val();
    return res;
}

// загрузить данные 
function LoadTour(Item_ID, elem) {
    send("/Admin/GetTour", Item_ID, UpdateTour);
    $('.list-group .active').removeClass("active");
    $(elem).addClass("active");
}

// в случае получения данных - обновить данные на сайте
function UpdateTour(data) {
    data = msg(data);
    let elem = $(".work-body .fields");
    if (elem != null) {
        log(data);
        $(elem).find("#IdField").val(data.ID);
        $(elem).find("#HotelField").val(data.Otel);
        $(elem).find("#DaysField").val(data.Days);
        $(elem).find("#DateField").val(data.Date);
        $(elem).find("#PriceField").val(data.Price);
        $(elem).find("#TextField").val(data.Text);
        $(elem).find("#TitleField").val(data.Title);
        $(elem).find("#PhotoField").val(data.Photo);
    }
}

// -------------------------------------- функции для форм: "Горящие туры"

// создать объект для отправки на сервер
function createFireTour(elem) {
    let res = new Object();
    res.Tour = $(elem).find("#TourField").val();
    res.TourID = $(elem).find("#TourIDField").val();
    res.StartDate = $(elem).find("#StartDateField").val();
    res.EndDate = $(elem).find("#EndDateField").val();
    res.Price = $(elem).find("#PriceField").val();
    return res;
}

// загрузить данные 
function LoadFireTour(Item_ID, elem) {
    send("/Admin/GetFireTour", Item_ID, UpdateFireTour);
    $('.list-group .active').removeClass("active");
    $(elem).addClass("active");
}

// в случае получения данных - обновить данные на сайте
function UpdateFireTour(data) {
    data = msg(data);
    let elem = $(".work-body .fields");
    if (elem != null) {
        $(elem).find("#TourField").val(data.Tour);
        $(elem).find("#TourIDField").val(data.TourID);
        $(elem).find("#StartDateField").val(data.StartDate);
        $(elem).find("#EndDateField").val(data.EndDate);
        $(elem).find("#PriceField").val(data.Price);
    }
}

// -------------------------------------- функции для форм: "Курорты"

// навигация на странице и поиск
function resortToVal(action, Page, value) {
    let Find = new Object();
    Find.Country = value;
    location.href = action + "?Page=" + Page + "&JsonMessage=" + JSON.stringify(Find);
}

// навигация на странице и поиск
function resortTo(action, Page) {
    let Find = new Object();
    Find.Country = $("#resort_country").val();
    location.href = action + "?Page=" + Page + "&JsonMessage=" + JSON.stringify(Find);
}

// -------------------------------------- функции для форм: "Отели"

// навигация на странице и поиск
function hotelTo(action, Page) {
    let Find = new Object();
    Find.Country = $("#hotel_country").val();
    Find.Resort = $("#hotel_resort").val();
    Find.Category = $("#hotel_category").val();
    Find.Eating = $("#hotel_eating").val();
    Find.MaxPrice = $("#hotel_price").val();
    location.href = action + "?Page=" + Page + "&JsonMessage=" + JSON.stringify(Find);
}

// -------------------------------------- функции для форм: "Акции и горячие предложения"

// навигация на странице и поиск
function firetourTo(action, Page) {
    let Find = new Object();
    Find.Country = $("#firetour_country").val();
    Find.Resort = $("#firetour_resort").val();
    Find.Category = $("#firetour_category").val();
    Find.Eating = $("#firetour_eating").val();
    Find.MaxPrice = $("#firetour_price").val();
    location.href = action + "?Page=" + Page + "&JsonMessage=" + JSON.stringify(Find);
}

// -------------------------------------- функции для форм: "Главный поиск"

// метод для компонента - поиск. Отображает выбранное значение
function setFindItem(id, country_id, country_name, callback) {
    let elem = $('#' + id);
    let data = $(elem).find(".value");
    data.text(country_name);
    data.attr('value', country_id);
    if (callback !== undefined)
        callback();
}

// метод загрузки доступных курортов, при выборе стран
function updMainResort() {
    let Item = new Object();
    Item.Country = $("#main_country .value").attr("value");
    send("/Home/GetResorts", JSON.stringify(Item), UpdateMainResort);
}

// в случае загрузки - обновить данные на сайте
function UpdateMainResort(data) {
    data = msg(data);
    let elem = $("#main_resort ~ .dropdown-menu");
    if (elem !== undefined) {
        elem.empty();
        elem.append('<button class="dropdown-item" onclick="setFindItem(\'main_resort\', -1, \'Любой\', updMainHotel)">Любой</button>');
        for (let i = 0; i < data.IDs.length; i++) {
            elem.append('<button class="dropdown-item" onclick="setFindItem(\'main_resort\', ' + data.IDs[i] + ', \'' + data.Titles[i] + '\', updMainHotel)">' + data.Titles[i] + '</button>');
        }
        elem = $("#main_resort .value");
        elem.text("Любой");
        elem.attr('value', -1);
    }
}

// метод загрузки доступных отелей, при выборе категорий или курортов
function updMainHotel() {
    let Item = new Object();
    Item.Resort = $("#main_resort .value").attr("value");
    Item.Category = $("#main_category .value").attr("value");
    Item.Eating = $("#main_eatings .value").attr("value");
    send("/Home/GetHotels", JSON.stringify(Item), UpdateMainHotel);
}

// в случае загрузки - обновить данные на сайте
function UpdateMainHotel(data) {
    data = msg(data);
    let elem = $("#main_hotels ~ .dropdown-menu");
    if (elem !== undefined) {
        elem.empty();
        elem.append('<button class="dropdown-item" onclick="setFindItem(\'main_hotels\', -1, \'Любой\')">Любой</button>');
        for (let i = 0; i < data.IDs.length; i++) {
            elem.append('<button class="dropdown-item" onclick="setFindItem(\'main_hotels\', ' + data.IDs[i] + ', \'' + data.Titles[i] + '\')">' + data.Titles[i] + '</button>');
        }
        elem = $("#main_hotels .value");
        elem.text("Любой");
        elem.attr('value', -1);
    }
}

// навигация на странице и поиск
function mainTo(action, Page) {
    let Find = new Object();
    Find.Country = $("#main_country .value").attr("value");
    Find.Resort = $("#main_resort .value").attr("value");
    Find.Category = $("#main_category .value").attr("value");
    Find.Eating = $("#main_eatings .value").attr("value");
    let elem = $("#main_hotels .value");
    if(elem !== undefined)
        Find.Hotel = elem.attr("value");
    Find.MaxPrice = $("#main_price").val();
    if (Find.MaxPrice.length == 0)
        Find.MaxPrice = 0;
    location.href = action + "?Page=" + Page + "&JsonMessage=" + JSON.stringify(Find);
}

// -------------------------------------- функции работы с заявками

// навигация на странице и поиск
function requestTo(action, Page) {
    let Find = new Object();
    Find.Country = $("#main_country .value").attr("value");
    Find.Resort = $("#main_resort .value").attr("value");
    let elem = $("#main_hotels .value");
    if (elem !== undefined)
        Find.Hotel = elem.attr("value");
    Find.MaxPrice = 0;
    location.href = action + "?Page=" + Page + "&JsonMessage=" + JSON.stringify(Find);
}

// отправить заявку
function send_request(UserID, RequestID) {
    if (UserID > -1) {
        // если пользователь авторизован
        let Item = new Object();
        Item.DataType = 0;
        Item.UserID = UserID;
        Item.RequestID = RequestID;
        send("/Request/AddRequest", JSON.stringify(Item), msg);
    } else {
        // если нет, то показываем окно с просьбой ввести свои данные
        $("#request_form button").attr("requestID", RequestID);
        Show(".request");
    }
}

// отправить заявку, в случае ,если пользователь не авторизован
function send_request_form() {
    let elem = $("#request_form");
    let items = $(elem).find("input")
    let res1 = check(null, items[0]);
    let res2 = check(null, items[1]);
    if (res1 && res2) {
        let Item = new Object();
        Item.DataType = 1;
        Item.UserName = $("#RequestNameBox").val();
        Item.UserPhone = $("#RequestPhoneBox").val();
        Item.RequestID = $("#request_form button").attr("requestID");
        send("/Request/AddRequest", JSON.stringify(Item), msg);
    }
}

// удалить свою заявку
function DeleteRequest(RequestID) {
    send("/Request/RemRequest", RequestID, msg);
}

// изменить состояние заявки
function setRequestState(RequestID) {
    let OnSend = true;
    let state = $("#RequestState_" + RequestID).val();
    let Item = new Object();
    Item.RequestID = RequestID;
    Item.RequestState = state;
    if (state == 2) {
        Item.RequestText = prompt("Введите причину отмены", "");
        if (Item.RequestText === undefined)
            OnSend = false;
    }
    if(OnSend)
        send("/Request/SetState", JSON.stringify(Item), msg);
}


// -----------------------------------------------------------------------------------------------------------------------