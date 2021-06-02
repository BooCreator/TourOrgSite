using System;

namespace TourSnapProjects.Models.Admin
{
    /// <summary>
    /// Мотедль с данными курорта для изменения
    /// </summary>
    public class ResortData
    {
        public Int32 ID { get; set; }
        public String Name { get; set; }
        public Int32 Country { get; set; }
        public String Map { get; set; }
        public String Photos { get; set; }
        public String Text { get; set; }
    }
}