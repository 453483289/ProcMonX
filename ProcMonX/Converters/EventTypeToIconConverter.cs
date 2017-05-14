using ProcMonX.Tracing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ProcMonX.Converters {
	class EventTypeToIconConverter : IValueConverter {
		static Dictionary<EventType, string> _icons = new Dictionary<EventType, string> {
			{ EventType.ProcessStart, "/icons/process_start.ico" },
			{ EventType.ProcessStop, "/icons/process_stop.ico" }
		};

		const string _genericIcon = "/icons/add.ico";

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			var type = (EventType)value;
			if (_icons.TryGetValue(type, out var icon))
				return icon;
			return _genericIcon;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
