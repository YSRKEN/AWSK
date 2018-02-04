using Microsoft.Practices.Unity;
using Prism.Unity;
using System.Windows;
using AWSK.Views;

namespace AWSK
{
	internal class Bootstrapper : UnityBootstrapper
	{
		protected override DependencyObject CreateShell()
			=> Container.Resolve<MainView>();
		protected override void InitializeShell()
			=> Application.Current.MainWindow.Show();
	}
}
