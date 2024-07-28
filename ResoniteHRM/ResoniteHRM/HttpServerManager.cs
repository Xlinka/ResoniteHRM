using Android.App;
using Android.Util;
using Android.Widget;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace com.LinkaIndustries.ResoniteHRM
{
    public class HttpServerManager
    {
        private readonly Activity activity;
        private readonly TextView ipTextView;
        private readonly TextView portTextView;
        private readonly SensorManagerHelper sensorManagerHelper;
        private HttpListener httpListener;
        private const string TAG = "HttpServerManager";
        private const int port = 3000;
        private string ipAddress;

        public HttpServerManager(Activity activity, TextView ipTextView, TextView portTextView, SensorManagerHelper sensorManagerHelper)
        {
            this.activity = activity;
            this.ipTextView = ipTextView;
            this.portTextView = portTextView;
            this.sensorManagerHelper = sensorManagerHelper;
        }

        public void StartServer()
        {
            try
            {
                ipAddress = GetLocalIPAddress();
                string prefix = $"http://{ipAddress}:{port}/heart_rate/";
                httpListener = new HttpListener();
                httpListener.Prefixes.Add(prefix);
                httpListener.Start();
                Log.Info(TAG, $"HTTP server started at {prefix}");
                activity.RunOnUiThread(() =>
                {
                    ipTextView?.SetText($"IP: {ipAddress}", TextView.BufferType.Normal);
                    portTextView?.SetText($"Port: {port}", TextView.BufferType.Normal);
                    Toast.MakeText(activity, "HTTP Server Started", ToastLength.Long).Show();
                });

                Task.Run(() =>
                {
                    try
                    {
                        while (httpListener.IsListening)
                        {
                            var context = httpListener.GetContext();
                            ProcessRequest(context);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(TAG, $"HTTP Server Task: Exception - {ex}");
                    }
                });
            }
            catch (HttpListenerException hlex)
            {
                Log.Error(TAG, $"StartServer: HttpListenerException - {hlex.Message}");
                activity.RunOnUiThread(() => Toast.MakeText(activity, $"Failed to start HTTP Server: {hlex.Message}", ToastLength.Long).Show());
            }
            catch (SocketException sex)
            {
                Log.Error(TAG, $"StartServer: SocketException - {sex.Message}");
                activity.RunOnUiThread(() => Toast.MakeText(activity, $"Failed to start HTTP Server: {sex.Message}", ToastLength.Long).Show());
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"StartServer: Exception - {ex.Message}");
                activity.RunOnUiThread(() => Toast.MakeText(activity, $"Failed to start HTTP Server: {ex.Message}", ToastLength.Long).Show());
            }
        }

        public void StopServer()
        {
            if (httpListener != null && httpListener.IsListening)
            {
                httpListener.Stop();
                httpListener.Close();
                Log.Info(TAG, "HTTP server stopped.");
                activity.RunOnUiThread(() =>
                {
                    ipTextView?.SetText("IP: --", TextView.BufferType.Normal);
                    portTextView?.SetText("Port: --", TextView.BufferType.Normal);
                    Toast.MakeText(activity, "HTTP Server Stopped", ToastLength.Long).Show();
                });
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                var response = context.Response;
                string responseString = $"{{ \"heart_rate\": {sensorManagerHelper.CurrentHeartRate} }}";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"ProcessRequest: Exception - {ex}");
            }
        }

        private string GetLocalIPAddress()
        {
            try
            {
                foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (networkInterface.OperationalStatus == OperationalStatus.Up)
                    {
                        foreach (var unicastAddress in networkInterface.GetIPProperties().UnicastAddresses)
                        {
                            if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                return unicastAddress.Address.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"GetLocalIPAddress: Exception - {ex}");
            }

            return "127.0.0.1"; // Default fallback
        }
    }
}
