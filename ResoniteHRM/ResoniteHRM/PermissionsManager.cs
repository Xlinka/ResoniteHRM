using Android;
using Android.App;
using Android.Content.PM;
using Android.Util;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.Core.App;
using System;

namespace com.LinkaIndustries.ResoniteHRM
{
    public class PermissionsManager
    {
        private readonly Activity activity;
        private readonly SensorManagerHelper sensorManagerHelper;
        private readonly HttpServerManager httpServerManager;
        private const string TAG = "PermissionsManager";

        public PermissionsManager(Activity activity, SensorManagerHelper sensorManagerHelper, HttpServerManager httpServerManager)
        {
            this.activity = activity;
            this.sensorManagerHelper = sensorManagerHelper;
            this.httpServerManager = httpServerManager;
        }

        public void RequestPermissions()
        {
            if (ContextCompat.CheckSelfPermission(activity, Manifest.Permission.BodySensors) != Permission.Granted ||
                ContextCompat.CheckSelfPermission(activity, Manifest.Permission.Internet) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(activity, new string[] { Manifest.Permission.BodySensors, Manifest.Permission.Internet }, 0);
            }
            else
            {
                InitializeApp();
            }
        }

        public void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            bool allPermissionsGranted = true;
            for (int i = 0; i < grantResults.Length; i++)
            {
                if (grantResults[i] != Permission.Granted)
                {
                    allPermissionsGranted = false;
                    break;
                }
            }

            if (allPermissionsGranted)
            {
                InitializeApp();
            }
            else
            {
                Toast.MakeText(activity, "Required permissions not granted", ToastLength.Long).Show();
                activity.Finish(); // Close the app if permissions are not granted, perms for the poor
            }
        }

        private void InitializeApp()
        {
            try
            {
                sensorManagerHelper.Initialize();
                httpServerManager.StartServer();
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"InitializeApp: Exception - {ex}");
                activity.RunOnUiThread(() => Toast.MakeText(activity, "Failed to start the application", ToastLength.Long).Show());
            }
        }
    }
}
