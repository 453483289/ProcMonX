using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ProcMonX.Tracing {
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
		ThreadPriority = KernelTraceEventParser.Keywords.ThreadPriority
	}

	enum EventType {
		None,
		ProcessStart, ProcessStop,
		ThreadStart, ThreadStop,
		VirtualAlloc, VirtualFree,
		RegistryOPenKey, RegistryQueryValue
	}

	sealed class TraceManager : IDisposable {
		TraceEventSession _session;
		KernelTraceEventParser _parser;
		Thread _processingThread;

		public event Action<ProcessTraceData, EventType> ProcessTrace;
		public event Action<ThreadTraceData, EventType> ThreadTrace;

		public TraceManager() {
			_session = new TraceEventSession(KernelTraceEventParser.KernelSessionName) {
				BufferSizeMB = 128,
				CpuSampleIntervalMSec = 10
			};
		}

		public void Dispose() {
			_session.Dispose();
		}

		public void Start(TraceElements elements) {
			_processingThread = new Thread(() => {
				_session.EnableKernelProvider((KernelTraceEventParser.Keywords)elements);
				_parser = new KernelTraceEventParser(_session.Source);
				SetupCallbacks(elements);
				_session.Source.Process();
			});
			_processingThread.Priority = ThreadPriority.BelowNormal;
			_processingThread.IsBackground = true;
			_processingThread.Start();
		}


		public void Stop() {
			_session.Source.StopProcessing();
		}

		private void SetupCallbacks(TraceElements elements) {
			if (elements.HasFlag(TraceElements.Process)) {
				_parser.ProcessStart += OnProcessStart;
				_parser.ProcessStop += OnProcessStop;
			}
			if (elements.HasFlag(TraceElements.Thread)) {
				_parser.ThreadStart += OnThreadStart; ;
			}
		}

		private void OnProcessStop(ProcessTraceData obj) {
			ProcessTrace?.Invoke((ProcessTraceData)obj.Clone(), EventType.ProcessStop);
		}

		private void OnThreadStart(ThreadTraceData obj) {
			ThreadTrace?.Invoke((ThreadTraceData)obj.Clone(), EventType.ThreadStart);
		}

		private void OnProcessStart(ProcessTraceData obj) {
			ProcessTrace?.Invoke((ProcessTraceData)obj.Clone(), EventType.ProcessStart);
		}
	}
}
