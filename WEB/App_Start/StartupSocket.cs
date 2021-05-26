using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(WEB.App_Start.StartupSocket))]

namespace WEB.App_Start
{
    public class StartupSocket
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}
