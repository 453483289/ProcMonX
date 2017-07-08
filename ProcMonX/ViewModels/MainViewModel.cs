using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Input;
using Prism.Mvvm;
using ProcMonX.Models;
using Prism.Commands;
using Zodiacon.WPF;
using ProcMonX.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using System.Diagnostics;
using System.Threading;
using System.Collections;

namespace ProcMonX.ViewModels {
	class MainViewModel : BindableBase {
		TraceManager _traceManager = new TraceManager();
		ObservableCollection<TabViewModelBase> _tabs = new ObservableCollection<TabViewModelBase>();
		ObservableCollection<TraceEventDataViewModel> _events = new ObservableCollection<TraceEventDataViewModel>();
		List<TraceEventDataViewModel> _tempEvents = new List<TraceEventDataViewModel>(128);
		DispatcherTimer _updateTimer;

		public AppOptions Options { get; } = new AppOptions();

		public IList<TabViewModelBase> Tabs => _tabs;

		public IList<TraceEventDataViewModel> Events => _events;

		public string Title => "Process Monitor X (C)2017 by Pavel Yosifovich";
		public string Icon => "/icons/app.ico";

		public readonly IUIServices UI;
		public readonly Dispatcher Dispatcher;

		public MainViewModel(IUIServices ui) {
			UI = ui;
			Dispatcher = Dispatcher.CurrentDispatcher;

			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
			Thread.CurrentThread.Priority = ThreadPriority.Highest;

			HookupEvents();
			Init();

			_updateTimer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.ApplicationIdle, (_, __) => Update(), Dispatcher);
			_updateTimer.Start();
		}

		private void Init() {
			var mainTab = new EventsTabViewModel(_events) {
				Header = "All Events",
				Icon = "/icons/tabs/event.ico",
			};
			AddTab(mainTab, true);
			AddTab(new EventsTabViewModel(_events, evt => evt.Data is ProcessTraceData) {
				Header = "Processes",
				Icon = "/icons/tabs/processes.ico",
			});

			AddTab(new EventsTabViewModel(_events, evt => evt.Data is ThreadTraceData) {
				Header = "Threads",
				Icon = "/icons/tabs/threads.ico",
			});
		}

		public IEnumerable ToolbarItems {
			get {
				yield return new { ToolTip = "Start Monitoring", Icon = "start" };
				yield return new { ToolTip = "Stop Monitoring", Icon = "stop" };
			}
		}

		private void HookupEvents() {
			_traceManager.ProcessTrace += (evt, type) => {
				lock(_tempEvents)
					_tempEvents.Add(new TraceEventDataViewModel<ProcessTraceData>(evt, type));
			};

			_traceManager.ThreadTrace += (evt, type) => {
				lock (_tempEvents)
					_tempEvents.Add(new TraceEventDataViewModel<ThreadTraceData>(evt, type));
			};

			_traceManager.RegistryTrace += (evt, type) => {
				lock (_tempEvents)
					_tempEvents.Add(new TraceEventDataViewModel<RegistryTraceData>(evt, type));
			};
		}

		void Update() {
			Debug.WriteLine($"{Environment.TickCount} Updating collection");

			lock (_tempEvents) {
				for(int i = 0; i < _tempEvents.Count; i++)
					_events.Add(_tempEvents[i]);
				_tempEvents.Clear();
			}
		}

		public ICommand AlwaysOnTopCommand => new DelegateCommand<DependencyObject>(element =>
			Window.GetWindow(element).Topmost = Options.AlwaysOnTop);

		public ICommand ExitCommand => new DelegateCommand(() => Application.Current.Shutdown());

		private TabViewModelBase _selectedTab;

		public TabViewModelBase SelectedTab {
			get { return _selectedTab; }
			set { SetProperty(ref _selectedTab, value); }
		}

		public void AddTab(TabViewModelBase item, bool activate = false) {
			_tabs.Add(item);
			if (activate)
				SelectedTab = item;
		}

		private bool _isMonitoring;

		public bool IsMonitoring {
			get { return _isMonitoring; }
			set { SetProperty(ref _isMonitoring, value); }
		}

		public DelegateCommandBase GoCommand => new DelegateCommand(
			() => ResumeMonitoring(),
			() => !IsMonitoring)
			.ObservesProperty(() => IsMonitoring);

		public DelegateCommandBase StopCommand => new DelegateCommand(
			() => StopMonitoring(),
			() => IsMonitoring)
			.ObservesProperty(() => IsMonitoring);

		private void StopMonitoring() {
			_traceManager.Stop();
			IsMonitoring = false;
		}

		private void ResumeMonitoring() {
			_traceManager.Start(TraceElements.Process | TraceElements.Thread | TraceElements.Registry, false);// _events.Count == 0);
			IsMonitoring = true;
		}
	}
}