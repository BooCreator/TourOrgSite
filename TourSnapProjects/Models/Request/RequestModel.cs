using System;

using TourSnapModels.Models.DataBase;

using TourSnapProjects.Models.PublicModels;

using RequestItem = TourSnapModels.Models.Data.Request;

namespace TourSnapProjects.Models.Request
{
    /// <summary>
    /// Модель данных о заявки для вывода на страницу
    /// </summary>
    public class RequestModel
    {
        public Int32 Index { get; set; }
        public Int32 ID { get; set; }
        public Int32 UserID { get; set; }
        public String User { get; set; }
        public Int32 TourID { get; set; }
        public String Tour { get; set; }
        public Double TourPrice { get; set; }
        public Boolean IsFire { get; set; }
        public String Date { get; set; }
        public Int32 StateID { get; set; }
        public String State { get; set; }

        public RequestModel(RequestItem Item, Int32 Index = -1)
        {
            this.Index = Index;
            this.ID = Item.ID;
            this.UserID = Item.User;
            this.Date = Item.Date.ToString("dd.MM.yyyy");
            this.StateID = Item.State;

            var User = Users.SelectFirst(Global.DataBase, Users.TableName, $"{Users.ID} = {Item.User}");
            if(User != null)
                this.User = User.Name;

            var Tour = Tours.SelectFirst(Global.DataBase, Tours.TableName, $"{Tours.ID} = {Item.Tour}");
            var TourData = (Tour != null) ? new TourModel(Tour) : null;
            if(TourData != null)
            {
                this.IsFire = TourData.IsFire;
                this.Tour = TourData.Title;
                this.TourID = TourData.ID;
                this.TourPrice = (TourData.IsFire) ? TourData.FirePrice : TourData.Price;
            }

            var State = RequestStates.SelectFirst(Global.DataBase, RequestStates.TableName, $"{RequestStates.ID} = {Item.State}");
            if(State != null)
                this.State = State.Title;
        }
    }
}