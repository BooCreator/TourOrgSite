using System;

namespace TourSnapProjects.Models.Admin
{
    /// <summary>
    ///  Модель с данными горящих туров для изменения
    /// </summary>
    public class FireTourData
    {
        public Int32 TourID { get; set; }
        public Int32 Tour { get; set; }
        public String StartDate { get; set; }
        public String EndDate { get; set; }
        public Double Price { get; set; }
    }
}