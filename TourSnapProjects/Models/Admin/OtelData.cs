using System;

namespace TourSnapProjects.Models.Admin
{
    /// <summary>
    ///  Модель с данными отеля для изменения
    /// </summary>
    public class OtelData
    {
        public Int32 ID { get; set; }
        public String Name { get; set; }
        public Int32 Resort { get; set; }
        public Double Price { get; set; }
        public Int32 Category { get; set; }
        public Int32 Eating { get; set; }
        public String Photos { get; set; }
        public String Text { get; set; }
    }
}