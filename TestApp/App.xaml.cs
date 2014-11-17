using System.Windows;
using Microsoft.Office.Interop.Outlook;
using Moq;
using OutlookTfs;
using Application = System.Windows.Application;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // init stubs
            var fakeExplorer = new Mock<Explorer>();
            fakeExplorer.SetupAllProperties();
            var fakeMailItem = new Mock<MailItem>();
            fakeMailItem.SetupAllProperties();
            fakeMailItem.Object.Subject = "test subj";
            fakeMailItem.Object.Body = "test body";
            fakeMailItem.SetupGet(m => m.Attachments).Returns(Mock.Of<Attachments>());
            var _container = new SimpleContainer()
                .RegisterSingle(fakeExplorer)
                .Register<IView>(container => new NewWorkItem())
                .Register<AppViewModel>(container => new AppViewModel())
                .Register<IPresenter>(container => new Presenter(container)
                {
                    View = container.Create<IView>(),
                    ViewModel = container.Create<AppViewModel>()
                });
            
            var form = _container.Create<IPresenter>();
            form.Initialize(fakeMailItem.Object);
        }
    }

}
