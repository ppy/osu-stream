using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace StreamTester
{
    public class TextBoxStreamWriter : TextWriter
    {
        TextBox _output;
        TextBox _mod;

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
                    _mod.Invoke((MethodInvoker)delegate { _mod.AppendText(writeable.Replace("[mod] ", "")); });
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
            get { return Encoding.UTF8; }
        }
    }
}