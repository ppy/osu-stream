using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osum.Support
{
    public interface IUpdateable
    {
        /// <summary>
        /// Updates this instance. Called every frame when loaded as a component.
        /// </summary>
        void Update();
    }
}
