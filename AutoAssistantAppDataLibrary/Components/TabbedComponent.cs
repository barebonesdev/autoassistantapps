using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Vx.Views;

namespace AutoAssistantAppDataLibrary.Components
{
    public class TabItem
    {
        public string Title { get; set; }
        public Func<Thickness, View> RenderContent { get; set; }
    }

    public class TabbedComponent : VxComponent
    {
        public TabItem[] Tabs { get; set; }

        private TabItem _selectedTab;

        private bool _isCompact = true;

        protected override void Initialize()
        {
            _selectedTab = Tabs?.Length > 0 ? Tabs[0] : null;
        }

        public override bool DelayFirstRenderTillSizePresent => true;

        protected override void OnSizeChanged(SizeF size, SizeF previousSize)
        {
            if (size.Width < 1000)
            {
                if (!_isCompact)
                {
                    _isCompact = true;
                    MarkDirty();
                }
            }
            else
            {
                if (_isCompact)
                {
                    _isCompact = false;
                    MarkDirty();
                }
            }
        }

        private View RenderCompactTab(TabItem tab)
        {
            bool isSelected = tab.Title == _selectedTab?.Title;

            return new TransparentContentButton
            {
                Content = new FrameLayout
                {
                    Children =
                    {
                        isSelected ? new Border
                        {
                            BackgroundColor = Theme.Current.AccentColor,
                            Height = 3,
                            VerticalAlignment = VerticalAlignment.Bottom,
                        } : null,

                        new TextBlock
                        {
                            Text = tab.Title,
                            FontWeight = isSelected ? FontWeights.Bold : FontWeights.Normal,
                            TextColor = isSelected ? Theme.Current.ForegroundColor : Theme.Current.SubtleForegroundColor,
                            TextAlignment = HorizontalAlignment.Center,
                            WrapText = false,
                            Margin = new Thickness(0, 12, 0, 12)
                        }
                    }
                },
                Click = () =>
                {
                    _selectedTab = tab;
                    MarkDirty();
                }
            }.LinearLayoutWeight(1);
        }

        private View RenderCompact()
        {
            View content = null;
            Thickness innerNookInsets = new Thickness(NookInsets.Left, 0, NookInsets.Right, 0);
            if (_selectedTab != null && _selectedTab.RenderContent != null)
            {
                content = _selectedTab.RenderContent(innerNookInsets);
            }
            if (content == null)
            {
                content = new Border();
            }

            var tabItems = new LinearLayout
            {
                Orientation = Orientation.Horizontal,
                BackgroundColor = Theme.Current.BackgroundAlt1Color
            };
            foreach (var tab in Tabs)
            {
                tabItems.Children.Add(RenderCompactTab(tab));
            }

            return new LinearLayout
            {
                Children =
                {
                    content.LinearLayoutWeight(1),

                    tabItems,

                    new Border
                    {
                        BackgroundColor = Theme.Current.SubtleForegroundColor,
                        Height = 1
                    }
                }
            };
        }

        private View RenderFull()
        {
            var layout = new LinearLayout
            {
                Orientation = Orientation.Horizontal
            };

            if (Tabs != null)
            {
                for (int i = 0; i < Tabs.Length; i++)
                {
                    var tab = Tabs[i];
                    var innerNookInsets = new Thickness(i == 0 ? NookInsets.Left : 0, 0, i == Tabs.Length - 1 ? NookInsets.Right : 0, 0);
                    var content = tab.RenderContent?.Invoke(innerNookInsets) ?? new Border();
                    content.LinearLayoutWeight(1);
                    layout.Children.Add(content);
                }
            }

            return layout;
        }

        protected override View Render()
        {
            return _isCompact ? RenderCompact() : RenderFull();
        }
    }
}
