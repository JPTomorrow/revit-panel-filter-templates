using System.ComponentModel;
using System.Runtime.CompilerServices;
using JPMorrow.Revit.Panels;

namespace JPMorrow.UI.ViewModels
{
    public partial class ParentViewModel
    {
        /*
        public class PanelFilterPresenter : Presenter
        {
            public PanelFilter Value;
            
            public PanelFilterPresenter(PanelFilter value)
            {
                Value = value;
                RefreshDisplay();
            }

            public void RefreshDisplay()
            {
                PanelName = Value.PanelName;
            }

            private string name;
            public string PanelName {get => name;
            set {
                name = value;
                Update("Filter_Items");
            }}

            //Item Selection Bindings
            private bool _isSelected;
            public bool IsSelected { get => _isSelected;
                set {
                    _isSelected = value;
                    Update("Filter_Items");
            }}
        }

        public class ViewFilterGroupPresenter : Presenter
        {
            public ViewFilterGroup Value;
            public ViewFilterGroupPresenter(ViewFilterGroup value)
            {
                Value = value;
                RefreshDisplay();
            }

            public void RefreshDisplay()
            {
                ViewName = Value.ViewName;
            }

            private string name;
            public string ViewName {get => name;
            set {
                name = value;
                Update("View_Items");
            }}

            //Item Selection Bindings
            private bool _isSelected;
            public bool IsSelected { get => _isSelected;
                set {
                    _isSelected = value;
                    Update("View_Items");
            }}
        }
        */
    }

    /// <summary>
    /// Default Presenter: Just Presents a string value as a listbox item,
    /// can replace with an object for more complex listbox databindings
    /// </summary>
    public class ItemPresenter : Presenter
    {
        private readonly string _value;
        public ItemPresenter(string value) => _value = value;
    }

    #region Inherited Classes
    public abstract class Presenter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void Update(string val) => RaisePropertyChanged(val);

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    #endregion
}
