using System;
using System.Drawing;
using System.Windows.Forms;

namespace osu_common.Helpers
{
    public class pForm : Form
    {
        private static Font _DefaultFont;
        private bool _FontSet;

        /// <summary>
        /// This member overrides <see cref="Control.Font"/>.
        /// </summary>
        public override Font Font
        {
            get
            {
                if (_FontSet)
                    return base.Font;
                else
                    return DefaultFont;
            }
            set
            {
                // Determine if we will need to raise the FontChanged event.
                // We will need to raise the event manually if the "base" Font is 
                // equal to the specified value and the font has not been set.
                bool raiseChangedEvent = false;
                if ((!_FontSet) && (base.Font == value))
                    raiseChangedEvent = true;

                // Change the Font property
                _FontSet = true;
                base.Font = value;

                if (raiseChangedEvent)
                    OnFontChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the default font of the control.
        /// </summary>
        /// <value>The default <see cref="Font"/> of the control. The default is the dialog font currently in use by the user's operating system.</value>
        public new static Font DefaultFont
        {
            get
            {
                if (_DefaultFont == null)
                {
                    if ((Environment.OSVersion.Platform == PlatformID.Win32NT) &&
                        (Environment.OSVersion.Version.Major >= 5))
                    {
                        // Use special 'MS Shell Dlg 2' font on Win2000+ platforms.
                        _DefaultFont = new Font("MS Shell Dlg 2", Control.DefaultFont.Size);
                    }
                    else
                    {
                        // Use special 'MS Shell Dlg' font on all other platforms.
                        _DefaultFont = new Font("MS Shell Dlg", Control.DefaultFont.Size);
                    }
                }
                return _DefaultFont;
            }
        }

        /// <summary>
        /// Indicates whether the <see cref="Control.Font">Font</see> property should be persisted.
        /// </summary>
        /// <returns><see langword="true"/> if the property value has changed from its default; otherwise, <see langword="false"/>.</returns>
        private bool ShouldSerializeFont()
        {
            if (_FontSet)
                return true;
            else
                return false;
        }

        /// <summary>
        /// This member overrides <see cref="Control.ResetFont"/>.
        /// </summary>
        public override void ResetFont()
        {
            if (_FontSet)
            {
                _FontSet = false;
                OnFontChanged(EventArgs.Empty);
            }
        }
    }
}