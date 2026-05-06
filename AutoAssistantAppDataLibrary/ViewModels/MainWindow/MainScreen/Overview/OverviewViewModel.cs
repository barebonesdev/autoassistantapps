using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using AutoAssistantAppDataLibrary.ViewItems;
using AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Garage;
using AutoAssistantAppDataLibrary.Extensions;
using Vx.Views;
using AutoAssistantAppDataLibrary.App;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Overview
{
    public class OverviewViewModel : BaseMainScreenViewModelChild
    {
        public ViewItemVehicle Vehicle { get; private set; }

        public OverviewViewModel(MainScreenViewModel parent, ViewItemVehicle vehicle) : base(parent)
        {
            Vehicle = vehicle;
        }

        public void Edit()
        {
            MainScreenViewModel.ShowPopup(AddVehicleViewModel.CreateForEdit(MainScreenViewModel, Vehicle));
        }

        public async void Delete()
        {
            try
            {
                if (await AutoAssistantApp.ConfirmDeleteAsync("Are you sure you want to delete this vehicle and all of its records?", "Delete vehicle?", useConfirmationCheckbox: true))
                {
                    await MainScreenViewModel.DeleteVehicle(Vehicle.Identifier);
                }
            }
            catch (Exception ex)
            {
                TelemetryExtension.Current?.TrackException(ex);
            }
        }

        private View RenderKeyValue(string title, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            return new LinearLayout
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new TextBlock
                    {
                        Text = title + ":",
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Top,
                        WrapText = false
                    },

                    new TextBlock
                    {
                        Text = content,
                        Margin = new Thickness(6, 0, 0, 0),
                        IsTextSelectionEnabled = true
                    }.LinearLayoutWeight(1)
                }
            };
        }

        protected override View Render()
        {
            return new ScrollView
            {
                Content = new LinearLayout
                {
                    Margin = new Thickness(Theme.Current.PageMargin),
                    Children =
                    {
                        new LinearLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = Vehicle.Nickname,
                                    WrapText = false
                                }.TitleStyle().LinearLayoutWeight(1),

                                new TransparentContentButton
                                {
                                    Content = new FontIcon
                                    {
                                        Glyph = MaterialDesign.MaterialDesignIcons.Edit,
                                        FontSize = 20,
                                        Margin = new Thickness(6)
                                    },
                                    Click = Edit
                                },

                                new TransparentContentButton
                                {
                                    Content = new FontIcon
                                    {
                                        Glyph = MaterialDesign.MaterialDesignIcons.Delete,
                                        FontSize = 20,
                                        Margin = new Thickness(6)
                                    },
                                    Click = Delete
                                }
                            }
                        },

                        new TextBlock
                        {
                            Text = Vehicle.YearMakeModelString,
                            Margin = new Thickness(0, 12, 0, 0),
                            IsTextSelectionEnabled = true
                        },

                        RenderKeyValue("Initial mileage", Vehicle.InitialMileage.ToString()),

                        RenderKeyValue("Purchased for", Vehicle.AmountPurchasedFor),

                        RenderKeyValue("Purchased from", Vehicle.PurchasedFrom),

                        RenderKeyValue("License plate", Vehicle.LicensePlate),

                        RenderKeyValue("VIN", Vehicle.VIN),

                        new TextBlock
                        {
                            Text = Vehicle.Notes,
                            IsTextSelectionEnabled = true,
                            Margin = new Thickness(0, 12, 0, 0)
                        }
                    }
                }
            };
        }
    }
}
