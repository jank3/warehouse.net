﻿using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.ViewModel;

namespace Warehouse.Silverlight.MainModule.Infrastructure
{
    public abstract class InteractionRequestViewModel : Confirmation, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void RaisePropertyChanged<T>(Expression<Func<T>> lambda)
        {
            var name = PropertySupport.ExtractPropertyName<T>(lambda);
            OnPropertyChanged(name);
        }
    }
}
