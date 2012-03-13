using System;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace ConsoleRedirection
{
    public class TextBoxStreamWriter : TextWriter
    {
        TextBox _output = null;
        TextBox _mod = null;

        public TextBoxStreamWriter(TextBox output, TextBox mod)
        {
            _output = output;
            _mod = mod;
            Console.SetOut(this);
        }

        StringBuilder buffer = new StringBuilder();

        public override void Write(char value)
        {
            base.Write(value);
            buffer.Append(value);

            if (value == '\n' || value == '.')
            {
                string writeable = buffer.ToString();

                if (writeable.StartsWith("[mod]"))
                {
                    _mod.Invoke((MethodInvoker)delegate { _mod.AppendText(writeable.Replace("[mod] ","")); });
                }
                else
                {
                    _output.Invoke((MethodInvoker)delegate { _output.AppendText(writeable); });
                }
                buffer = new StringBuilder();
            }
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}