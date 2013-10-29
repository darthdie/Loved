﻿using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ThomasJaworski.ComponentModel {
    public abstract class ChangeListener : INotifyPropertyChanged, IDisposable {
        #region *** Members ***
        protected string _propertyName;
        #endregion


        #region *** Abstract Members ***
        protected abstract void Unsubscribe();
        #endregion


        #region *** INotifyPropertyChanged Members and Invoker ***
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(object sender, string propertyName) {
            var temp = PropertyChanged;
            if (temp != null)
                temp(sender ?? this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        #region *** Disposable Pattern ***

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                Unsubscribe();
            }
        }

        ~ChangeListener() {
            Dispose(false);
        }

        #endregion


        #region *** Factory ***
        public static ChangeListener Create(INotifyPropertyChanged value) {
            return Create(value, null);
        }

        public static ChangeListener Create(INotifyPropertyChanged value, string propertyName) {
            if (value is INotifyCollectionChanged) {
                return new CollectionChangeListener(value as INotifyCollectionChanged, propertyName);
            }
            else if (value is INotifyPropertyChanged) {
                return new ChildChangeListener(value as INotifyPropertyChanged, propertyName);
            }
            else
                return null;
        }
        #endregion
    }
}