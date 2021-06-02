using System;

namespace TourSnapProjects.Models.Find
{
    /// <summary>
    /// Модель главного поиска
    /// </summary>
    public class MainTourFindModel
    {
        public Int32 Country { get; set; } = -1;
        public Int32 Resort { get; set; } = -1;
        public Int32 Category { get; set; } = -1;
        public Int32 Eating { get; set; } = -1;
        public Int32 Hotel { get; set; } = -1;
        public Double MaxPrice { get; set; } = 0;
    }
}