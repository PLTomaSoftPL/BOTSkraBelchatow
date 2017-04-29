using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using System.Timers;
namespace GksKatowiceBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);


            //Helpers.BaseDB.AddToLog("Wywołanie metody Application_Start");
            //Controllers.ThreadClass.SendThreadMessage();
            //var aTimer = new System.Timers.Timer();
            //aTimer.Interval = 3 * 60 * 1000;

            //aTimer.Elapsed += OnTimedEvent;
            //aTimer.Start();
        }
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
           // Controllers.ThreadClass.SendThreadMessage();
        }
    }
}
