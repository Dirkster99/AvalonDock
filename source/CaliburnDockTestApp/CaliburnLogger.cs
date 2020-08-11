using Caliburn.Micro;
using System;
using System.Diagnostics;

namespace CaliburnDockTestApp
{
	class CaliburnLogger : ILog
	{
		private readonly Type _type;

		public CaliburnLogger(Type type)
		{
			_type = type;
		}

		public void Error(Exception exception)
		{
			Trace.TraceError($"{_type.Name}: {exception}");
		}

		public void Info(string format, params object[] args)
		{
			Trace.TraceInformation($"{_type.Name}: " + format, args);
		}

		public void Warn(string format, params object[] args)
		{
			Trace.TraceWarning($"{_type.Name}: " + format, args);
		}
	}
}
