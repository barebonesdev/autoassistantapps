using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AutoAssistantAppDataLibrary.DataLayer
{
    [DataContract(Namespace = "")]
    public class ChangedPropertiesOfDataItem
    {
        private const string KEY_DELETED = "ChangedItemDeleted";
        private const string KEY_NEW = "ChangedItemNew";

        public enum ChangeType { New, Edited, Deleted }

        [DataMember]
        public Guid Identifier { get; set; }

        /// <summary>
        /// Key represents sync property name. Value represents whether we sent the change.
        /// </summary>
        [DataMember]
        public Dictionary<string, bool> _changedProperties = new Dictionary<string, bool>();


        public ChangeType Type
        {
            get
            {
                if (_changedProperties.ContainsKey(KEY_DELETED))
                    return ChangeType.Deleted;

                if (_changedProperties.ContainsKey(KEY_NEW))
                    return ChangeType.New;

                return ChangeType.Edited;
            }
        }

        public bool IsEmpty()
        {
            return _changedProperties.Count == 0;
        }

        public void SetNew()
        {
            _changedProperties.Clear();
            _changedProperties[KEY_NEW] = false;
        }

        public void SetEdited(IEnumerable<string> changedProperties)
        {
            // If it was deleted
            if (Type == ChangeType.Deleted)
            {
                // Remove that deleted flag
                _changedProperties.Remove(KEY_DELETED);
            }

            // If adding nothing, do nothing
            if (!changedProperties.Any())
                return;

            // If already has All, we still need to write these properties since All might currently be syncing and finish, but these edited properties need to sync afterwards

            // Cannot add deleted property through this method
            if (changedProperties.Contains(KEY_DELETED))
                throw new ArgumentException("The Deleted property cannot be added through here");

            // Cannot add All property through this method
            if (changedProperties.Contains(KEY_NEW))
                throw new ArgumentException("The All property cannot be added through here");


            // Add each one
            foreach (var p in changedProperties)
                _changedProperties[p] = false;
        }

        /// <summary>
        /// Returns true if change was actually made. Returns false if already deleted and thus no change made.
        /// </summary>
        /// <returns></returns>
        public void SetDeleted()
        {
            _changedProperties.Clear();
            _changedProperties[KEY_DELETED] = false;
        }

        /// <summary>
        /// Returns true if made changes, otherwise false
        /// </summary>
        /// <returns></returns>
        internal bool ClearSyncing()
        {
            string[] toClear = _changedProperties.Where(i => i.Value).Select(i => i.Key).ToArray();

            foreach (var p in toClear)
                _changedProperties.Remove(p);

            return toClear.Length > 0;
        }

        internal bool NeedsClearSyncing()
        {
            return _changedProperties.Any(i => i.Value);
        }

        /// <summary>
        /// Marks all values as sent
        /// </summary>
        internal void MarkSent()
        {
            var toSetAsSent = _changedProperties.Where(i => !i.Value).Select(i => i.Key).ToArray();

            foreach (var p in toSetAsSent)
                _changedProperties[p] = true;
        }

        public string[] GetEditedProperties()
        {
            return _changedProperties.Keys.ToArray();
        }
    }
}
