using AutoAssistantAppDataLibrary.ViewItemsGroup;
using BareMvvm.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsPortable;

namespace AutoAssistantAppDataLibraryTests.Helpers
{
    public static class ChangeHelper
    {
        public static Task WaitTillCountAsync<T>(MyObservableList<T> list, int desiredCount)
        {
            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();

            NotifyCollectionChangedEventHandler collectionChangedAction = null;
            collectionChangedAction = delegate
            {
                if (list.Count == desiredCount)
                {
                    list.CollectionChanged -= collectionChangedAction;
                    completionSource.TrySetResult(true);
                }
            };

            list.CollectionChanged += collectionChangedAction;

            return completionSource.Task;
        }

        public static Task<T> GetContentAsync<T>(PagedViewModel viewModel) where T : BaseViewModel
        {
            TaskCompletionSource<T> completionSource = new TaskCompletionSource<T>();

            PropertyChangedEventHandler propertyChangedHandler = null;
            propertyChangedHandler = delegate
            {
                if (viewModel.Content != null && viewModel.Content.GetType() == typeof(T))
                {
                    viewModel.PropertyChanged -= propertyChangedHandler;
                    completionSource.TrySetResult(viewModel.Content as T);
                }
            };

            viewModel.PropertyChanged += propertyChangedHandler;

            propertyChangedHandler(viewModel, new PropertyChangedEventArgs("Content"));

            return completionSource.Task;
        }

        public static Task WaitTillDataChangedCompleted(BaseAccountViewItemsGroup viewGroup)
        {
            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();

            EventHandler completedHandler = null;
            completedHandler = delegate
            {
                viewGroup.OnDataChangedCompleted -= completedHandler;
                completionSource.TrySetResult(true);
            };

            viewGroup.OnDataChangedCompleted += completedHandler;

            return completionSource.Task;
        }
    }
}
