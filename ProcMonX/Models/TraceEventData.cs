using Microsoft.Diagnostics.Tracing;
using ProcMonX.Tracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcMonX.Models {
	class TraceEventData {
		static int _globalIndex;
		public int Index { get; }
		public TraceEvent Data { get; }
		public EventType Type { get; }
		public string TypeAsString { get; }

		public TraceEventData(TraceEvent evt, EventType type, string typeAsString) {
			Data = evt;
			Type = type;
			TypeAsString = typeAsString ?? type.ToString();
			Index = Interlocked.Increment(ref _globalIndex);
		}
	}

	class TraceEventData<T> : TraceEventData where T : TraceEvent {
		public new T Data { get; }

		public TraceEventData(T evt, EventType type, string typeAsString) : base(evt, type, typeAsString) {
			Data = evt;
		}
	}
}
