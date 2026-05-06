using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using AutoAssistantAppDataLibrary.ViewItems;
using ToolsPortable;
using AutoAssistantAppDataLibrary.ViewItemsGroup;
using Vx.Views;
using System.Drawing;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Garage
{
    public class GarageViewModel : PopupComponentViewModel
    {
        private GarageViewItemsGroup _garageViewItemsGroup;
        public MyObservableList<ViewItemVehicle> Vehicles { get; private set; }

        public GarageViewModel(MainScreenViewModel parent) : base(parent)
        {
            Title = "Garage";
            _ = LoadAsync();
        }

        protected override async Task LoadAsyncOverride()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("GarageViewModel: Starting LoadAsyncOverride");
                _garageViewItemsGroup = await GarageViewItemsGroup.LoadAsync(FindAncestor<MainScreenViewModel>().CurrentLocalAccountId);
                System.Diagnostics.Debug.WriteLine($"GarageViewModel: Loaded, Vehicles count = {_garageViewItemsGroup?.Vehicles?.Count}");
                Vehicles = _garageViewItemsGroup.Vehicles;
                OnPropertyChanged(nameof(Vehicles));
                System.Diagnostics.Debug.WriteLine("GarageViewModel: OnPropertyChanged called");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GarageViewModel ERROR: {ex}");
                throw;
            }
        }

        public void AddVehicle()
        {
            ShowPopup(AddVehicleViewModel.CreateForAdd(FindAncestor<MainScreenViewModel>()));
        }

        public async Task OpenVehicle(ViewItemVehicle vehicle)
        {
            await FindAncestor<MainScreenViewModel>().SetCurrentVehicleAsync(vehicle.Identifier);
            RemoveViewModel();
        }

        protected override View Render()
        {
            if (Vehicles == null)
            {
                return new Border();
            }

            if (Vehicles.Count == 0)
            {
                return RenderGenericPopupContent(

                    new LinearLayout
                    {
                        Orientation = Orientation.Vertical,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Your garage is empty. Add a vehicle to get started!",
                                TextAlignment = HorizontalAlignment.Center
                            },

                            new AccentButton
                            {
                                Text = "Add vehicle",
                                Margin = new Thickness(0, 12, 0, 0),
                                Click = AddVehicle
                            }
                        }
                    }

                );
            }

            return new LinearLayout
            {
                Orientation = Orientation.Vertical,
                Children =
                {
                    new ListView
                    {
                        Items = Vehicles,
                        ItemTemplate = VehicleListItemView
                    }.LinearLayoutWeight(1),

                    new AccentButton
                    {
                        Text = "Add vehicle",
                        Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, 12, Theme.Current.PageMargin + NookInsets.Right, Theme.Current.PageMargin + NookInsets.Bottom),
                        Click = AddVehicle
                    }
                }
            };
        }

        private View VehicleListItemView(object obj)
        {
            var vehicle = (ViewItemVehicle)obj;

            vehicle.StartInitializeUpcomingMaintenance(FindAncestor<MainScreenViewModel>());

            return new LinearLayout
            {
                Orientation = Orientation.Vertical,
                BackgroundColor = Color.FromArgb(24, 0, 0, 0),
                Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, 12, Theme.Current.PageMargin + NookInsets.Right, 12),
                Children =
                {
                    new LinearLayout
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(12),
                        Children =
                        {
                            new TextBlock
                            {
                                Text = vehicle.Nickname,
                                WrapText = false,
                                FontSize = 20
                            }.LinearLayoutWeight(1),

                            new Border
                            {
                                CornerRadius = 4,
                                BackgroundColor = vehicle.MaintenanceStatusType == ViewItemVehicle.MaintenanceStatus.Overdue ? Color.Red : Color.LightGray,
                                Content = new TextBlock
                                {
                                    Text = vehicle.MaintenanceStatusText,
                                    TextColor = vehicle.MaintenanceStatusType == ViewItemVehicle.MaintenanceStatus.Overdue ? Color.White : Theme.Current.ForegroundColor,
                                    WrapText = false
                                },
                                VerticalAlignment = VerticalAlignment.Center,
                                Padding = new Thickness(6, 4, 6, 4)
                            }
                        }
                    },

                    !string.IsNullOrWhiteSpace(vehicle.YearMakeModelString) ? new TextBlock
                    {
                        Text = vehicle.YearMakeModelString,
                        Margin = new Thickness(12, 12, 12, 0)
                    } : null,

                    new Button
                    {
                        Text = "Select vehicle",
                        Margin = new Thickness(12, 12, 12, 12),
                        Click = () => _ = OpenVehicle(vehicle)
                    }
                }
            };
        }
    }
}
