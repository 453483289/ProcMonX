using Microsoft.Diagnostics.Tracing;
using ProcMonX.Tracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProcMonX.Models;

namespace ProcMonX.ViewModels {
	class TraceEventDataViewModel {
		static int _globalIndex;
		public int Index { get; }
		public TraceEvent Data { get; }
		public EventType Type { get; }
		public string TypeAsString { get; }

		public readonly EventInfo Info;

		public string Icon { get; }

		public int? ThreadId => Data.ThreadID < 0 ? (int?)null : Data.ThreadID;

		public TraceEventDataViewModel(TraceEvent evt, EventType type) {
			Data = evt;
			Type = type;
			Info = EventInfo.AllEvents[type];
			TypeAsString = Info.AsString ?? type.ToString();
			Index = Interlocked.Increment(ref _globalIndex);
			Icon = $"/icons/events/{type.ToString()}.ico";
		}
	}

	class TraceEventDataViewModel<T> : TraceEventDataViewModel where T : TraceEvent {
		public new T Data { get; }

		public TraceEventDataViewModel(T evt, EventType type) : base(evt, type) {
			Data = evt;
		}
	}
}
