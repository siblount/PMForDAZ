using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using System.Text;
using System;

namespace DAZ_Installer
{
    internal class MSTestLoggerSink : ILogEventSink
    {
        private ITextFormatter formatter = SerilogLoggerConstants.Template;
        Action<string, object[]> logMessageFunc;
        // We can't directly do this because the namespace is not exposed to non-testing solutions.
        public MSTestLoggerSink(ITextFormatter? formatter = null, Action<string, object[]> logMessageFunc = null)
        {
            if (formatter != null) this.formatter = formatter;
            this.logMessageFunc = logMessageFunc ?? throw new ArgumentNullException(nameof(logMessageFunc));
        }
        public void Emit(LogEvent logEvent)
        {
            using StringWriter stringWriter = new StringWriter();
            formatter.Format(logEvent, stringWriter);
            logMessageFunc(stringWriter.ToString(), Array.Empty<object>());
        }
    }
}
