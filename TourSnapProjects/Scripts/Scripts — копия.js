function log(message) {
    console.log(message);
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

function check_set(elem, value) {
    if (!value)
        elem.classList.add("invalid");
    else
        elem.classList.remove("invalid");
}

// -------------------------------------- функции проверок и работ с типами данных

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


// -------------------------------------- функции редактирования

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

function RemoveElem(elem_id, action) {
    let id = $('#' + elem_id).val();
    if (confirm('Вы действительно хотите удалить элемент?'))
        post(action, id);
}

// -------------------------------------- функции для форм: "Страны"

function createCountry(elem) {
    let res = new Object();
    res.ID = $(elem).find("#IdField").val();
    res.Name = $(elem).find("#NameField").val();
    res.Map = $(elem).find("#MapField").val();
    res.Photo = $(elem).find("#PhotoField").val();
    return res;
}

function LoadCountry(Item_ID, elem) {
    send("/Admin/GetCountry", Item_ID, UpdateCountry);
    $('.list-group .active').removeClass("active");
    $(elem).addClass("active");
}

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

function LoadResort(Item_ID, elem) {
    send("/Admin/GetResort", Item_ID, UpdateResort);
    $('.list-group .active').removeClass("active");
    $(elem).addClass("active");
}

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

function LoadHotel(Item_ID, elem) {
    send("/Admin/GetHotel", Item_ID, UpdateHotel);
    $('.list-group .active').removeClass("active");
    $(elem).addClass("active");
}

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

function LoadTour(Item_ID, elem) {
    send("/Admin/GetTour", Item_ID, UpdateTour);
    $('.list-group .active').removeClass("active");
    $(elem).addClass("active");
}

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

function createFireTour(elem) {
    let res = new Object();
    res.Tour = $(elem).find("#TourField").val();
    res.TourID = $(elem).find("#TourIDField").val();
    res.StartDate = $(elem).find("#StartDateField").val();
    res.EndDate = $(elem).find("#EndDateField").val();
    res.Price = $(elem).find("#PriceField").val();
    return res;
}

function LoadFireTour(Item_ID, elem) {
    send("/Admin/GetFireTour", Item_ID, UpdateFireTour);
    $('.list-group .active').removeClass("active");
    $(elem).addClass("active");
}

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

function resortToVal(action, Page, value) {
    let Find = new Object();
    Find.Country = value;
    location.href = action + "?Page=" + Page + "&JsonMessage=" + JSON.stringify(Find);
}

function resortTo(action, Page) {
    let Find = new Object();
    Find.Country = $("#resort_country").val();
    location.href = action + "?Page=" + Page + "&JsonMessage=" + JSON.stringify(Find);
}

// -------------------------------------- функции для форм: "Отели"

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

function setFindItem(id, country_id, country_name, callback) {
    let elem = $('#' + id);
    let data = $(elem).find(".value");
    data.text(country_name);
    data.attr('value', country_id);
    if (callback !== undefined)
        callback();
}

function updMainResort() {
    let Item = new Object();
    Item.Country = $("#main_country .value").attr("value");
    send("/Home/GetResorts", JSON.stringify(Item), UpdateMainResort);
}

function UpdateMainResort(data) {
    data = msg(data);
    let elem = $("#main_resort ~ .dropdown-menu")
    elem.empty();
    elem.append('<button class="dropdown-item" onclick="setFindItem(\'main_resort\', -1, \'Любой\', updMainHotel)">Любой</button>');
    for (let i = 0; i < data.IDs.length; i++) {
        elem.append('<button class="dropdown-item" onclick="setFindItem(\'main_resort\', ' + data.IDs[i] + ', \'' + data.Titles[i] + '\', updMainHotel)">' + data.Titles[i] + '</button>');
    }
    elem = $("#main_resort .value");
    elem.text("Любой");
    elem.attr('value', -1);
}

function updMainHotel() {
    let Item = new Object();
    Item.Resort = $("#main_resort .value").attr("value");
    Item.Category = $("#main_category .value").attr("value");
    Item.Eating = $("#main_eatings .value").attr("value");
    send("/Home/GetHotels", JSON.stringify(Item), UpdateMainHotel);
}

function UpdateMainHotel(data) {
    data = msg(data);
    let elem = $("#main_hotels ~ .dropdown-menu")
    elem.empty();
    elem.append('<button class="dropdown-item" onclick="setFindItem(\'main_hotels\', -1, \'Любой\')">Любой</button>');
    for (let i = 0; i < data.IDs.length; i++) {
        elem.append('<button class="dropdown-item" onclick="setFindItem(\'main_hotels\', ' + data.IDs[i] + ', \'' + data.Titles[i] + '\')">' + data.Titles[i] + '</button>');
    }
    elem = $("#main_hotels .value");
    elem.text("Любой");
    elem.attr('value', -1);
}


function mainTo(action, Page) {
    let Find = new Object();
    Find.Country = $("#main_country .value").attr("value");
    Find.Resort = $("#main_resort .value").attr("value");
    Find.Category = $("#main_category .value").attr("value");
    Find.Eating = $("#main_eatings .value").attr("value");
    Find.Hotel = $("#main_hotels .value").attr("value");
    Find.MaxPrice = $("#main_price").val();
    if (Find.MaxPrice.length == 0)
        Find.MaxPrice = 0;
    location.href = action + "?Page=" + Page + "&JsonMessage=" + JSON.stringify(Find);
}




// -----------------------------------------------------------------------------------------------------------------------
// показать работы выбранной категории по селектору
function ShowCategory(select) {
    $(".category.show").removeClass("show");
    $(select).addClass("show");
}
// показать картинку во весь экран
function Lightbox(elem) {
    let src = $(elem).find("img").attr("src");
    let image = $("#lightbox_image").attr("src", src);
    $(".lightbox").addClass("show");
}

// добавить пустой блок на странице редактирования страниц
function add_edit() {
    $("#items").append(
        '<div class="edit">' +
        '<div class="type">' +
        $($(".type")[0]).html() +
        '</div>' +
        '<textarea maxlength=\"8000\"></textarea>' +
        '</div>'
    );
}
// добавить пустую комнату в калькуляторе
function add_room() {
    $(".check_room").append(
        '<div class= "room" >' +
        '<label class="form-label title">Помещение</label>' +
        '<div>' +
        '<label for="validationCustom01" class="form-label">Площадь(М²)</label>' +
        '<input type="text" class="form-control sqare" value="" onchange="SumAll()">' +
        '<div class="invalid-feedback"></div>' +
        '</div>' +
        '<div>' +
        '<label for="validationCustom01" class="form-label">Высота (М)</label>' +
        '<input type="text" class="form-control height" value="" onchange="SumAll()">' +
        '<div class="invalid-feedback"></div>' +
        '</div>' +
        '</div>'
    );
}
// удалить блок на странице редактирования страниц
function remove_edit(elem) {
    if ($(".edit").length > 1)
        $(elem).closest(".edit").remove();
    else
        msg("На странице должен быть как минимум один блок!");
}

//отправить информацию страницы после редактирования/добавления
function send_page(action, item_id) {
    let value = $("#Title").val();
    let submenu = $("#submenu").val();
    let elems = $(".edit");
    let items = [];
    for (let i = 0; i < elems.length; i++) {
        let type = $(elems[i]).children(".type").children("select").val();
        let text = $(elems[i]).children("textarea").val();
        items[i] = {
            Type: type,
            Text: text.replaceAll("<", "&code_lt;").replaceAll(">", "&code_gt;")
        };
    }
    let item = {
        Id: item_id,
        Title: value,
        Submenu: submenu,
        Items: items
    };
    $.post(action, { Json: JSON.stringify(item) }).done(
        function (data) {
            msg(data);
            if (item_id == -1)
                location.href = "/Page/LastPage";
            else
                location.href = "/Page/Index/" + item_id;
        });
}
// рассчитать общую сумму работ на странице калькулятора
function SumAll() {
    let price = $('input[name="radio-work"]:checked').val();
    if (price === undefined) {
        price = 0;
    }
    let items = $(".room");
    let sum = 0;
    for (let i = 0; i < items.length; i++) {
        let input1 = $(items[i]).find(".sqare").val();
        let input2 = $(items[i]).find(".height").val();;
        if (input1 > 0 && input2 > 0) {
            sum += price * input1 + price * input2;
        }
    }
    $("#price").text(sum + " руб.");
}
// отправить заявку
function send_request(action) {
    let name = $("#fio_name").val();
    let phone = $("#phone").val();
    if (name.length > 0 && phone.length > 0) {
        let calculalte = "";
        let category = $('input[name="radio-stacked"]:checked').attr("id");
        let work = $('input[name="radio-work"]:checked').attr("id");
        if (work !== undefined) {
            let price = $('#price').html();
            if (price === undefined) {
                price = 0;
            }
            price = price.replace(" руб.", "");
            calculalte += "[" + category.replace("category_", "") + "]";
            calculalte += "[" + work.replace("work_", "") + "]";
            calculalte += "[" + price + "]";
            let items = $(".room");
            for (let i = 0; i < items.length; i++) {
                let input1 = $(items[i]).find(".sqare").val();
                let input2 = $(items[i]).find(".height").val();;
                if (input1 > 0 && input2 > 0) {
                    calculalte += "[" + input1 + "," + input2 + "]";
                }
            }
        }
        $.post(action, { Name: name, Phone: phone, Calculate: calculalte }).done(
            function (data) {
                msg(data);
                location.reload();
            });
    }
}
// обновить состояние запроса
function update_ReqState(action, request, elem) {
    let state = $(elem).val();
    $.post(action, { Id: request, State: state }).done(function (data) { });
}
// обновить состояние запроса, с указанием конкретного значения
function update_ReqState_Value(action, request, state) {
    $.post(action, { Id: request, State: state }).done(function (data) { });
}

// обновить блок в подвале для страницы
function update_PageBlock(action, request, elem) {
    let block = $(elem).val();
    $.post(action, { Id: request, Block: block }).done(function (data) { });
}
// обновить блок в подвале для страницы, с указанием конкретного значения
function update_PageBlock_value(action, request, block) {
    $.post(action, { Id: request, Block: block }).done(function (data) { });
}

// удаление страницы по ID установленному в #page
function remove(action) {
    let value = $("#page").val();
    post(action, value);
}

// показаль элемент по селектору и установить значение
function Show_Value(select, value) {
    $("#page").val(value);
    let elem = $(select);
    $(elem).addClass("show");
}

// отправить данные профиля на обновление
function send_profile(action) {
    let id = $("#edit_id").attr("value");
    let name = $("#edit_fio").val();
    let phone = $("#edit_phone").val();
    let npass = $("#NewPassword").val();
    let pass = $("#edit_password").val();
    if (name.length > 0 && phone.length > 0 && pass.length > 0) {
        $.post(action, { Id: id, Name: name, Phone: phone, Password: pass, NewPassword: npass }).done(
            function (data) {
                msg(data);
                location.reload();
            });
    }
}