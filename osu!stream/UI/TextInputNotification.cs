using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Helpers;

#if iOS
using UIKit;
using osum.Support.iPhone;
using System.Drawing;
using osum.Resources;
using CoreGraphics;

namespace osum.UI
{
    class TextInputNotification : UIAlertViewDelegate
    {
        TextAlertView tav;

        BoolDelegate complete;
        public string Text;

        public TextInputNotification(string title, string defaultText, BoolDelegate complete)
        {
            tav = new TextAlertView(title, this, defaultText);
            this.complete = complete;
            tav.Show();

            AppDelegate.SetUsingViewController(true);
        }

        public override void Dismissed(UIAlertView alertView, nint buttonIndex)
        {
            Text = tav.Text;

            complete(buttonIndex == 1);
            AppDelegate.SetUsingViewController(false);
            tav.Dispose();
        }
    }

    class TextAlertView : UIAlertView
    {
        UITextField textField;
        public string Text;

        public TextAlertView(string title, UIAlertViewDelegate del, string defaultText)
            : base(title, " ", del, LocalisationManager.GetString(OsuString.Cancel), LocalisationManager.GetString(OsuString.Okay))
        {
            textField = new UITextField(new RectangleF(12, 45, 260, 25));
            textField.BackgroundColor = UIColor.White;
            textField.AutocorrectionType = UITextAutocorrectionType.No;
            textField.Text = defaultText;

            AddSubview(textField);
            //Transform = CGAffineTransform.MakeTranslation(0,130);
        }

        public override void DismissWithClickedButtonIndex(nint index, bool animated)
        {
            Text = textField.Text;
            base.DismissWithClickedButtonIndex(index, animated);
        }

        public override void Show()
        {
            textField.BecomeFirstResponder();
            base.Show();
        }
    }
}
#endif