using Prism.Mvvm;
using ProcMonX.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using System.Windows;
using Zodiacon.WPF;
using System.Collections.ObjectModel;
using Microsoft.Diagnostics.Tracing;
using ProcMonX.Tracing;
using System.Windows.Threading;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;

namespace ProcMonX.ViewModels {
	class MainViewModel : BindableBase {
		TraceManager _traceManager = new TraceManager();
		ObservableCollection<TabViewModelBase> _tabs = new ObservableCollection<TabViewModelBase>();
		ObservableCollection<TraceEventData> _events = new ObservableCollection<TraceEventData>();
		ObservableCollection<TraceEventData<ProcessTraceData>> _processTrace = new ObservableCollection<TraceEventData<ProcessTraceData>>();
		ObservableCollection<TraceEventData<ThreadTraceData>> _threadTrace = new ObservableCollection<TraceEventData<ThreadTraceData>>();

		public AppOptions Options { get; } = new AppOptions();

		public IList<TabViewModelBase> Tabs => _tabs;

		public string Title => "Process Monitor X (C)2017 by Pavel Yosifovich";
		public string Icon => "/icons/app.ico";

		public readonly IUIServices UI;
		public readonly Dispatcher Dispatcher;

		public MainViewModel(IUIServices ui) {
			UI = ui;
			Dispatcher = Dispatcher.CurrentDispatcher;

			HookupEvents();
			Init();
		}

		private void Init() {
			var mainTab = new EventsTabViewModel(_events);
			AddTab(mainTab, true);
		}

		private void HookupEvents() {
			_traceManager.ProcessTrace += (evt, type) => Dispatcher.InvokeAsync(() => {
				var data = new TraceEventData<ProcessTraceData>(evt, type, GetTypeName(type));
				_processTrace.Add(data);
				_events.Add(data);
			});

			_traceManager.ThreadTrace += (evt, type) => Dispatcher.InvokeAsync(() => {
				var data = new TraceEventData<ThreadTraceData>(evt, type, GetTypeName(type));
				_threadTrace.Add(data);
				_events.Add(data);
			});
		}

		private string GetTypeName(EventType type) {
			switch (type) {
				case EventType.ProcessStart: return "Process Start";
				case EventType.ProcessStop: return "Process Stop";
				case EventType.ThreadStart: return "Thread Start";
				case EventType.ThreadStop: return "Thread Stop";
			}
			return null;
		}

		public ICommand AlwaysOnTopCommand => new DelegateCommand<FrameworkElement>(element =>
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
			_traceManager.Start(TraceElements.Process | TraceElements.Thread);
			IsMonitoring = true;
		}
	}
}
