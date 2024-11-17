using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using System.IO;
using System.Text;
using System;
using Serilog.Configuration;
using Serilog;

namespace DAZ_Installer
{
    public class BufferSink : ILogEventSink
    {
        private readonly StringBuilder _buffer;
        private readonly ITextFormatter formatter = SerilogLoggerConstants.Template;

        public BufferSink(ITextFormatter? formatter = null)
        {
            _buffer = new StringBuilder(1024);
            if (formatter != null) this.formatter = formatter;
        }

        public void Emit(LogEvent logEvent)
        {
            using StringWriter stringWriter = new StringWriter();
            formatter.Format(logEvent, stringWriter);
            _buffer.AppendLine(stringWriter.ToString());
        }

        public string GetLogContent() => _buffer.ToString();
        public override string ToString() => _buffer.ToString();

        public void Clear() => _buffer.Clear();
    }
}
