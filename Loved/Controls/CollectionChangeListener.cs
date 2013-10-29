using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ThomasJaworski.ComponentModel {
    public class CollectionChangeListener : ChangeListener {
        #region *** Members ***
        private readonly INotifyCollectionChanged _value;
        private readonly Dictionary<INotifyPropertyChanged, ChangeListener> _collectionListeners = new Dictionary<INotifyPropertyChanged, ChangeListener>();
        #endregion


        #region *** Constructors ***
        public CollectionChangeListener(INotifyCollectionChanged collection, string propertyName) {
            _value = collection;
            _propertyName = propertyName;

            Subscribe();
        }
        #endregion


        #region *** Private Methods ***
        private void Subscribe() {
            _value.CollectionChanged += new NotifyCollectionChangedEventHandler(value_CollectionChanged);

            foreach (INotifyPropertyChanged item in (IEnumerable)_value) {
                ResetChildListener(item);
            }
        }

        private void ResetChildListener(INotifyPropertyChanged item) {
            if (item == null)
                throw new ArgumentNullException("item");

            RemoveItem(item);

            ChangeListener listener = null;

            // Add new
            if (item is INotifyCollectionChanged)
                listener = new CollectionChangeListener(item as INotifyCollectionChanged, _propertyName);
            else
                listener = new ChildChangeListener(item as INotifyPropertyChanged);

            listener.PropertyChanged += new PropertyChangedEventHandler(listener_PropertyChanged);
            _collectionListeners.Add(item, listener);
        }

        private void RemoveItem(INotifyPropertyChanged item) {
            // Remove old
            if (_collectionListeners.ContainsKey(item)) {
                _collectionListeners[item].PropertyChanged -= new PropertyChangedEventHandler(listener_PropertyChanged);

                _collectionListeners[item].Dispose();
                _collectionListeners.Remove(item);
            }
        }


        private void ClearCollection() {
            foreach (var key in _collectionListeners.Keys) {
                _collectionListeners[key].Dispose();
            }

            _collectionListeners.Clear();
        }
        #endregion


        #region *** Event handlers ***
        void value_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.Action == NotifyCollectionChangedAction.Reset) {
                ClearCollection();
            }
            else {
                // Don't care about e.Action, if there are old items, Remove them...
                if (e.OldItems != null) {
                    foreach (INotifyPropertyChanged item in (IEnumerable)e.OldItems)
                        RemoveItem(item);
                }

                // ...add new items as well
                if (e.NewItems != null) {
                    foreach (INotifyPropertyChanged item in (IEnumerable)e.NewItems)
                        ResetChildListener(item);
                }
            }
        }


        void listener_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            // ...then, notify about it
            RaisePropertyChanged(sender, string.Format("{0}{1}{2}",
                _propertyName, _propertyName != null ? "[]." : null, e.PropertyName));
        }
        #endregion


        #region *** Overrides ***
        /// <summary>
        /// Releases all collection item handlers and self handler
        /// </summary>
        protected override void Unsubscribe() {
            ClearCollection();

            _value.CollectionChanged -= new NotifyCollectionChangedEventHandler(value_CollectionChanged);

            System.Diagnostics.Debug.WriteLine("CollectionChangeListener unsubscribed");
        }
        #endregion
    }
}