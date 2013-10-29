﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Loved;

namespace ThomasJaworski.ComponentModel {
    public class ChildChangeListener : ChangeListener {
        #region *** Members ***
        protected static readonly Type _inotifyType = typeof(INotifyPropertyChanged);

        private readonly INotifyPropertyChanged _value;
        private readonly Type _type;
        private readonly Dictionary<string, ChangeListener> _childListeners = new Dictionary<string, ChangeListener>();
        #endregion


        #region *** Constructors ***
        public ChildChangeListener(INotifyPropertyChanged instance) {
            if (instance == null)
                throw new ArgumentNullException("instance");

            _value = instance;
            _type = _value.GetType();

            Subscribe();
        }

        public ChildChangeListener(INotifyPropertyChanged instance, string propertyName)
            : this(instance) {
            _propertyName = propertyName;
        }
        #endregion


        #region *** Private Methods ***
        private void Subscribe() {
            _value.PropertyChanged += new PropertyChangedEventHandler(value_PropertyChanged);

            var query =
                from property
                in _type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                where _inotifyType.IsAssignableFrom(property.PropertyType)
                select property;

            foreach (var property in query) {
                // Declare property as known "Child", then register it
                _childListeners.Add(property.Name, null);
                ResetChildListener(property.Name);
            }
        }


        /// <summary>
        /// Resets known (must exist in children collection) child event handlers
        /// </summary>
        /// <param name="propertyName">Name of known child property</param>
        private void ResetChildListener(string propertyName) {
            if (_childListeners.ContainsKey(propertyName)) {
                // Unsubscribe if existing
                if (_childListeners[propertyName] != null) {
                    _childListeners[propertyName].PropertyChanged -= new PropertyChangedEventHandler(child_PropertyChanged);

                    // Should unsubscribe all events
                    _childListeners[propertyName].Dispose();
                    _childListeners[propertyName] = null;
                }

                var property = _type.GetProperty(propertyName);
                if (property == null)
                    throw new InvalidOperationException(string.Format("Was unable to get '{0}' property information from Type '{1}'", propertyName, _type.Name));

                var at = property.GetCustomAttributes();
                if (property.GetCustomAttribute<ChangeListenerIgnoreAttribute>(true) != null) {
                    return;
                }

                object newValue = property.GetValue(_value, null);

                // Only recreate if there is a new value
                if (newValue != null) {
                    if (newValue is INotifyCollectionChanged) {
                        _childListeners[propertyName] =
                            new CollectionChangeListener(newValue as INotifyCollectionChanged, propertyName);
                    }
                    else if (newValue is INotifyPropertyChanged) {
                        _childListeners[propertyName] =
                            new ChildChangeListener(newValue as INotifyPropertyChanged, propertyName);
                    }

                    if (_childListeners[propertyName] != null)
                        _childListeners[propertyName].PropertyChanged += new PropertyChangedEventHandler(child_PropertyChanged);
                }
            }
        }
        #endregion


        #region *** Event Handler ***
        void child_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            RaisePropertyChanged(sender, e.PropertyName);
        }

        void value_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            // First, reset child on change, if required...
            ResetChildListener(e.PropertyName);

            // ...then, notify about it
            RaisePropertyChanged(sender, e.PropertyName);
        }

        protected override void RaisePropertyChanged(object sender, string propertyName) {
            // Special Formatting
            base.RaisePropertyChanged(sender, string.Format("{0}{1}{2}",
                _propertyName, _propertyName != null ? "." : null, propertyName));
        }
        #endregion


        #region *** Overrides ***
        /// <summary>
        /// Release all child handlers and self handler
        /// </summary>
        protected override void Unsubscribe() {
            _value.PropertyChanged -= new PropertyChangedEventHandler(value_PropertyChanged);

            foreach (var binderKey in _childListeners.Keys) {
                if (_childListeners[binderKey] != null)
                    _childListeners[binderKey].Dispose();
            }

            _childListeners.Clear();

            System.Diagnostics.Debug.WriteLine("ChildChangeListener '{0}' unsubscribed", _propertyName);
        }
        #endregion
    }
}