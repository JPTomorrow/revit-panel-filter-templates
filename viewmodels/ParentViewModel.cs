using System.Windows.Input;
using System.Windows;
using JPMorrow.Revit.Documents;
using JPMorrow.Views.RelayCmd;
using om = System.Collections.ObjectModel;
using System.Windows.Controls;

namespace JPMorrow.UI.ViewModels
{
    // observable collection aliases
    //using ObsPanelFilter = om.ObservableCollection<ParentViewModel.PanelFilterPresenter>;
    //using ObsViewGroup = om.ObservableCollection<ParentViewModel.ViewFilterGroupPresenter>;

    public partial class ParentViewModel : Presenter {
        
        private static ModelInfo Info { get; set; }
        
        // public ObsPanelFilter Filter_Items { get; set; } = new ObsPanelFilter();
        // public ObsViewGroup View_Items { get; set; } = new ObsViewGroup();

        // public ICommand RefreshPanelsCmd => new RelayCommand<Window>(RefreshPanels);
        // public ICommand MasterCloseCmd => new RelayCommand<Window>(MasterClose);
        // public ICommand FilterSelChangedCmd => new RelayCommand<Window>(FilterSelChanged);
        // public ICommand ViewSelChangedCmd => new RelayCommand<Window>(ViewSelChanged);
        
        // public ICommand SelectViewsCmd => new RelayCommand<DataGrid>(SelectViews);
        // public ICommand DeselectViewsCmd => new RelayCommand<DataGrid>(DeselectViews);

        public ParentViewModel(ModelInfo info)
        {
            //revit documents and pre converted elements
            Info = info;

            // RefreshPanels(null);
        }
    }
}
