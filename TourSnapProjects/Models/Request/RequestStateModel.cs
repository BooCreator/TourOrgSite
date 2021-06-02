using System;

namespace TourSnapProjects.Models.Request
{
    /// <summary>
    /// Модель изменения состояния заявки
    /// </summary>
    public class RequestStateModel
    {
        public Int32 RequestID { get; set; }
        public Int32 RequestState { get; set; }
        public String RequestText { get; set; }
    }
}