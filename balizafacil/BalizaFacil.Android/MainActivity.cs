﻿using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.OS;
using Android.Util;
using Android.Runtime;
using Android.Content;
using System;
using BalizaFacil.Droid.Services;
using Xamarin.Forms;
using System.Threading;
using System.Threading.Tasks;
using Android.Media;
using Android;
using Xamarin.Essentials;
using Firebase;

namespace BalizaFacil.Droid
{
    [Activity(Label = "Baliza Fácil", Theme = "@style/Theme.Splash", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]

    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static MainActivity Instance;

        public MediaPlayer player;// = new MediaPlayer();     
        public void Playsong()
        {
            //if (player.IsPlaying)
            //{
            //    return;
            //}
            //else
            player.Start();
        }

        LocationService Location = new LocationService();

        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == 0x1)
            {
                if (resultCode != Result.Ok)
                {
                    await Location.RequestEnableGPS();
                }
            }

            await CheckAndRequestLocationPermission();
        }


        protected async override void OnCreate(Bundle bundle)
        {

            base.OnCreate(bundle);

            StorageService storage = new StorageService();
            BluetoothService bluetooth = new BluetoothService();
            App.BluetoothStartedEnabled = bluetooth.IsEnabled;
            Location.RequestEnableGPS();

            var a = FirebaseApp.InitializeApp(this);

          

            player = MediaPlayer.Create(this, Resource.Raw.Stop);
            Thread TurnOnAndConnect = new Thread(() =>
            {
                try
                {
                    if (!App.BluetoothStartedEnabled)
                    {
                        bluetooth.Enable();
                        Thread.Sleep(3000);
                    }
                    if (!string.IsNullOrWhiteSpace(storage.Address))
                        bluetooth.ConnectToSensor(storage.Address, true);
                }
                catch (System.Exception ex)
                {
                    string title = this.GetType().Name + " - " + System.Reflection.MethodBase.GetCurrentMethod().Name;
                    if (BalizaFacil.App.Instance != null)
                        BalizaFacil.App.Instance.UnhandledException(title, ex);
                }
            });
            TurnOnAndConnect.Priority = System.Threading.ThreadPriority.Highest;
            TurnOnAndConnect.Name = nameof(TurnOnAndConnect);

            TurnOnAndConnect.Start();


            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            //RequestPermissions(new string[] { Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation }, 0);
            Xamarin.Essentials.Platform.Init(this, bundle);
            global::Xamarin.Forms.Forms.Init(this, bundle);

            Instance = this;
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            ZXing.Net.Mobile.Forms.Android.Platform.Init();
            ZXing.Mobile.MobileBarcodeScanner.Initialize(Application);

            DisplayMetrics displayMetrics = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetRealMetrics(displayMetrics);
            App.ScreenHeight = (int)(Resources.DisplayMetrics.HeightPixels / Resources.DisplayMetrics.Density);
            App.ScreenWidth = (int)(Resources.DisplayMetrics.WidthPixels / Resources.DisplayMetrics.Density);
            App.HeightPixels = displayMetrics.HeightPixels;
            App.WidthPixels = displayMetrics.WidthPixels;
            App.IsAndroidSDKBelowMarshmallow = Build.VERSION.SdkInt < BuildVersionCodes.M;

            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnTaskSchedulerOnUnobservedTaskException;
            AndroidEnvironment.UnhandledExceptionRaiser += OnAndroidEnvironmentOnUnhandledException;

            LoadApplication(new App());
        }

        public async Task<PermissionStatus> CheckAndRequestLocationPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
                return status;

            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // Prompt the user to turn on in settings
                // On iOS once a permission has been denied it may not be requested again from the application
                return status;
            }

            if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
            {
                // Prompt the user with additional information as to why the permission is needed
            }

            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            return status;
        }



        private void OnTaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            App.Instance.UnhandledException("OnTaskSchedulerOnUnobservedTaskException", e.Exception);
        }

        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            App.Instance.UnhandledException("OnCurrentDomainUnhandledException", e.ExceptionObject as Exception);
        }

        private void OnAndroidEnvironmentOnUnhandledException(object sender, RaiseThrowableEventArgs e)
        {
            App.Instance.UnhandledException("OnAndroidEnvironmentOnUnhandledException", e.Exception);
        }


      /*  public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            global::ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        } */

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override void OnTrimMemory([GeneratedEnum] TrimMemory level)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

            base.OnTrimMemory(level);
        }

        public override void OnLowMemory()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

            base.OnLowMemory();
        }
    }
}