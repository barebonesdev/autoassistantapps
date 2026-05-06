using AutoAssistantLibrary.Items;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace AutoAssistantAppDataLibrary.DataLayer.DataItems.BaseItems
{

    public class DataItemProperty
    {
        public string[] AffectedSyncPropertyNames { get; private set; }

        private DataItemProperty() { }

        public static DataItemProperty Register(params string[] affectedSyncPropertyNames)
        {
            return new DataItemProperty()
            {
                AffectedSyncPropertyNames = affectedSyncPropertyNames
            };
        }
    }

    [DataContract]
    public abstract class BaseDataItem : IComparable, IComparable<BaseDataItem>, IEquatable<BaseDataItem>
    {
        private const string IDENTIFIER = "Identifier";
        private const string DATE_CREATED = "DateCreated";
        private const string UPDATED = "Updated";

        [NotMapped]
        public AccountDataItem Account { get; internal set; }

        private Dictionary<DataItemProperty, object> _propertyValues = new Dictionary<DataItemProperty, object>();

        internal object GetValue(DataItemProperty property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            object value;

            if (_propertyValues.TryGetValue(property, out value))
                return value;

            return null;
        }

        internal T GetValue<T>(DataItemProperty property, T defaultValue = default(T))
        {
            if (property == null)
                throw new ArgumentNullException("property");

            object value = GetValue(property);

            if (value is T)
                return (T)value;

            return defaultValue;
        }

        internal void SetValue(DataItemProperty property, object value)
        {
            if (property == null)
            {
                return;
            }

            _propertyValues[property] = value;
        }

        public bool IsNewItem()
        {
            return Identifier == Guid.Empty;
        }

        private static Dictionary<Type, DataItemProperty[]> _cachedProperties = new Dictionary<Type, DataItemProperty[]>();

        internal DataItemProperty[] GetProperties()
        {
            /// The properties are static readonly properties on the class. So if an instance of the class already exists, that means the properties 
            /// have already been assigned and will never change. Thus, we can load them once for each type and then simply cache them for
            /// future lookups.

            DataItemProperty[] properties;

            Type type = this.GetType();

            // If the properties have already been loaded for this type
            if (_cachedProperties.TryGetValue(type, out properties))
                return properties;

            // Otherwise load the properties for this type
            properties = type.GetTypeInfo().DeclaredFields.Where(f => f.IsStatic && f.FieldType == typeof(DataItemProperty)).Select(f => f.GetValue(null)).OfType<DataItemProperty>().ToArray();

            // And then cache the properties for this type
            _cachedProperties[type] = properties;

            // And then return them
            return properties;
        }

        /// <summary>
        /// Gets a collection of properties that have already been set on the item
        /// </summary>
        /// <returns></returns>
        internal DataItemProperty[] GetSetProperties()
        {
            return _propertyValues.Keys.ToArray();
        }

        /// <summary>
        /// Applies changes from the item, and tracks which properties actually changed. Properties that weren't set on the From item are ignored.
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        internal HashSet<string> ImportChanges(BaseDataItem from)
        {
            // Updated is always changed, we always copy it, we always sync it up too
            this.Updated = from.Updated;

            // Construct a list for our answer of sync properties that have changes
            HashSet<string> changedSyncProperties = new HashSet<string>();

            // Get all the properties that have been set on the item
            DataItemProperty[] setProperties = from.GetSetProperties();

            // And copy each property to this item
            foreach (var property in setProperties)
            {
                object currValue = this.GetValue(property);
                object newValue = from.GetValue(property);

                // If the objects are equal, no need to make a change
                if (object.Equals(currValue, newValue))
                    continue;

                // Otherwise, make the change
                this.SetValue(property, newValue);

                // And also flag that these sync properties have changed
                foreach (var syncPropertyName in property.AffectedSyncPropertyNames)
                    changedSyncProperties.Add(syncPropertyName);
            }

            // And return our answer of changed sync properties
            return changedSyncProperties;
        }

        public static string SerializeToString(object obj)
        {
            if (obj == null)
                return null;

            using (StringWriter writer = new StringWriter())
            {
                new JsonSerializer().Serialize(writer, obj);

                writer.Flush();

                return writer.ToString();
            }
        }

        public static T DeserializeFromString<T>(string str)
        {
            if (str == null)
                return default(T);

            using (StringReader reader = new StringReader(str))
            {
                return (T)new JsonSerializer().Deserialize(reader, typeof(T));
            }
        }

        public enum DataPropertyNames
        {
            //Identifier always sent
            DateCreated,
            //Updated always sent
            UpperIdentifier,
            SecondUpperIdentifier,
            Name,
            Details,
            RawImageNames,
            Date,
            EndTime,
            Reminder,
            PercentComplete,

            OverriddenGrade,

            //class
            CourseNumber,
            Credits,
            ShouldAverageGradeTotals,
            DoesRoundGradesUp,
            RawColor,
            Position,
            DoesCountTowardGPA,
            GradeScales,
            GPA,

            //grade
            GradeReceived,
            GradeTotal,
            IsDropped,
            IndividualWeight,

            //schedule
            DayOfWeek,
            StartTime,
            //EndTime already listed
            Room,
            ScheduleType,
            ScheduleWeek,
            LocationLatitude,
            LocationLongitude,

            //semester
            Start,
            End,

            //teacher
            PhoneNumbers,
            EmailAddresses,
            PostalAddresses,
            OfficeLocations,

            //weight category
            WeightValue,

            //used if item was created, send up everything
            All
        }

        [Key]
        [Column(IDENTIFIER)]
        public Guid Identifier { get; set; }

        private DateTime _dateCreated = SqlDate.MinValue;
        [Column(DATE_CREATED)]
        public DateTime DateCreated
        {
            get { return _dateCreated; }
            set { _dateCreated = EnsureUtc(value); }
        }

        private DateTime _updated = SqlDate.MinValue;
        [Column(UPDATED)]
        public DateTime Updated
        {
            get { return _updated; }
            set { _updated = EnsureUtc(value); }
        }

        private DateTime EnsureUtc(DateTime original)
        {
            if (original.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(original, DateTimeKind.Utc);

            return original;
        }

        protected abstract Type GetSyncItemType();

        public BaseSyncItem Serialize()
        {
            var properties = GetSyncItemType().GetRuntimeProperties().ToArray();

            return SerializeOnlyChangedProperties(properties.Select(i => i.Name).ToArray());
        }

        public BaseSyncItem SerializeOnlyChangedProperties(string[] changedProperties)
        {
            var syncItem = (BaseSyncItem)Activator.CreateInstance(GetSyncItemType());

            PopulateSyncItemCoreProperties(syncItem);

            // Properties on the sync item that were changed
            var properties = syncItem.GetType().GetRuntimeProperties().Where(i => changedProperties.Contains(i.Name)).ToArray();

            // Automatically populate the editable values. We know they're all already set since they were
            // loaded from the database.
            foreach (var val in _propertyValues)
            {
                foreach (var syncPropName in val.Key.AffectedSyncPropertyNames)
                {
                    var propToSet = properties.FirstOrDefault(i => i.Name.Equals(syncPropName));
                    if (propToSet != null)
                    {
                        propToSet.SetValue(syncItem, val.Value);
                    }
                }
            }

            return syncItem;
        }

        /// <summary>
        /// Populates the core properties that aren't editable.
        /// </summary>
        /// <param name="syncItem"></param>
        protected virtual void PopulateSyncItemCoreProperties(BaseSyncItem syncItem)
        {
            syncItem.Identifier = Identifier;
            syncItem.Updated = Updated;
            syncItem.DateCreated = DateCreated;
        }

        /// <summary>
        /// Offset has already been taken into consideration by the BaseItem.
        /// </summary>
        /// <param name="item"></param>
        public void Deserialize(BaseSyncItem item, List<string> changedProperties)
        {
            DeserializeCoreProperties(item);

            var properties = item.GetType().GetRuntimeProperties().ToArray();

            // Find all the data item properties
            var dataItemProperties = this.GetType().GetRuntimeFields().Where(i => i.IsStatic && i.FieldType == typeof(DataItemProperty)).Select(i => i.GetValue(null)).OfType<DataItemProperty>().ToArray();

            foreach (var dataProp in dataItemProperties)
            {
                foreach (var syncPropName in dataProp.AffectedSyncPropertyNames)
                {
                    var propToGet = properties.FirstOrDefault(i => i.Name.Equals(syncPropName));
                    if (propToGet != null)
                    {
                        object newVal = propToGet.GetValue(item);

                        // If tracking changed properties, and value has changed
                        if (changedProperties != null && !object.Equals(GetValue(dataProp), newVal))
                        {
                            // Add the property to tracked
                            changedProperties.Add(syncPropName);
                        }

                        // Set the new value
                        SetValue(dataProp, newVal);
                    }
                }
            }
        }

        protected virtual void DeserializeCoreProperties(BaseSyncItem item)
        {
            Updated = item.Updated;

            if (item.DateCreated != null)
                DateCreated = item.DateCreated.Value; //we only send up DateCreated when item is locally created, it can't change after that
        }

        public override int GetHashCode()
        {
            return Identifier.GetHashCode();
        }

        /// <summary>
        /// Some classes can implement this, so that when called it'll recalculate its own item
        /// </summary>
        public virtual void Calculate()
        {
            //nothing
        }


        public enum ChangeType { Add, Remove, Edited }


        public virtual int CompareTo(object obj)
        {
            if (obj is BaseDataItem)
                return CompareTo(obj as BaseDataItem);

            return 0;
        }

        /// <summary>
        /// Things with earlier creation dates go first
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual int CompareTo(BaseDataItem other)
        {
            return DateCreated.CompareTo(other.DateCreated);
        }

        //public BaseItemWin Find(Guid identifier)
        //{
        //    IEnumerable<BaseItemWin> children = GetChildren();

        //    //see if it's one of the immediate children
        //    foreach (BaseItemWin item in children)
        //        if (item.Identifier.Equals(identifier))
        //            return item;

        //    //otherwise have each child look for the item
        //    foreach (BaseItemWin item in children)
        //    {
        //        BaseItemWin found = item.Find(identifier);
        //        if (found != null)
        //            return found;
        //    }

        //    //otherwise not found
        //    return null;
        //}

        public bool Equals(BaseDataItem other)
        {
            return Identifier == other.Identifier;
        }
    }
}
