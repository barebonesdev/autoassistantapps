using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using AutoAssistantAppDataLibrary.ViewItems;
using AutoAssistantAppDataLibrary.ViewItemsGroup;
using ToolsPortable;
using ToolsPortable.Indexing;
using System.Threading;
using AutoAssistantAppDataLibrary.Extensions;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Maintenance
{
    public class SearchMaintenanceRecordsViewModel : BaseMainScreenViewModelChild
    {
        public ViewItemVehicle Vehicle { get; private set; }

        public MyObservableList<ViewItemMaintenanceRecordEntry> SearchResults { get; private set; } = new MyObservableList<ViewItemMaintenanceRecordEntry>();

        private MyIndexer<ViewItemMaintenanceRecordEntry> _indexer = new MyIndexer<ViewItemMaintenanceRecordEntry>();

        private string _searchText = "";
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                UpdateResults();
            }
        }

        private VehicleViewItemsGroup _vehicleViewItemsGroup;

        public SearchMaintenanceRecordsViewModel(MainScreenViewModel parent, ViewItemVehicle vehicle) : base(parent)
        {
            Vehicle = vehicle;
        }

        protected override async Task LoadAsyncOverride()
        {
            await base.LoadAsyncOverride();

            _vehicleViewItemsGroup = await VehicleViewItemsGroup.LoadAsync(Vehicle);
            _vehicleViewItemsGroup.OnChangesMade += new WeakEventHandler<EventArgs>(_vehicleViewItemsGroup_OnChangesMade).Handler;
            OnDataChanged();
        }

        private void OnDataChanged()
        {
            _indexer = new MyIndexer<ViewItemMaintenanceRecordEntry>();
            foreach (var r in _vehicleViewItemsGroup.MaintenanceRecords)
            {
                _indexer.Index(r.Title, r, importance: 10);
                _indexer.Index(r.Details, r, importance: 9);
                _indexer.Index(r.Subtitle, r, importance: 8);
                _indexer.Index(r.DoneBy, r, importance: 7);

                foreach (var servicePerformed in r.ServicesPerformed)
                {
                    _indexer.Index(servicePerformed.Title, r, importance: 6);
                }
            }

            UpdateResults();
        }

        private void _vehicleViewItemsGroup_OnChangesMade(object sender, EventArgs e)
        {
            OnDataChanged();
        }

        private CancellationTokenSource _cancellationTokenSource;
        private string _searchingString;
        private async void UpdateResults()
        {
            if (_indexer == null)
            {
                return;
            }

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;

            List<ViewItemMaintenanceRecordEntry> results = null;

            await Task.Delay(300);
            await Task.Run(delegate
            {
                try
                {
                    lock (_indexer)
                    {
                        string newSearchString = SearchText;
                        if (_searchingString == newSearchString)
                        {
                            return;
                        }
                        _searchingString = newSearchString;

                        _cancellationTokenSource?.Cancel();
                        _cancellationTokenSource = null;

                        if (string.IsNullOrWhiteSpace(newSearchString))
                        {
                            results = _vehicleViewItemsGroup.MaintenanceRecords.ToList();
                            return;
                        }

                        _cancellationTokenSource = new CancellationTokenSource();

                        try
                        {
                            results = _indexer.GetMatches(newSearchString, int.MaxValue, _cancellationTokenSource.Token);
                            results.Sort();
                        }
                        catch (OperationCanceledException)
                        {
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    TelemetryExtension.Current?.TrackException(ex);
                }
            });

            if (results != null)
            {
                SearchResults.MakeListLike(results);
            }
        }

        public void ViewMaintenanceRecord(ViewItemMaintenanceRecordEntry entry)
        {
            MainScreenViewModel.ShowPopup(new ViewMaintenanceRecordViewModel(MainScreenViewModel, entry));
        }
    }
}
