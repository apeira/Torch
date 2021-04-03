using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Torch.Core.Commands
{
    /// <summary>
    /// Wraps an existing <see cref="TextWriter"/> and raises events before and after
    /// each write.
    /// </summary>
    public class NotifyingTextWriter : TextWriter
    {
        private readonly TextWriter _writer;

        public event Action BeforeWrite;

        public event Action AfterWrite;

        public NotifyingTextWriter(TextWriter writer)
        {
            _writer = writer;
        }

        private void OnBeforeWrite()
        {
            var beforeConsoleWrite = BeforeWrite;
            beforeConsoleWrite?.Invoke();
        }

        private void OnAfterWrite()
        {
            var afterConsoleWrite = AfterWrite;
            afterConsoleWrite?.Invoke();
        }

        public override void Close()
        {
            _writer.Close();
        }

        protected override void Dispose(bool disposing)
        {
            _writer.Dispose();
        }

        public override void Flush()
        {
            _writer.Flush();
        }

        public override void Write(char value)
        {
            OnBeforeWrite();
            _writer.Write(value);
            OnAfterWrite();
        }

        public override void Write(char[] buffer)
        {
            OnBeforeWrite();
            _writer.Write(buffer);
            OnAfterWrite();
        }

        public override void Write(char[] buffer, int index, int count)
        {
            OnBeforeWrite();
            _writer.Write(buffer, index, count);
            OnAfterWrite();
        }

        public override void Write(bool value)
        {
            OnBeforeWrite();
            _writer.Write(value);
            OnAfterWrite();
        }

        public override void Write(int value)
        {
            OnBeforeWrite();
            _writer.Write(value);
            OnAfterWrite();
        }

        public override void Write(uint value)
        {
            OnBeforeWrite();
            _writer.Write(value);
            OnAfterWrite();
        }

        public override void Write(long value)
        {
            OnBeforeWrite();
            _writer.Write(value);
            OnAfterWrite();
        }

        public override void Write(ulong value)
        {
            OnBeforeWrite();
            _writer.Write(value);
            OnAfterWrite();
        }

        public override void Write(float value)
        {
            OnBeforeWrite();
            _writer.Write(value);
            OnAfterWrite();
        }

        public override void Write(double value)
        {
            OnBeforeWrite();
            _writer.Write(value);
            OnAfterWrite();
        }

        public override void Write(decimal value)
        {
            OnBeforeWrite();
            _writer.Write(value);
            OnAfterWrite();
        }

        public override void Write(string value)
        {
            OnBeforeWrite();
            _writer.Write(value);
            OnAfterWrite();
        }

        public override void Write(object value)
        {
            OnBeforeWrite();
            _writer.Write(value);
            OnAfterWrite();
        }

        public override void Write(string format, object arg0)
        {
            OnBeforeWrite();
            _writer.Write(format, arg0);
            OnAfterWrite();
        }

        public override void Write(string format, object arg0, object arg1)
        {
            OnBeforeWrite();
            _writer.Write(format, arg0, arg1);
            OnAfterWrite();
        }

        public override void Write(string format, object arg0, object arg1, object arg2)
        {
            OnBeforeWrite();
            _writer.Write(format, arg0, arg1, arg2);
            OnAfterWrite();
        }

        public override void Write(string format, params object[] arg)
        {
            OnBeforeWrite();
            _writer.Write(format, arg);
            OnAfterWrite();
        }

        public override void WriteLine()
        {
            OnBeforeWrite();
            _writer.WriteLine();
            OnAfterWrite();
        }

        public override void WriteLine(char value)
        {
            OnBeforeWrite();
            _writer.WriteLine(value);
            OnAfterWrite();
        }

        public override void WriteLine(char[] buffer)
        {
            OnBeforeWrite();
            _writer.WriteLine(buffer);
            OnAfterWrite();
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            OnBeforeWrite();
            _writer.WriteLine(buffer, index, count);
            OnAfterWrite();
        }

        public override void WriteLine(bool value)
        {
            OnBeforeWrite();
            _writer.WriteLine(value);
            OnAfterWrite();
        }

        public override void WriteLine(int value)
        {
            OnBeforeWrite();
            _writer.WriteLine(value);
            OnAfterWrite();
        }

        public override void WriteLine(uint value)
        {
            OnBeforeWrite();
            _writer.WriteLine(value);
            OnAfterWrite();
        }

        public override void WriteLine(long value)
        {
            OnBeforeWrite();
            _writer.WriteLine(value);
            OnAfterWrite();
        }

        public override void WriteLine(ulong value)
        {
            OnBeforeWrite();
            _writer.WriteLine(value);
            OnAfterWrite();
        }

        public override void WriteLine(float value)
        {
            OnBeforeWrite();
            _writer.WriteLine(value);
            OnAfterWrite();
        }

        public override void WriteLine(double value)
        {
            OnBeforeWrite();
            _writer.WriteLine(value);
            OnAfterWrite();
        }

        public override void WriteLine(decimal value)
        {
            OnBeforeWrite();
            _writer.WriteLine(value);
            OnAfterWrite();
        }

        public override void WriteLine(string value)
        {
            OnBeforeWrite();
            _writer.WriteLine(value);
            OnAfterWrite();
        }

        public override void WriteLine(object value)
        {
            OnBeforeWrite();
            _writer.WriteLine(value);
            OnAfterWrite();
        }

        public override void WriteLine(string format, object arg0)
        {
            OnBeforeWrite();
            _writer.WriteLine(format, arg0);
            OnAfterWrite();
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            OnBeforeWrite();
            _writer.WriteLine(format, arg0, arg1);
            OnAfterWrite();
        }

        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            OnBeforeWrite();
            _writer.WriteLine(format, arg0, arg1, arg2);
            OnAfterWrite();
        }

        public override void WriteLine(string format, params object[] arg)
        {
            OnBeforeWrite();
            _writer.WriteLine(format, arg);
            OnAfterWrite();
        }

        public override Task WriteAsync(char value)
        {
            return Task.Run(async () =>
            {
                OnBeforeWrite();
                await _writer.WriteAsync(value);
                OnAfterWrite();
            });
        }

        public override Task WriteAsync(string value)
        {
            return Task.Run(async () =>
            {
                OnBeforeWrite();
                await _writer.WriteAsync(value);
                OnAfterWrite();
            });
        }

        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            return Task.Run(async () =>
            {
                OnBeforeWrite();
                await _writer.WriteAsync(buffer, index, count);
                OnAfterWrite();
            });
        }

        public override Task WriteLineAsync(char value)
        {
            return Task.Run(async () =>
            {
                OnBeforeWrite();
                await _writer.WriteAsync(value);
                OnAfterWrite();
            });
        }

        public override Task WriteLineAsync(string value)
        {
            return Task.Run(async () =>
            {
                OnBeforeWrite();
                await _writer.WriteAsync(value);
                OnAfterWrite();
            });
        }

        public override Task WriteLineAsync(char[] buffer, int index, int count)
        {
            return Task.Run(async () =>
            {
                OnBeforeWrite();
                await _writer.WriteAsync(buffer, index, count);
                OnAfterWrite();
            });
        }

        public override Task WriteLineAsync()
        {
            return Task.Run(async () =>
            {
                OnBeforeWrite();
                await _writer.WriteLineAsync();
                OnAfterWrite();
            });
        }

        public override Task FlushAsync()
        {
            return _writer.FlushAsync();
        }

        public override IFormatProvider FormatProvider => _writer.FormatProvider;

        public override Encoding Encoding => _writer.Encoding;

        public override string NewLine
        {
            get => _writer.NewLine;
            set => _writer.NewLine = value;
        }
    }
}