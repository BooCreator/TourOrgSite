using System;

namespace TourSnapProjects.Models.Find
{
    /// <summary>
    /// Упрощённая модель поиска отеля
    /// </summary>
    public class HotelSimpleFindModel
    {
        public Int32 Resort { get; set; } = -1;
        public Int32 Category { get; set; } = -1;
        public Int32 Eating { get; set; } = -1;
    }
}