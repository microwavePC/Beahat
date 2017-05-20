using Microsoft.Practices.Unity;
using Plugin.Beahat;
using Plugin.Beahat.Abstractions;
using Prism.Unity;

namespace BeahatTutorial.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();
            LoadApplication(new BeahatTutorial.App(new UwpInitializer()));
        }
    }

    public class UwpInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IUnityContainer container)
        {
            container.RegisterType<IBeahat, BeahatImplementation>(new ContainerControlledLifetimeManager());
        }
    }

}
