using Android.App;
using Android.Hardware;
using Android.Util;
using Android.Widget;
using System;

namespace com.LinkaIndustries.ResoniteHRM
{
    public class SensorManagerHelper : Java.Lang.Object, ISensorEventListener
    {
        private readonly Activity activity;
        private readonly TextView heartRateTextView;
        private SensorManager sensorManager;
        private Sensor heartRateSensor;
        private const string TAG = "SensorManagerHelper";
        public int CurrentHeartRate { get; private set; }

        public SensorManagerHelper(Activity activity, TextView heartRateTextView)
        {
            this.activity = activity;
            this.heartRateTextView = heartRateTextView;
        }

        public void Initialize()
        {
            sensorManager = (SensorManager)activity.GetSystemService(Android.Content.Context.SensorService);
            if (sensorManager == null)
            {
                Log.Error(TAG, "Initialize: SensorManager initialization failed");
                throw new Exception("SensorManager initialization failed");
            }

            heartRateSensor = sensorManager.GetDefaultSensor(SensorType.HeartRate);
            if (heartRateSensor == null)
            {
                Log.Error(TAG, "Initialize: Heart rate sensor not available");
                activity.RunOnUiThread(() => heartRateTextView.Text = activity.Resources.GetString(global::ResoniteHRM.Resource.String.heart_rate_sensor_not_available));
            }
            else
            {
                sensorManager.RegisterListener(this, heartRateSensor, SensorDelay.Normal);
                Log.Info(TAG, "Initialize: Heart rate sensor registered");
            }
        }

        public void OnSensorChanged(SensorEvent e)
        {
            try
            {
                if (e?.Sensor?.Type == SensorType.HeartRate)
                {
                    CurrentHeartRate = (int)e.Values[0];
                    activity.RunOnUiThread(() => heartRateTextView?.SetText($"Heart Rate: {CurrentHeartRate}", TextView.BufferType.Normal));
                    Log.Info(TAG, $"OnSensorChanged: Heart Rate: {CurrentHeartRate}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(TAG, $"OnSensorChanged: Exception - {ex}");
            }
        }

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
        }

        public void UnregisterListener()
        {
            sensorManager?.UnregisterListener(this, heartRateSensor);
        }
    }
}
