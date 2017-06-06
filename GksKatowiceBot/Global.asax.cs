using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using System.Timers;
using System.Data;
using System.Threading.Tasks;

namespace GksKatowiceBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);


            Helpers.BaseDB.AddToLog("Wywołanie metody Application_Start");
            var aTimer = new System.Timers.Timer();
            aTimer.Interval = 3 * 60 * 1000;

            aTimer.Elapsed += OnTimedEvent;
            aTimer.Start();
        }
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (DateTime.UtcNow.Hour == 15 && (DateTime.UtcNow.Minute > 0 && DateTime.UtcNow.Minute <= 3))
            {
                DataTable dtWiadomosci = Helpers.BaseDB.GetWiadomosci();
                DataTable dt = Helpers.BaseGETMethod.GetUser();
                foreach (DataRow dr in dt.Rows)
                {
                    Task.Run(() => Controllers.ThreadClass.SendThreadMessage(dr,dtWiadomosci));
                }

                List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                Helpers.BaseGETMethod.GetCardsAttachmentsAktualnosci(ref hrefList, false, dtWiadomosci);
                Helpers.BaseDB.AddWiadomosc(hrefList);
            }
        }
    }
}
