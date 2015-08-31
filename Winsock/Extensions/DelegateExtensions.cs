using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treorisoft.Net
{
    internal static class DelegateExtensions
    {

        /// <summary>
        /// Tries to safely invoke on the target thread the method represented by the current delegate.
        /// </summary>
        /// <param name="evt">The <see cref="Delegate"/> to invoke.</param>
        /// <param name="allow">A boolean to indicate whether to invoke the <see cref="Delegate"/> or not (for supporting CanRaiseEvents).</param>
        public static void SafeRaise(this Delegate evt, bool allow)
        {
            SafeRaise(evt, allow, null, EventArgs.Empty);
        }

        /// <summary>
        /// Tries to safely invoke on the target thread the method represented by the current delegate.
        /// </summary>
        /// <param name="evt">The <see cref="Delegate"/> to invoke.</param>
        /// <param name="allow">A boolean to indicate whether to invoke the <see cref="Delegate"/> or not (for supporting CanRaiseEvents).</param>
        /// <param name="sender">The object raising the event.</param>
        public static void SafeRaise(this Delegate evt, bool allow, object sender)
        {
            SafeRaise(evt, allow, sender, EventArgs.Empty);
        }

        /// <summary>
        /// Tries to safely invoke on the target thread the method represented by the current delegate.
        /// </summary>
        /// <param name="evt">The <see cref="Delegate"/> to invoke.</param>
        /// <param name="allow">A boolean to indicate whether to invoke the <see cref="Delegate"/> or not (for supporting CanRaiseEvents).</param>
        /// <param name="sender">The object raising the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> to be passed on to the event handler.</param>
        public static void SafeRaise(this Delegate evt, bool allow, object sender, EventArgs e)
        {
            if (evt == null || !allow) return;

            foreach (Delegate singleCast in evt.GetInvocationList())
            {
                ISynchronizeInvoke syncInvoke = singleCast.Target as ISynchronizeInvoke;
                if (syncInvoke != null && syncInvoke.InvokeRequired)
                    syncInvoke.BeginInvoke(singleCast, new object[] { sender, e });
                else
                    singleCast.DynamicInvoke(sender, e);
            }
        }

    }
}
