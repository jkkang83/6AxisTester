using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.Bootstrapper
{
    public enum BootStrapperType
    {
        None = 0,
        DependencyInjection = 1,
    }
    public class AppBootstrapperFactory
    {
        public IAppBootStrapper CreateBootStrapper(BootStrapperType type)
        {
            switch (type)
            {
                case BootStrapperType.None:
                    return new DefaultAppBootStrapper();
                case BootStrapperType.DependencyInjection:
                    return new DIAppBootStrapper();
                default :
                    return new DefaultAppBootStrapper();
            }
        }
    }
}
