﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Interfaces;
using UnityMvvmToolkit.Core;
using UnityMvvmToolkit.UniTask;
using UnityMvvmToolkit.UniTask.Interfaces;

namespace ViewModels
{
    public class MainViewModel : ViewModel, IDisposable
    {
        private readonly IDialogsService _dialogsService;

        public MainViewModel(IAppContext appContext)
        {
            _dialogsService = appContext.Resolve<IDialogsService>();

            TaskItems = new ObservableCollection<TaskItemData>();
            TaskItems.CollectionChanged += OnTaskItemsCollectionChanged;

            ShowAddTaskDialogCommand = new AsyncLazyCommand(ShowAddTaskDialogAsync);
            HideAddTaskDialogCommand = new AsyncLazyCommand(HideAddTaskDialogAsync);

            SubscribeOnTaskAddMessage(appContext.Resolve<TaskBroker>()).Forget();
        }

        public string Date => GetTodayDate();
        public int CreatedTasks => TaskItems.Count;
        public int CompletedTasks => TaskItems.Count(data => data.IsDone);
        public bool IsAddTaskDialogActive => _dialogsService.IsAddTaskDialogActive;
        public ObservableCollection<TaskItemData> TaskItems { get; }

        public IAsyncCommand ShowAddTaskDialogCommand { get; }
        public IAsyncCommand HideAddTaskDialogCommand { get; }

        public void Dispose()
        {
            TaskItems.CollectionChanged -= OnTaskItemsCollectionChanged;
        }

        private async UniTask ShowAddTaskDialogAsync(CancellationToken cancellationToken = default)
        {
            var showDialogTask = _dialogsService.ShowAddTaskDialogAsync();

            OnPropertyChanged(nameof(IsAddTaskDialogActive));

            await showDialogTask;
        }

        private async UniTask HideAddTaskDialogAsync(CancellationToken cancellationToken = default)
        {
            var hideDialogTask = _dialogsService.HideAddTaskDialogAsync();

            OnPropertyChanged(nameof(IsAddTaskDialogActive));

            await hideDialogTask;
        }

        private async UniTaskVoid SubscribeOnTaskAddMessage(TaskBroker taskBroker)
        {
            await foreach (var task in taskBroker.Subscribe())
            {
                TaskItems.Add(new TaskItemData { Name = task });
                await HideAddTaskDialogAsync();
            }
        }

        private string GetTodayDate()
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(DateTime.Today.ToString("dddd, d MMM"));
        }

        private void OnTaskItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnPropertyChanged(nameof(CreatedTasks));
                    break;
                case NotifyCollectionChangedAction.Replace:
                    OnPropertyChanged(nameof(CompletedTasks));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnPropertyChanged(nameof(CreatedTasks));
                    OnPropertyChanged(nameof(CompletedTasks));
                    break;
            }
        }
    }
}