using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.Practices.Unity;
using Plugin.Beahat;
using Plugin.Beahat.Abstractions;
using Prism.Unity;

namespace BeahatTutorial.Droid
{
    [Activity(Label = "BeahatTutorial", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.tabs;
            ToolbarResource = Resource.Layout.toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App(new AndroidInitializer()));
        }
    }

    public class AndroidInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IUnityContainer container)
        {
            container.RegisterType<IBeahat, BeahatImplementation>(new ContainerControlledLifetimeManager());
        }
    }
}

