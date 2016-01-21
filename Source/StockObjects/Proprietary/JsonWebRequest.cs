using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Wpf2Html5.StockObjects
{
    /// <summary>
    /// An AJAX style HTTP request using JSON format.
    /// </summary>
    [GeneratorIgnore]
    public class JsonWebRequest : DependencyObject
    {
        /// <summary>
        /// The server URL.
        /// </summary>
        public string Location { get; set; }

        public CookieContainer Cookies { get; private set; }

        public Func<string, object> Parse { get; set; }

        /// <summary>Called when a communication error occurs.</summary>
        /// <param name="message">Textual description of the error.</param>
        public Action<string> TransactError { get; set; }

        public JsonWebRequest(WebViewModelBase parent)
        {
            Cookies = parent.Cookies;
        }

        public async virtual void Get(string url, Action<object> receive)
        {
            try
            {
                var wreq = WebRequest.CreateHttp(url);
                wreq.Method = "GET";
                wreq.CookieContainer = Cookies;

                await Transact(wreq, receive);
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => TransactError(ex.Message));
            }
        }

        /// <summary>
        /// Post a JSON request to the server asynchronously.
        /// </summary>
        /// <param name="msg">The message object to be serialized and sent to the server.</param>
        public async virtual void Send(object msg, Action<object> receive)
        {
            try
            {
                var wreq = WebRequest.CreateHttp(Location);
                wreq.Method = "POST";
                wreq.CookieContainer = Cookies;

                using (var sreq = await wreq.GetRequestStreamAsync())
                {
                    var text = JsonConvert.SerializeObject(msg);
                    var data = Encoding.UTF8.GetBytes(text);
                    await sreq.WriteAsync(data, 0, data.Length);
                }

                await Transact(wreq, receive);
            }
            catch (Exception ex)
            {
                if (null != TransactError)
                {
                    Dispatcher.Invoke(() => TransactError(ex.Message));
                }
            }
        }

        private async Task Transact(WebRequest wreq, Action<object> receive)
        {

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

                    if (null != Parse)
                    {
                        rdata = Parse(sdata);
                    }
                    else
                    {
                        rdata = sdata;
                    }
                }
            }

            Dispatcher.Invoke(() => receive(rdata));
        }
    }
}
