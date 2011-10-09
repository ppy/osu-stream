using System;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace ConsoleRedirection
{
    public class TextBoxStreamWriter : TextWriter
    {
        TextBox _output = null;

        public TextBoxStreamWriter(TextBox output)
        {
            _output = output;
            Console.SetOut(this);
        }

        StringBuilder buffer = new StringBuilder();

        public override void Write(char value)
        {
            base.Write(value);
            buffer.Append(value);
            if (value == '\n')
            {
                _output.Invoke((MethodInvoker)delegate
                {
                    _output.AppendText(buffer.ToString()); // When character data is written, append it to the text box.
                });
                buffer = new StringBuilder();
            }
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}