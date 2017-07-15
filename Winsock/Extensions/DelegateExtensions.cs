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
            SafeRaise(evt, allow, null, EventArgs.Empty, false);
        }

        /// <summary>
        /// Tries to safely invoke on the target thread the method represented by the current delegate.
        /// </summary>
        /// <param name="evt">The <see cref="Delegate"/> to invoke.</param>
        /// <param name="allow">A boolean to indicate whether to invoke the <see cref="Delegate"/> or not (for supporting CanRaiseEvents).</param>
        /// <param name="sender">The object raising the event.</param>
        public static void SafeRaise(this Delegate evt, bool allow, object sender)
        {
            SafeRaise(evt, allow, sender, EventArgs.Empty, false);
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
            SafeRaise(evt, allow, sender, e, false);
        }

        /// <summary>
        /// Tries to safely invoke on the target thread the method represented by the current delegate.
        /// </summary>
        /// <param name="evt">The <see cref="Delegate"/> to invoke.</param>
        /// <param name="allow">A boolean to indicate whether to invoke the <see cref="Delegate"/> or not (for supporting CanRaiseEvents).</param>
        /// <param name="sender">The object raising the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> to be passed on to the event handler.</param>
        /// <param name="synchronous">Whether the event should be raised synchronously or not.</param>
        public static void SafeRaise(this Delegate evt, bool allow, object sender, EventArgs e, bool synchronous)
        {
            if (evt == null || !allow) return;

            foreach (Delegate singleCast in evt.GetInvocationList())
            {
                ISynchronizeInvoke syncInvoke = singleCast.Target as ISynchronizeInvoke;
                if (syncInvoke != null && syncInvoke.InvokeRequired)
                {
                    var result = syncInvoke.BeginInvoke(singleCast, new object[] { sender, e });
                    if (synchronous)
                    {
                        var endResult = syncInvoke.EndInvoke(result);
                    }
                }
                else
                    singleCast.DynamicInvoke(sender, e);
            }
        }

    }
}
