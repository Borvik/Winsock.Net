using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Treorisoft.Net.Utilities
{
    /// <summary>
    /// Contains a list of <see cref="Winsock"/> objects, passing events to the parent containing <see cref="Winsock"/> object.
    /// </summary>
    public class WinsockCollection : CollectionBase, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WinsockCollection"/> with the specified parent.
        /// </summary>
        /// <param name="parent">The <see cref="Winsock"/> object that should act as the parent item.</param>
        public WinsockCollection(Winsock parent)
        {
            if (parent == null) throw new ArgumentNullException("parent");
            _parent = parent;
        }

        #region Private Members

        private readonly Winsock _parent = null;
        private Dictionary<Guid, Winsock> _values = new Dictionary<Guid, Winsock>();
        private List<Guid> _forceAutoRemove = new List<Guid>();
        private Queue<Guid> _autoRemoveList = new Queue<Guid>();
        private object _syncRootRemoveList;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Winsock"/> clients should be automatically removed when they get disconnected.
        /// </summary>
        public bool AutoRemove { get; set; } = true;

        /// <summary>
        /// Gets a collection containing the keys in this <see cref="WinsockCollection"/>.
        /// </summary>
        public IEnumerable<Guid> Keys { get { return _values.Keys; } }

        /// <summary>
        /// Gets a collection containing the <see cref="Winsock"/> clients contained within this <see cref="WinsockCollection"/>.
        /// </summary>
        public IEnumerable<Winsock> Values { get { return _values.Values; } }

        /// <summary>
        /// Gets the <see cref="Winsock"/> client using the specified key.
        /// </summary>
        /// <param name="guid">A <see cref="Guid"/> key referencing the <see cref="Winsock"/> client contained in the collection.</param>
        /// <returns>A <see cref="Winsock"/> client.</returns>
        public Winsock this[Guid guid] { get { return _values[guid]; } }

        /// <summary>
        /// Gets the <see cref="Winsock"/> client at the specified index.
        /// </summary>
        /// <param name="index">The index of the <see cref="Winsock"/> client to be retrieved.</param>
        /// <returns>A <see cref="Winsock"/> client.</returns>
        public Winsock this[int index]
        {
            get
            {
                var key = _values.Keys.ElementAt(index);
                return _values[key];
            }
        }

        #endregion

        #region Private Properties

        private Winsock Parent { get { return _parent; } }
        private object SyncRootRemoveList
        {
            get
            {
                if (_syncRootRemoveList == null)
                    Interlocked.CompareExchange(ref _syncRootRemoveList, new object(), null);
                return _syncRootRemoveList;
            }
        }

        #endregion

        #region Private Methods

        private Guid Add(Guid guid, Winsock client)
        {
            AttachEvents(client);
            _values.Add(guid, client);
            List.Add(guid);
            return guid;
        }
        private void AttachEvents(Winsock client)
        {
            //client.StateChanged += Client_StateChanged;
            client.StateChanged += Parent.OnStateChanged;
            client.ErrorReceived += Parent.OnErrorReceived;
            client.DataArrival += Parent.OnDataArrival;
            client.ReceiveProgress += Parent.OnReceiveProgress;
            client.SendProgress += Parent.OnSendProgress;
            client.Connected += Parent.OnConnected;
            client.Disconnected += Client_Disconnected;
            client.Disconnected += Parent.OnDisconnected;
            client.ValidateCertificate += Parent.OnValidateCollectionCertificate;
        }
        private void DetachEvents(Winsock client)
        {
            //client.StateChanged -= Client_StateChanged;
            client.StateChanged -= Parent.OnStateChanged;
            client.ErrorReceived -= Parent.OnErrorReceived;
            client.DataArrival -= Parent.OnDataArrival;
            client.ReceiveProgress -= Parent.OnReceiveProgress;
            client.SendProgress -= Parent.OnSendProgress;
            client.Connected -= Parent.OnConnected;
            client.Disconnected -= Client_Disconnected;
            client.Disconnected -= Parent.OnDisconnected;
            client.ValidateCertificate -= Parent.OnValidateCollectionCertificate;
        }

        private void Client_Disconnected(object sender, EventArgs e)
        {
            // Close or Disconnected detected - check for AutoRemoval
            Winsock client = (Winsock)sender;
            Guid guid = FindKey(client);
            if (guid != Guid.Empty && (AutoRemove || _forceAutoRemove.Contains(guid)))
            {
                lock (SyncRootRemoveList)
                    _autoRemoveList.Enqueue(guid);
                (new Thread(ClientRemovalThread)).Start();
            }
        }

        private void ClientRemovalThread()
        {
            Thread.Sleep(50);
            Guid guid = Guid.Empty;
            lock (SyncRootRemoveList)
                guid = _autoRemoveList.Dequeue();
            Remove(guid);
        }

        private Guid FindKey(Winsock client)
        {
            foreach (Guid key in _values.Keys)
            {
                if (ReferenceEquals(_values[key], client))
                    return key;
            }
            return Guid.Empty;
        }


        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a client to the collection.
        /// </summary>
        /// <param name="client">The client to add to the collection.</param>
        /// <returns>A <see cref="Guid"/> key to help identify this particular client.</returns>
        public Guid Add(Winsock client)
        {
            return Add(Guid.NewGuid(), client);
        }
        /// <summary>
        /// Removes the specified client from the collection.
        /// </summary>
        /// <param name="client">The client to remove from the collection.</param>
        public void Remove(Winsock client)
        {
            Guid guid = FindKey(client);
            Remove(guid);
        }
        /// <summary>
        /// Removes the specified client from the collection.
        /// </summary>
        /// <param name="guid">The <see cref="Guid"/> key that identifies the connection to remove.</param>
        public void Remove(Guid guid)
        {
            if (guid == Guid.Empty || !_values.ContainsKey(guid))
                return;

            DetachEvents(_values[guid]);
            _values[guid].Dispose();
            _values.Remove(guid);
            List.Remove(guid);
        }

        /// <summary>
        /// Sends an object to all the connected <see cref="Winscok"/> clients contained in this collection.
        /// </summary>
        /// <param name="data">The <see cref="object"/> to be serialized and sent.</param>
        public void SendToAll(object data)
        {
            foreach (var wsk in _values.Values)
                wsk.Send(data);
        }

        /// <summary>
        /// Sends a string to all the connected <see cref="Winsock"/> clients contained in this collection.
        /// </summary>
        /// <param name="data">The <see cref="string"/> data to be sent.</param>
        public void SendToAll(string data)
        {
            foreach (var wsk in _values.Values)
                wsk.Send(data);
        }

        /// <summary>
        /// Sends a sequence of bytes to all connected <see cref="Winsock"/> clients contained in this collection.
        /// </summary>
        /// <param name="data">A <see cref="byte"/> array containing the data to be sent.</param>
        public void SendToAll(byte[] data)
        {
            foreach (var wsk in _values.Values)
                wsk.Send(data);
        }

        /// <summary>
        /// Sends a file to all the connected <see cref="Winsock"/> clients contained in this collection.
        /// </summary>
        /// <param name="fileName">The full path of the file to send.</param>
        public void SendFileToAll(string fileName)
        {
            SendFileToAll(new FileInfo(fileName));
        }

        /// <summary>
        /// Sends a file to all the connected <see cref="Winsock"/> clients contained in this collection.
        /// </summary>
        public void SendFileToAll(FileInfo fileInfo)
        {
            if (fileInfo == null) throw new ArgumentNullException("fileInfo");
            if (!fileInfo.Exists) throw new FileNotFoundException(fileInfo.FullName);
            foreach (var wsk in _values.Values)
                wsk.SendFile(fileInfo);
        }

        /// <summary>
        /// Closes all the connected <see cref="Winsock"/> clients contained in this collection.
        /// </summary>
        /// <remarks>Alias for <see cref="CollectionBase.Clear"/>.</remarks>
        public void CloseAll()
        {
            Clear();
        }

        #endregion

        #region Method Overrides

        int _preClearCount = -1;
        protected override void OnClear()
        {
            _preClearCount = List.Count;
            base.OnClear();
        }
        protected override void OnClearComplete()
        {
            base.OnClearComplete();
            foreach (var guid in Keys)
            {
                _forceAutoRemove.Add(guid);
                _values[guid].Close();
            }
            Parent.OnClientCountChanged(this);
        }
        protected override void OnInsertComplete(int index, object value)
        {
            base.OnInsertComplete(index, value);
            Parent.OnClientCountChanged(this);
        }
        protected override void OnRemoveComplete(int index, object value)
        {
            base.OnRemoveComplete(index, value);
            Parent.OnClientCountChanged(this);
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var client in Values)
                    {
                        DetachEvents(client);
                        client.Dispose();
                    }
                }
                disposedValue = true;
            }
        }

        /// <summary>
        /// Releases all resources used by the <see cref="WinsockCollection"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
