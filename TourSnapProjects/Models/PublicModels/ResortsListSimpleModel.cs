using System;
using System.Collections.Generic;

using TourSnapModels.Models.Data;

namespace TourSnapProjects.Models.PublicModels
{
    /// <summary>
    /// Модель для вывода подходящих курортов в меню поиска
    /// </summary>
    public class ResortsListSimpleModel
    {
        public Int32[] IDs { get; set; }
        public String[] Titles { get; set; }
        public ResortsListSimpleModel(List<Resort> Items)
        {
            List<int> IDs = new List<int>();
            List<string> Titles = new List<string>();
            foreach(var Item in Items){
                IDs.Add(Item.ID);
                Titles.Add(Item.Name);
            }
            this.IDs = IDs.ToArray();
            this.Titles = Titles.ToArray();
        }
    }
}