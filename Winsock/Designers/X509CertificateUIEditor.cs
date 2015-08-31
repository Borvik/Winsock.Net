using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Design;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Treorisoft.Net.Designers
{
    /// <summary>
    /// Provides a user interface for selecting an <see cref="X509Certificate"/>.
    /// </summary>
    public class X509CertificateUIEditor : UITypeEditor, IDisposable
    {
        /// <summary>
        /// Field to store a cached copy of the <see cref="OpenFileDialog"/> referenced used as an editor.
        /// </summary>
        private FileDialog fileDialog;

        /// <summary>
        /// Gets the editing style used by the <see cref="EditValue"/> method.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that can be used to gain additional context information.</param>
        /// <returns>One of the <see cref="UITypeEditorEditStyle"/> values indicating the provided editing style.</returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        /// <summary>
        /// Edits the specified object using the editor style provided by the <see cref="GetEditStyle"/> method.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that can be used to gain additional context information.</param>
        /// <param name="provider">A service provider object through which editing services may be obtained.</param>
        /// <param name="value">An instance of the value being edited.</param>
        /// <returns>The new value of the object. If the value of the object hasn't changed, this should return the same object it was passed.</returns>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider == null) return value;
            var service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            if (service == null) return value;

            if (fileDialog == null)
            {
                fileDialog = new OpenFileDialog();
                fileDialog.Filter = GetFilterString();
            }
            try
            {
                if (fileDialog.ShowDialog() == DialogResult.OK)
                    value = X509Certificate.CreateFromCertFile(fileDialog.FileName);
            }
            finally { }
            return value;
        }

        /// <summary>
        /// Gets a list of the acceptable extensions for a <see cref="X509Certificate"/>.
        /// </summary>
        /// <returns>The list of acceptable extensions.</returns>
        protected virtual string[] GetExtensions()
        {
            return new string[] { "der", "pem", "crt", "cer", "key" };
        }

        /// <summary>
        /// Gets the user friendly name for the extension filter.
        /// </summary>
        /// <returns>The user friendly name for the filter.</returns>
        protected virtual string GetFilterDescription()
        {
            return "Common Certificates";
        }

        /// <summary>
        /// Gets the file extension filter string (no user friendly name).
        /// </summary>
        /// <param name="delim">The delimiter to put between the acceptable extensions.</param>
        /// <returns>The delimited filter string.</returns>
        protected string GetExtFilterString(string delim)
        {
            string[] exts = GetExtensions();
            if (exts == null || exts.Length == 0) return null;
            return "*." + string.Join(delim + "*.", exts);
        }

        /// <summary>
        /// Gets the full file extension filter string, including the friendly name.
        /// </summary>
        /// <returns>The file extension filter string.</returns>
        protected string GetFilterString()
        {
            return string.Format("{0}({1})|{2}",
                GetFilterDescription(),
                GetExtFilterString(","),
                GetExtFilterString(";"));
        }

        #region IDisposable Support

        /// <summary>
        /// Field used to detect multiple calls to Dispose.
        /// </summary>
        private bool disposedValue = false;

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="X509CertificateUIEditor"/> class.
        /// </summary>
        /// <param name="disposing">A boolean indicating whether Dispose was called from the Disose() method, or from the class Destructor.</param>
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "fileDialog", Justification = "Disposed using ?.Dispose().")]
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    fileDialog?.Dispose();
                }
                disposedValue = true;
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="X509CertificateUIEditor"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
