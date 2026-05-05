using AutoAssistantAppDataLibrary.DataLayer;
using AutoAssistantAppDataLibrary.DataLayer.DataItems.BaseItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAssistantAppDataLibrary.ViewItems.BaseViewItems
{
    internal class ViewItemPropertyAttribute : Attribute
    {

    }

    public abstract class BaseViewItem : BaseViewItemWithoutData, IComparable, IComparable<BaseViewItem>
    {
        public BaseDataItem DataItem { get; private set; }

        public AccountDataItem Account
        {
            get { return DataItem?.Account; }
        }

        public bool HasData
        {
            get { return DataItem != null; }
        }

        public BaseViewItem(Guid identifier)
        {
            Identifier = identifier;
        }

        public BaseViewItem(BaseDataItem dataItem)
        {
            if (dataItem != null)
            {
                DataItem = dataItem;

                Identifier = dataItem.Identifier;
                DateCreated = dataItem.DateCreated;

                PopulateFromDataItemOverride(dataItem);
            }
        }

        public void PopulateFromDataItem(BaseDataItem dataItem)
        {
            if (HasData)
            {
                DataItem = dataItem;
                PopulateFromDataItemOverride(dataItem);
            }
        }

        protected virtual void PopulateFromDataItemOverride(BaseDataItem dataItem)
        {
            Updated = dataItem.Updated;
        }

        public Guid Identifier { get; private set; }

        public DateTime DateCreated { get; set; }

        private DateTime _updated;
        public DateTime Updated
        {
            get { return _updated; }
            set { SetProperty(ref _updated, value, nameof(Updated)); }
        }


        public virtual int CompareTo(BaseViewItem other)
        {
            return this.DateCreated.CompareTo(other.DateCreated);
        }

        public virtual int CompareTo(object obj)
        {
            if (obj is BaseViewItem)
                return CompareTo(obj as BaseViewItem);

            return 0;
        }
    }
}
