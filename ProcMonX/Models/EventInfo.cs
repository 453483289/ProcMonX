using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing.Parsers;

namespace ProcMonX.Models {
	[Flags]
	enum TraceElements {
		None = 0,
		Process = KernelTraceEventParser.Keywords.Process,
		Thread = KernelTraceEventParser.Keywords.Thread,
		File = KernelTraceEventParser.Keywords.FileIO,
		ImageLoad = KernelTraceEventParser.Keywords.ImageLoad,
		Registry = KernelTraceEventParser.Keywords.Registry,
		VirtualAlloc = KernelTraceEventParser.Keywords.VirtualAlloc,
		Driver = KernelTraceEventParser.Keywords.Driver,
		Network = KernelTraceEventParser.Keywords.NetworkTCPIP,
	}

	enum EventType {
		None,
		ProcessStart, ProcessStop, ProcessExists, ProcessExited,
		ThreadStart, ThreadStop, ThreadExists,
		VirtualAlloc, VirtualFree,
		RegistryOpenKey, RegistryQueryValue, RegistryWriteValue, RegistryReadValue, RegistryCreateKey,
		AlpcSendMessage, AlpcReceiveMessage,
		ImageLoad, ImageUnload,
	}

	class EventInfo {
		public TraceElements TraceElement { get; private set; }
		public EventType EventType { get; private set; }
		public string AsString { get; private set; }

		public static readonly IDictionary<EventType, EventInfo> AllEvents =
			new List<EventInfo> {
				new EventInfo { EventType = EventType.ProcessStart, TraceElement = TraceElements.Process, AsString = "Process Start" },
				new EventInfo { EventType = EventType.ProcessExists, TraceElement = TraceElements.Process, AsString = "Process Exists" },
				new EventInfo { EventType = EventType.ProcessStop, TraceElement = TraceElements.Process, AsString = "Process Stop" },
				new EventInfo { EventType = EventType.ThreadStart, TraceElement = TraceElements.Thread, AsString = "Thread Start" },
				new EventInfo { EventType = EventType.ThreadExists, TraceElement = TraceElements.Thread, AsString = "Thread Exists" },
				new EventInfo { EventType = EventType.ThreadStop, TraceElement = TraceElements.Thread, AsString = "Thread Stop" },
				new EventInfo { EventType = EventType.RegistryOpenKey, TraceElement = TraceElements.Registry, AsString = "Registry Key Open" },
				new EventInfo { EventType = EventType.RegistryCreateKey, TraceElement = TraceElements.Registry, AsString = "Registry Key Create" },
				new EventInfo { EventType = EventType.RegistryQueryValue, TraceElement = TraceElements.Registry, AsString = "Registry Query Value" },
				new EventInfo { EventType = EventType.ImageLoad, TraceElement = TraceElements.ImageLoad, AsString = "Image Loaded" },
				new EventInfo { EventType = EventType.ImageUnload, TraceElement = TraceElements.ImageLoad, AsString = "Image Unloaded" },
			}.ToDictionary(evt => evt.EventType);
	}
}
