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

namespace ProcMonX.ViewModels {
	class MainViewModel : BindableBase {
		public AppOptions Options { get; } = new AppOptions();

		public string Title => "Process Monitor X (C)2017 by Pavel Yosifovich";
		public string Icon => "/icons/app.ico";

		public readonly IUIServices UI;

		public MainViewModel(IUIServices ui) {
			UI = ui;
		}

		public ICommand AlwaysOnTopCommand => new DelegateCommand<FrameworkElement>(element =>
			Window.GetWindow(element).Topmost = Options.AlwaysOnTop);

		public ICommand ExitCommand => new DelegateCommand(() => Application.Current.Shutdown());
	}
}
