using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BareMvvm.Core.ViewModels;
using Vx.Views;

namespace AutoAssistantAppDataLibrary.ViewModels.MainWindow.MainScreen.Fuel
{
    public class ImportFuelIntroViewModel : PopupComponentViewModel
    {
        public ImportFuelIntroViewModel(BaseViewModel parent) : base(parent)
        {
            Title = "Import fuel records";
        }

        public void Next()
        {
            ShowPopup(new ImportFuelSelectCsvViewModel(Parent));
        }

        protected override View Render()
        {
            return new LinearLayout
            {
                Children =
                {
                    new ScrollView
                    {
                        Content = new LinearLayout
                        {
                            Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, Theme.Current.PageMargin + NookInsets.Top, Theme.Current.PageMargin + NookInsets.Right, 0),
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = "You can import fuel records into Auto Assistant by providing a CSV file. The records can be added in addition to your current records, or they can replace your existing records."
                                },

                                new TextBlock
                                {
                                    Text = "Provide these columns in EXACTLY THIS ORDER...",
                                    Margin = new Thickness(0, 12, 0, 0)
                                },

                                new TextBlock
                                {
                                    Text = "Odometer",
                                    Margin = new Thickness(0, 12, 0, 0),
                                    FontSize = Theme.Current.CaptionFontSize
                                },

                                new TextBlock
                                {
                                    Text = "[decimal] The odometer reading from your vehicle when you filled up. Must be a number equal to or greater than 0, like 173,450. Do not include units.",
                                    FontSize = Theme.Current.CaptionFontSize,
                                    TextColor = Theme.Current.SubtleForegroundColor
                                },

                                new TextBlock
                                {
                                    Text = "Cost per gallon",
                                    Margin = new Thickness(0, 12, 0, 0),
                                    FontSize = Theme.Current.CaptionFontSize
                                },

                                new TextBlock
                                {
                                    Text = "[decimal] How much the fuel costed per gallon. Do not include units. Values can be left BLANK.",
                                    FontSize = Theme.Current.CaptionFontSize,
                                    TextColor = Theme.Current.SubtleForegroundColor
                                },

                                new TextBlock
                                {
                                    Text = "Gallons",
                                    Margin = new Thickness(0, 12, 0, 0),
                                    FontSize = Theme.Current.CaptionFontSize
                                },

                                new TextBlock
                                {
                                    Text = "[decimal] The number of gallons of fuel. Must be a number equal to or greater than 0, like 13.54. Do not include units.",
                                    FontSize = Theme.Current.CaptionFontSize,
                                    TextColor = Theme.Current.SubtleForegroundColor
                                },

                                new TextBlock
                                {
                                    Text = "Date",
                                    Margin = new Thickness(0, 12, 0, 0),
                                    FontSize = Theme.Current.CaptionFontSize
                                },

                                new TextBlock
                                {
                                    Text = "[DateTime] The date and time of fillup. Values can be left BLANK, and will be auto-filled with today's date. Value should be formatted like '3/24/2015 5:37:00 PM', but other date formats might work too.",
                                    FontSize = Theme.Current.CaptionFontSize,
                                    TextColor = Theme.Current.SubtleForegroundColor
                                },

                                new TextBlock
                                {
                                    Text = "Store name",
                                    Margin = new Thickness(0, 12, 0, 0),
                                    FontSize = Theme.Current.CaptionFontSize
                                },

                                new TextBlock
                                {
                                    Text = "[string] The name of the store you filled up at. Values can be left BLANK.",
                                    FontSize = Theme.Current.CaptionFontSize,
                                    TextColor = Theme.Current.SubtleForegroundColor
                                },

                                new TextBlock
                                {
                                    Text = "Fuel type",
                                    Margin = new Thickness(0, 12, 0, 0),
                                    FontSize = Theme.Current.CaptionFontSize
                                },

                                new TextBlock
                                {
                                    Text = "[enum] Type of fuel. Can only be these exact values: { None, Oct87, Oct89, Oct91, Diesel }. Values can also be left BLANK, and None will automatically be selected.",
                                    FontSize = Theme.Current.CaptionFontSize,
                                    TextColor = Theme.Current.SubtleForegroundColor
                                },

                                new TextBlock
                                {
                                    Text = "Partial fillup",
                                    Margin = new Thickness(0, 12, 0, 0),
                                    FontSize = Theme.Current.CaptionFontSize
                                },

                                new TextBlock
                                {
                                    Text = "[boolean] If you didn't fill the tank all the way to the top, this value should be 'True'. Otherwise, leave it BLANK or 'False'.",
                                    FontSize = Theme.Current.CaptionFontSize,
                                    TextColor = Theme.Current.SubtleForegroundColor
                                },

                                new TextBlock
                                {
                                    Text = "Skipped previous",
                                    Margin = new Thickness(0, 12, 0, 0),
                                    FontSize = Theme.Current.CaptionFontSize
                                },

                                new TextBlock
                                {
                                    Text = "[boolean] If you didn't record a previous fillup, this value should be 'True'. Otherwise, leave it BLANK or 'False'.",
                                    FontSize = Theme.Current.CaptionFontSize,
                                    TextColor = Theme.Current.SubtleForegroundColor
                                },

                                new TextBlock
                                {
                                    Text = "Notes",
                                    Margin = new Thickness(0, 12, 0, 0),
                                    FontSize = Theme.Current.CaptionFontSize
                                },

                                new TextBlock
                                {
                                    Text = "[string] Any notes about the fillup. Values can be left BLANK.",
                                    FontSize = Theme.Current.CaptionFontSize,
                                    TextColor = Theme.Current.SubtleForegroundColor
                                },

                                new TextBlock
                                {
                                    Text = "Here is a valid CSV file example...",
                                    Margin = new Thickness(0, 12, 0, 0)
                                },

                                new TextBlock
                                {
                                    Text = "Odometer,Cost per gallon,Gallons,Date,Store name,Fuel type,Partial fillup,Skipped previous,Notes\r\n173533,2.299,14.641,12/14/2014 4:17:19 PM,,Oct87,False,False,\r\n173240,2.379,8.402,12/13/2014 3:51:48 PM,,Oct87,False,False,",
                                    Margin = new Thickness(0, 12, 0, 0),
                                    FontSize = Theme.Current.CaptionFontSize,
                                    TextColor = Theme.Current.SubtleForegroundColor,
                                    IsTextSelectionEnabled = true
                                },
                            }
                        }
                    }.LinearLayoutWeight(1),

                    new AccentButton
                    {
                        Margin = new Thickness(Theme.Current.PageMargin + NookInsets.Left, Theme.Current.PageMargin, Theme.Current.PageMargin + NookInsets.Right, Theme.Current.PageMargin + NookInsets.Bottom),
                        Text = "Next",
                        Click = Next
                    }
                }
            };
        }
    }
}
