/********************************************************************************************
        Author: Sergey Stoyan
        sergey.stoyan@gmail.com
        http://www.cliversoft.com
********************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using System.IO;
using LiteDB;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;

namespace Cliver.Probidder
{
    public interface IView : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        int Id { get; }

        void ValidateAllProperties();
        void OnPropertyChanged(string propertyName);
        bool Edited { get; }
    }

    public partial class View<D> : IView where D: Db.Document, new()
    {
        public View()
        {
            Model = new D();
            set_new_model();
        }

        public View(D d)
        {
            if (d != null)
                Model = d;
            else
            {
                Model = new D();
                set_new_model();
            }
        }

        virtual protected void set_new_model()
        {
        }
        protected  readonly D Model;

        public int Id { get { return Model.Id; } }
        
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void ValidateAllProperties()
        {
            forced_validation = true;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(null));
            forced_validation = false;
        }

      protected  void check(string property, string error)
        {
            if (!edited && !forced_validation)
                return;
            string e0 = null;
            if (columnNames2error.TryGetValue(property, out e0))
                InitialControlSetting = false;
            columnNames2error[property] = error;
            if (e0 != error)
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(property));
        }
        protected readonly Dictionary<string, string> columnNames2error = new Dictionary<string, string>();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public bool InitialControlSetting = true;

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return null;
            return columnNames2error.Where(x => x.Key == propertyName && x.Value != null).Select(x => x.Key);
        }
        bool forced_validation = false;

        public bool HasErrors
        {
            get
            {
                return columnNames2error.Where(x => x.Value != null).Select(x => x.Key).FirstOrDefault() != null;
            }
        }

        public bool Edited
        {
            get
            {
                //return columnNames2error.Select(x => x.Key).FirstOrDefault() != null;
                return edited;
            }
        }
        protected  bool edited;
    }
}