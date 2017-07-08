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
using ProcMonX.Models;

namespace ProcMonX.Tracing {

	sealed class TraceManager : IDisposable {
		TraceEventSession _session;
		KernelTraceEventParser _parser;
		Thread _processingThread;
		bool _includeInit;

		public event Action<ProcessTraceData, EventType> ProcessTrace;
		public event Action<ThreadTraceData, EventType> ThreadTrace;
		public event Action<RegistryTraceData, EventType> RegistryTrace;

		public TraceManager() {
		}

		public void Dispose() {
			_session.Dispose();
		}

		public void Start(TraceElements elements, bool includeInit) {
			_includeInit = includeInit;
			_session = new TraceEventSession(KernelTraceEventParser.KernelSessionName) {
				BufferSizeMB = 128,
				CpuSampleIntervalMSec = 10
			};
			_session.EnableKernelProvider((KernelTraceEventParser.Keywords)elements);

			_processingThread = new Thread(() => {
				_parser = new KernelTraceEventParser(_session.Source);
				SetupCallbacks(elements);
				_session.Source.Process();
			});
			_processingThread.Priority = ThreadPriority.Lowest;
			_processingThread.IsBackground = true;
			_processingThread.Start();
		}


		public void Stop() {
			_session.Flush();
			_session.Stop();
		}

		private void SetupCallbacks(TraceElements elements) {
			if (elements.HasFlag(TraceElements.Process)) {
				_parser.ProcessStart += OnProcessStart;
				if (_includeInit) {
					_parser.ProcessDCStart += OnProcessDCStart;
					_parser.ProcessDCStop += obj => ProcessTrace?.Invoke((ProcessTraceData)obj.Clone(), EventType.ProcessExited);
				}
				_parser.ProcessStop += OnProcessStop;
			}
			if (elements.HasFlag(TraceElements.Thread)) {
				_parser.ThreadStart += OnThreadStart;
				_parser.ThreadStop += OnThreadStop;
			}
			if (elements.HasFlag(TraceElements.Registry)) {
				_parser.RegistryCreate += OnRegistryCreate;
				_parser.RegistryOpen += obj => RegistryTrace?.Invoke((RegistryTraceData)obj.Clone(), EventType.RegistryOpenKey);
			}
		}

		private void OnRegistryCreate(RegistryTraceData obj) {
			RegistryTrace?.Invoke((RegistryTraceData)obj.Clone(), EventType.RegistryCreateKey);
		}

		private void OnProcessDCStart(ProcessTraceData obj) {
			var data = (ProcessTraceData)obj.Clone();
			ProcessTrace?.Invoke(data, EventType.ProcessExists);
		}

		private void OnThreadStop(ThreadTraceData obj) {
			ThreadTrace?.Invoke((ThreadTraceData)obj.Clone(), EventType.ThreadStop);
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
