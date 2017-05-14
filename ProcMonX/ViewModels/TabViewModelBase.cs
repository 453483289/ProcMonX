using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ProcMonX.ViewModels {
	[AttributeUsage(AttributeTargets.Class, Inherited = true)]
	sealed class TabItemAttribute : Attribute {
		public string Header { get; set; }
		public string Icon { get; set; }
		public bool CanClose { get; set; } = true;
	}

	abstract class TabViewModelBase : BindableBase {
		public string Header { get; set; }
		public string Icon { get; set; }

		public bool CanClose { get; set; } = true;

		protected TabViewModelBase() {
			var tabItemAttribute = GetType().GetCustomAttribute<TabItemAttribute>();
			if (tabItemAttribute != null) {
				Header = tabItemAttribute.Header;
				Icon = tabItemAttribute.Icon;
				CanClose = tabItemAttribute.CanClose;
			}
		}
	}
}
