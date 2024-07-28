using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using Android.Hardware;
using Android.Util;
using AndroidX.Core.Content;
using AndroidX.Core.App;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using ResoniteHRM;

namespace com.LinkaIndustries.ResoniteHRM
{
    [Activity(Label = "@string/app_name", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, Theme = "@android:style/Theme.DeviceDefault")]
    public class MainActivity : Activity
    {
        private const string TAG = "MainActivity";
        private SensorManagerHelper sensorManagerHelper;
        private HttpServerManager httpServerManager;
        private PermissionsManager permissionsManager;
        private Button startServerButton;
        private Button stopServerButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(global::ResoniteHRM.Resource.Layout.activity_main);

            var heartRateTextView = FindViewById<TextView>(global::ResoniteHRM.Resource.Id.heart_rate_text);
            var ipTextView = FindViewById<TextView>(global::ResoniteHRM.Resource.Id.ip_text);
            var portTextView = FindViewById<TextView>(global::ResoniteHRM.Resource.Id.port_text);
            startServerButton = FindViewById<Button>(global::ResoniteHRM.Resource.Id.start_server_button);
            stopServerButton = FindViewById<Button>(global::ResoniteHRM.Resource.Id.stop_server_button);

            sensorManagerHelper = new SensorManagerHelper(this, heartRateTextView);
            httpServerManager = new HttpServerManager(this, ipTextView, portTextView, sensorManagerHelper);
            permissionsManager = new PermissionsManager(this, sensorManagerHelper, httpServerManager);

            // Request permissions
            permissionsManager.RequestPermissions();

            // Set button click handlers
            startServerButton.Click += (sender, e) => httpServerManager.StartServer();
            stopServerButton.Click += (sender, e) => httpServerManager.StopServer();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            permissionsManager.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnDestroy()
        {
            httpServerManager?.StopServer();
            base.OnDestroy();
        }
    }
}
