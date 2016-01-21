using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Wpf2Html5.StockObjects
{
    /// <summary>
    /// Baseclass for view models using HTML5.
    /// </summary>
    /// <remarks>This class serves as base class for components that communicate with 
    /// an HTTP server using JSON messages. It provides basic methods for asynchronous 
    /// communication and deserialization.</remarks>
    [GeneratorIgnore]
    public class WebViewModelBase : DependencyObject
    {
        #region Private Fields

        private CookieContainer _cookies = new CookieContainer();
#if JSON_SERIALIZATION_BINDER
        private Newtonsoft.Json.SerializationBinder _binder = null; // new WebUISerializationBinder();
#else
        private SerializationBinder _binder = null; // new WebUISerializationBinder();
#endif

        #endregion

        #region Public Properties

        /// <summary>The server URL.</summary>
        public string Location { get; set; }

        public CookieContainer Cookies { get { return _cookies; } }

        #endregion

        #region Construction

        public WebViewModelBase()
        {
        }

        #endregion

        #region Diagnostics

        protected virtual bool ShowIO { get; set; }

        protected virtual bool ShowMessage { get; set; }

        protected virtual void Trace(string format, params object[] args)
        {
            Debug.WriteLine(format, args);
        }

        #endregion

        #region Public Methods

        public async virtual void Initialize()
        {
            try
            {
                var wreq = WebRequest.CreateHttp(Location);
                wreq.Method = "GET";
                wreq.CookieContainer = _cookies;
                var wres = await wreq.GetResponseAsync();

                InitializeComplete();
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => InitializeError(ex.Message));
            }
        }

        /// <summary>
        /// Post a JSON request to the server asynchronously.
        /// </summary>
        /// <param name="msg">The message object to be serialized and sent to the server.</param>
        public async virtual void SendMessage(object msg)
        {
            await Transact(msg);
        }

        public bool ValidateFileName(string filename)
        {
            return filename.Length > 0;
        }

        public void SetFocusToOverlay(object col)
        {
        }


        #endregion

        #region Overrideables

        /// <summary>Initializes the serialization binder for this object.</summary>
        /// <returns>The serialization binder provided by the implementation.</returns>
        protected virtual SerializationBinder InitializeSerializationBinder()
        {
            return null;
        }

        protected virtual void InitializeError(string p)
        {
        }

        protected virtual void InitializeComplete()
        { 
        }

        /// <summary>Called when a message is received.</summary>
        /// <param name="msg">The deserialized message object.</param>
        protected virtual void ReceiveMessage(object msg)
        {

        }

        /// <summary>Deserializes JSON data.</summary>
        /// <param name="json">JSON string.</param>
        /// <returns>The deserialized object.</returns>
        protected virtual object ParseMessage(string json)
        {
            return JsonConvert.DeserializeObject(json, GetSerializerSettings());
        }

        /// <summary>Called when a communication error occurs.</summary>
        /// <param name="message">Textual description of the error.</param>
        protected virtual void TransactError(string message)
        {
        }

        /// <summary>Populates a view model with properties from a message object.</summary>
        /// <param name="message">The message object where to take properties from.</param>
        /// <param name="viewmodel">The view model where to apply the properties.</param>
        protected virtual void PopulateViewModel(object message, object viewmodel)
        {
            var fromtype = message.GetType();
            var totype = viewmodel.GetType();
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty;

            foreach (var pifrom in fromtype.GetProperties(flags))
            {
                var pito = totype.GetProperty(pifrom.Name, flags);
                if (null == pito)
                {
                    Trace("property '{0}' not found on [{1}].", pifrom.Name, totype.FullName);
                    continue;
                }

                try
                {
                    var value = pifrom.GetValue(message);
                    pito.SetValue(viewmodel, value);
                }
                catch (Exception ex)
                {
                    Trace("failed to set property '{0}': {1}", pifrom.Name, ex.Message);
                }
            }
        }

        /// <summary>Installs the change notification reader (MVJY5JKUV6).</summary>
        /// <param name="subpath"></param>
        protected virtual void InstallNotificationReader(string subpath)
        {
            throw new NotImplementedException();
        }

        protected virtual List<T> ConvertList<T>(IEnumerable<T> list)
        {
            return list.ToList();
        }

        protected virtual List<T> CopyList<T>(IEnumerable<T> list)
        {
            return list.ToList();
        }

        public void ClearBindings(DependencyObject d)
        {
        }

        public void BindingStats()
        { }

        public void Reload()
        {

        }

        public void PrintObject(string header, object obj)
        {

        }

        public bool ConfirmMessageBox(string msg)
        {
            return true;
        }

        protected virtual void HistoryNavigation(string viewstate, object id)
        {

        }

        protected virtual void ActivateUserInterface()
        {

        }

        protected void RedirectToURL(string p)
        {
            throw new NotImplementedException();
        }

        protected string EscapeString(object arg)
        {
            throw new NotImplementedException();
        }

        public bool IsNullOrEmpty(string p)
        {
            return string.IsNullOrEmpty(p);
        }

        #endregion

        #region Private Methods

        private JsonSerializerSettings GetSerializerSettings()
        {
            if(null == _binder)
            {
                if(null == (_binder = InitializeSerializationBinder()))
                {
                    throw new Exception("serialization binder is not set, override InitializeSerializationBinder.");
                }
            }

            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Objects;
            settings.Formatting = Formatting.Indented;
            settings.Binder = _binder;
            return settings;
        }

        private async Task Transact(object request)
        {
            try
            {
                var wreq = WebRequest.CreateHttp(Location);
                wreq.Method = "POST";
                wreq.CookieContainer = _cookies;

                using (var sreq = await wreq.GetRequestStreamAsync())
                {
                    var text = JsonConvert.SerializeObject(request);

                    var data = Encoding.UTF8.GetBytes(text);
                    await sreq.WriteAsync(data, 0, data.Length);
                }

                // read response
                object rdata = null;
                using (var wres = await wreq.GetResponseAsync())
                using (var sres = wres.GetResponseStream())
                {
                    var ms = new MemoryStream();
                    await sres.CopyToAsync(ms);

                    ms.Position = 0;
                    using (var sreader = new StreamReader(ms))
                    {
                        var sdata = sreader.ReadToEnd();
                        rdata = ParseMessage(sdata);
                    }
                }

                Dispatcher.Invoke(() => ReceiveMessage(rdata));
            }
            catch(Exception ex)
            {
                Dispatcher.Invoke(() => TransactError(ex.Message));
            }
        }

        #endregion

    }
}
