/*****************************************************************************
*                                                                            *
* HID USB DRIVER - FLORIAN LEITNER                                           *
* Copyright 2007 - Florian Leitner | http://www.florian-leitner.de           *
* mail@florian-leitner.de                                                    *
*                                                                            *   
* This file is part of HID USB DRIVER.                                       *
*                                                                            *
*   HID USB DRIVER is free software; you can redistribute it and/or modify   *
*   it under the terms of the GNU General Public License 3.0 as published by *
*   the Free Software Foundation;                                            *
*   HID USB DRIVER is distributed in the hope that it will be useful,        *
*   but WITHOUT ANY WARRANTY; without even the implied warranty of           *
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the            *
*   GNU General Public License for more details.                             *
*   You should have received a copy of the GNU General Public License        *
*   along with this program.  If not, see <http://www.gnu.org/licenses/>.    *
*                                                                            *
******************************************************************************/
//---------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace USBHIDDRIVER.List
{
    /// <summary>
    /// A class that works just like ArrayList, but sends event
    /// notifications whenever the list changes
    /// </summary>
    public class ListWithEvent : System.Collections.ArrayList
    {

        /// <summary>
        /// An event that clients can use to be notified whenever the
        /// elements of the list change
        /// </summary>
        public event System.EventHandler Changed;

        /// <summary>
        /// Invoke the Changed event; called whenever list changes
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnChanged(System.EventArgs e)
        {
            if (Changed != null)
            {
                Changed(this, e);
            }
        }

        // Override some of the methods that can change the list;
        // invoke event after each:
        /// <summary>
        /// Fügt am Ende von <see cref="T:System.Collections.ArrayList"></see> ein Objekt hinzu.
        /// </summary>
        /// <param name="value">Das <see cref="T:System.Object"></see>, das am Ende der <see cref="T:System.Collections.ArrayList"></see> hinzugefügt werden soll. Der Wert kann null sein.</param>
        /// <returns>
        /// Der <see cref="T:System.Collections.ArrayList"></see>-Index, an dem value hinzugefügt wurde.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException"><see cref="T:System.Collections.ArrayList"></see> ist schreibgeschützt.- oder -<see cref="T:System.Collections.ArrayList"></see> hat eine feste Größe. </exception>
        public override int Add(object value)
        {
            int i = base.Add(value);
            OnChanged(System.EventArgs.Empty);
            return i;
        }
    
    }

    
}
    

