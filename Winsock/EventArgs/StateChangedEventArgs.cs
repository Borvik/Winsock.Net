using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treorisoft.Net
{
    /// <summary>
    /// Provides data for the <see cref="Winsock"/>.StateChanged event.
    /// </summary>
    public class StateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateChangedEventArgs"/> class.
        /// </summary>
        /// <param name="oldState">The old state of the <see cref="Winsock"/> component.</param>
        /// <param name="newState">The new state of the <see cref="Winsock"/> component.</param>
        public StateChangedEventArgs(State oldState, State newState)
        {
            OldState = oldState;
            NewState = newState;
        }

        /// <summary>
        /// Gets the old state of the <see cref="Winsock"/> object.
        /// </summary>
        public State OldState { get; private set; }

        /// <summary>
        /// Gets the new state of the <see cref="Winsock"/> object.
        /// </summary>
        public State NewState { get; private set; }
    }
}
