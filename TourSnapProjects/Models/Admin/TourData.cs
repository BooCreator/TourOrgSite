using System;

namespace TourSnapProjects.Models.Admin
{
    /// <summary>
    /// Модель с данными тура для вывода изменения
    /// </summary>
    public class TourData
    {
        public Int32 ID { get; set; }
        public Int32 Otel { get; set; }
        public Int32 Days { get; set; }
        public String Date { get; set; }
        public Double Price { get; set; }
        public String Text { get; set; }
        public String Title { get; set; }
        public String Photo { get; set; }
        public Boolean IsFire { get; set; }
    }
}