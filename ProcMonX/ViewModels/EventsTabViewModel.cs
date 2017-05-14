using Microsoft.Diagnostics.Tracing;
using ProcMonX.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcMonX.ViewModels {
	[TabItem(CanClose = false, Header = "All Events", Icon = "/icons/event.png")]
	class EventsTabViewModel : TabViewModelBase {
		public EventsTabViewModel(IList<TraceEventData> events) {
			Events = events;
		}
		public IList<TraceEventData> Events { get; }

	}
}
