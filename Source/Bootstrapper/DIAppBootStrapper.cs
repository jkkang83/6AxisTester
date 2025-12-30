using FZ4P.AppCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P.Bootstrapper
{
    public class DIAppBootStrapper : IAppBootStrapper
    {
        private IServiceProvider _serviceProvider;

        public void Run()
        {
            STATIC.Disable();
            ConfigurationService();
            var mainForm = _serviceProvider.GetRequiredService<F_Main>();
            Application.Run((Form)mainForm);
        }

        private void ConfigurationService()
        {
            var services = new ServiceCollection();

            //클래스 생명주기 명시
            services.AddSingleton<F_Main>();
            services.AddSingleton<AppPath>();

            _serviceProvider = services.BuildServiceProvider();
        }
    }
}
