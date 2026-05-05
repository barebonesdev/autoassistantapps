using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAssistantAppDataLibrary.ViewItems
{
    public class ViewItemMileageEntry : IComparable<ViewItemMileageEntry>, IEquatable<ViewItemMileageEntry>
    {
        public decimal Mileage { get; set; }

        public DateTime Date { get; set; }

        public DateTime DateCreated { get; set; }

        public ViewItemMileageEntry(ViewItemFuelEntry entry)
        {
            Mileage = entry.Mileage;
            Date = entry.Date;
            DateCreated = entry.DateCreated;
        }

        public ViewItemMileageEntry(ViewItemMaintenanceRecordEntry entry)
        {
            Mileage = entry.Mileage;
            Date = entry.Date;
            DateCreated = entry.DateCreated;
        }

        public int CompareTo(ViewItemMileageEntry other)
        {
            return Mileage.CompareTo(other.Mileage);
        }

        public bool Equals(ViewItemMileageEntry other)
        {
            return Mileage == other.Mileage && Date == other.Date;
        }
    }
}
