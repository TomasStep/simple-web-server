using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace simple_web_server
{
    public class WebServer
    {
        //Initializing HttpListener to respond to users HTTP requests
        private HttpListener listener = new HttpListener();
        //Initializing Http response information
        private readonly Func<HttpListenerRequest, string> responderMethod;
        //WebServer contructor
        public WebServer(IReadOnlyCollection<string> prefixes, Func<HttpListenerRequest, string> method)
        {
            foreach (var s in prefixes)
            {
                listener.Prefixes.Add(s);
            }

            responderMethod = method;
            listener.Start();
        }
        public WebServer(Func<HttpListenerRequest, string> method, params string[] prefixes)
           : this(prefixes, method)
        {
        }
        //Starting Web Service
        public void Run()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                Console.WriteLine("Webserver is starting...");
                try
                {
                    while (listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem(c =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                //After user ask for a request web server returns a string
                                //String is converting to bytes to create response form of a stream object
                                var rstr = responderMethod(ctx.Request);
                                var buf = Encoding.UTF8.GetBytes(rstr);
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            catch
                            {}
                            finally
                            {
                                // always close the stream
                                if (ctx != null)
                                {
                                    ctx.Response.OutputStream.Close();
                                }
                            }
                        }, listener.GetContext());
                    }
                }
                catch
                {}
            });
        }
        //Stopping Web Service
        public void Stop()
        {
            listener.Stop();
            //listener.Close();
        }
    }
    class Program
    {
        public static string SendResponse(HttpListenerRequest request)
        {
            //Servers poor main page for users, greetings and date/time
            var greetings = $"<html><body><h1>Greetings!</h1><p>This is a test web server.</br></br>The current time is {DateTime.Now}</p></body></html>";
            return string.Format(greetings);
        }
        private static void Main(string[] args)
        {
            //The main information about server
            var port = 10000;
            WebServer webServer = new WebServer(SendResponse, $"http://localhost:{port}/test/");
            webServer.Run();
            Console.WriteLine($"Hello, an example of a simple webserver.\nAddress:\nhttp://localhost:{port}/test/\nPort:{port} \nPress any key to quit...");
            Console.ReadKey();
            webServer.Stop();
        }
    }
}