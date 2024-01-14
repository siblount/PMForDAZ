using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.TestingSuiteWindows
{
    internal class RichTextBoxSink : ILogEventSink
    {
        private ITextFormatter formatter = SerilogLoggerConstants.Template;
        private RichTextBox? _textBox;

        public RichTextBoxSink() { }

        public void Emit(LogEvent logEvent)
        {
            _textBox ??= MainForm.Instance?.logOutputTxtBox ?? null;
            if (_textBox is null || !_textBox.IsHandleCreated) return;

            using StringWriter stringWriter = new StringWriter();
            formatter.Format(logEvent, stringWriter);
            _textBox.BeginInvoke(() => _textBox.AppendText(stringWriter.ToString()));
        }
    }
}
