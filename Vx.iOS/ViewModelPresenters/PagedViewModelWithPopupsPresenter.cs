using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BareMvvm.Core.ViewModels;
using Foundation;
using UIKit;
using System.Collections.Specialized;
using ToolsPortable;
using System.Threading.Tasks;
using System.ComponentModel;

namespace InterfacesiOS.ViewModelPresenters
{
    public class PagedViewModelWithPopupsPresenter : PagedViewModelPresenter
    {
        private bool _destroyed = false;

        /// <summary>
        /// Tracks each presented popup modal: the ViewModel and its UIViewController.
        /// </summary>
        private List<PopupEntry> _presentedPopups = new List<PopupEntry>();
        private bool _isSyncing = false;

        private class PopupEntry
        {
            public BaseViewModel ViewModel { get; set; }
            public UIViewController ViewController { get; set; }
        }

        public new PagedViewModelWithPopups ViewModel
        {
            get { return base.ViewModel as PagedViewModelWithPopups; }
            set { base.ViewModel = value; }
        }

        private NotifyCollectionChangedEventHandler _popupsCollectionChangedHandler;
        private PropertyChangedEventHandler _propertyChangedEventHandler;
        protected override void OnViewModelChanged(PagedViewModel oldViewModel, PagedViewModel currentViewModel)
        {
            Deregister(oldViewModel);

            if (_popupsCollectionChangedHandler == null)
            {
                _popupsCollectionChangedHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>(Popups_CollectionChanged).Handler;
            }

            if (_propertyChangedEventHandler == null)
            {
                _propertyChangedEventHandler = new WeakEventHandler<PropertyChangedEventArgs>(ViewModel_PropertyChanged).Handler;
            }

            PagedViewModelWithPopups newModel = currentViewModel as PagedViewModelWithPopups;
            if (newModel != null)
            {
                newModel.PropertyChanged += _propertyChangedEventHandler;
                newModel.Popups.CollectionChanged += _popupsCollectionChangedHandler;
            }

            SyncPopups();
            UpdateFullScreenPopup();

            base.OnViewModelChanged(oldViewModel, currentViewModel);
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.FullScreenPopup))
            {
                UpdateFullScreenPopup();
            }
        }

        private UIViewController _prevFullScreenController;
        private BaseViewModel _prevFullScreenViewModel;
        private void UpdateFullScreenPopup()
        {
            // If there shouldn't be any full screen content
            if (ViewModel == null || ViewModel.FullScreenPopup == null)
            {
                // If there was full screen content
                if (_prevFullScreenController != null)
                {
                    // Dismiss it and update current
                    _prevFullScreenController.DismissViewController(true, null);
                    _prevFullScreenController = null;
                    _prevFullScreenViewModel = null;
                }

                return;
            }

            // Otherwise, if the full screen content is the same
            if (ViewModel.FullScreenPopup == _prevFullScreenViewModel)
            {
                // Do nothing
                return;
            }

            // Otherwise, the full screen content must be initialized and is different
            if (_prevFullScreenController != null)
            {
                _prevFullScreenController.DismissViewController(false, null);
            }

            var newController = ViewModelToViewConverter.Convert(ViewModel.FullScreenPopup);
            newController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
            ShowDetailViewController(newController, null);
            _prevFullScreenController = newController;
            _prevFullScreenViewModel = ViewModel.FullScreenPopup;
        }

        private void Deregister(BaseViewModel oldViewModel)
        {
            PagedViewModelWithPopups old = oldViewModel as PagedViewModelWithPopups;

            if (old != null)
            {
                old.PropertyChanged -= _propertyChangedEventHandler;
                old.Popups.CollectionChanged -= _popupsCollectionChangedHandler;
            }
        }

        /// <summary>
        /// Syncs the presented modals with the Popups collection.
        /// Each popup is presented as its own modal sheet, stacked on top of the previous one.
        /// </summary>
        private void SyncPopups()
        {
            if (_isSyncing)
            {
                return;
            }

            _isSyncing = true;
            try
            {
                if (ViewModel == null || _destroyed)
                {
                    // Dismiss all
                    DismissAllPopups();
                    return;
                }

                var desiredPopups = ViewModel.Popups.ToList();

                // Remove any popups that are no longer in the list (from the top down)
                for (int i = _presentedPopups.Count - 1; i >= 0; i--)
                {
                    if (i >= desiredPopups.Count || _presentedPopups[i].ViewModel != desiredPopups[i])
                    {
                        // Dismiss from this point and everything above it
                        DismissPopupsFrom(i);
                        break;
                    }
                }

                // Present any new popups
                for (int i = _presentedPopups.Count; i < desiredPopups.Count; i++)
                {
                    PresentPopup(desiredPopups[i]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG PopupsPresenter: Exception syncing popups: {ex}");
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private void PresentPopup(BaseViewModel viewModel)
        {
            var viewController = ViewModelToViewConverter.Convert(viewModel);
            var wrapper = new PopupModalWrapper(viewController, this);

            // Present from the top-most currently presented VC
            UIViewController presenter = _presentedPopups.Count > 0
                ? _presentedPopups[_presentedPopups.Count - 1].ViewController
                : this;

            presenter.PresentViewController(wrapper, true, null);

            _presentedPopups.Add(new PopupEntry
            {
                ViewModel = viewModel,
                ViewController = wrapper
            });
        }

        private void DismissPopupsFrom(int index)
        {
            if (index < _presentedPopups.Count)
            {
                // Dismissing a modal also dismisses any modals presented on top of it
                _presentedPopups[index].ViewController.DismissViewController(true, null);
                _presentedPopups.RemoveRange(index, _presentedPopups.Count - index);
            }
        }

        private void DismissAllPopups()
        {
            DismissPopupsFrom(0);
        }

        /// <summary>
        /// Called when a modal is dismissed via swipe gesture (not programmatic dismissal).
        /// </summary>
        internal void HandleModalDismissedByUser(PopupModalWrapper wrapper)
        {
            // Find which popup was dismissed
            int index = _presentedPopups.FindIndex(p => p.ViewController == wrapper);
            if (index < 0)
            {
                return;
            }

            // Remove from our tracking (everything at and above this index)
            _presentedPopups.RemoveRange(index, _presentedPopups.Count - index);

            // Sync back to the ViewModel's Popups collection
            while (ViewModel.Popups.Count > index)
            {
                ViewModel.Popups.RemoveAt(ViewModel.Popups.Count - 1);
            }
        }

        private void Popups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ViewModel?.Popups != sender)
            {
                return;
            }

            SyncPopups();
        }

        internal override void Destroy()
        {
            Deregister(ViewModel);
            _destroyed = true;
            DismissAllPopups();

            base.Destroy();
        }
    }

    /// <summary>
    /// Wraps a popup view controller in a UINavigationController for modal presentation,
    /// and detects user-initiated dismissals (swipe gesture).
    /// </summary>
    internal class PopupModalWrapper : UINavigationController
    {
        private PagedViewModelWithPopupsPresenter _presenter;

        public PopupModalWrapper(UIViewController rootViewController, PagedViewModelWithPopupsPresenter presenter) : base(rootViewController)
        {
            _presenter = presenter;
            NavigationBarHidden = true;
            PresentationController.Delegate = new PopupPresentationDelegate(this);
        }

        private class PopupPresentationDelegate : UIAdaptivePresentationControllerDelegate
        {
            private PopupModalWrapper _wrapper;

            public PopupPresentationDelegate(PopupModalWrapper wrapper)
            {
                _wrapper = wrapper;
            }

            public override void DidDismiss(UIPresentationController presentationController)
            {
                _wrapper._presenter.HandleModalDismissedByUser(_wrapper);
            }
        }
    }
}