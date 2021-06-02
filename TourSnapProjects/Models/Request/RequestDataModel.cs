using System;

namespace TourSnapProjects.Models.Request
{
    /// <summary>
    /// Модель данных о заявки для оформления
    /// </summary>
    public class RequestDataModel
    {
        public Int32 DataType { get; set; }
        public Int32 UserID { get; set; }
        public Int32 RequestID { get; set; }
        public String UserName { get; set; }
        public Int32 UserPhone { get; set; }
    }
}