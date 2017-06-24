using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json.Linq;
using Parameters;
using GksKatowiceBot.Helpers;
using System.Json;

namespace GksKatowiceBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            try
            {

                if (activity.Type == ActivityTypes.Message)
                {

                    if (BaseDB.czyAdministrator(activity.From.Id)!=null && ( ((activity.Text!=null && activity.Text.IndexOf("!!!") != -1 )|| (activity.Attachments!=null && activity.Attachments.Count>0))))
                    {

                        if (activity.Text == null) activity.Text = "";

                        WebClient client = new WebClient();

                        if (activity.Text!=null && activity.Text.ToUpper().IndexOf("!!!ANKIETA")>-1)
                        {
                            try
                            {
                           //     int index = activity.Text.ToUpper().IndexOf("!!!ANKIETA");
                            DataTable dt = BaseDB.DajAnkiete(Convert.ToInt32(activity.Text.ToUpper().Substring(10)));
                            if(dt.Rows.Count>0)
                                {
                                    CreateAnkieta(dt, activity.From.Id.ToString());
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        
                        else if (activity.Attachments!=null && activity.Attachments.Count> 0 && activity.Text!=null && !activity.Text.Contains("skra.pl/"))
                        {
                            //Uri uri = new Uri(activity.Attachments[0].ContentUrl);
                            string filename = activity.Attachments[0].ContentUrl.Substring(activity.Attachments[0].ContentUrl.Length - 4, 3).Replace(".","");


                          //  WebClient client = new WebClient();
                            client.Credentials = new NetworkCredential("serwer1606926", "Tomason1910");
                            client.BaseAddress = "ftp://serwer1606926.home.pl/public_html/pub/";


                            byte[] data;
                            using (WebClient client2 = new WebClient())
                            {
                                data = client2.DownloadData(activity.Attachments[0].ContentUrl);
                            }
                            if(activity.Attachments[0].ContentType.Contains("image")) client.UploadData(filename+".png", data); //since the baseaddress
                            else if(activity.Attachments[0].ContentType.Contains("video")) client.UploadData(filename + ".mp4", data);
                        }


                        else if (activity.Text.Contains("skra.pl/"))
                        {
                            activity.Attachments = BaseGETMethod.GetCardsAttachmentsExtra(false, activity.Text);
                            activity.Text = "Ważne!";
                        }

                        if (activity.Text.ToUpper().IndexOf("!!!ANKIETA") == -1)
                        {
                            CreateMessage(activity.Attachments, activity.Text == null ? "" : activity.Text.Replace("!!!", ""), activity.From.Id);
                        }
                        
                    }
                    else
                    {

                        

                        string komenda = "";
                        if (activity.ChannelData != null)
                        {
                            try
                            {
                                BaseDB.AddToLog("Przesylany Json " + activity.ChannelData.ToString());
                                dynamic stuff = JsonConvert.DeserializeObject(activity.ChannelData.ToString());
                                komenda = stuff.message.quick_reply.payload;
                                BaseDB.AddToLog("Komenda: " + komenda);
                            }
                            catch (Exception ex)
                            {
                                BaseDB.AddToLog("Bład rozkładania Jsona " + ex.ToString());
                            }
                        }

                        

                        MicrosoftAppCredentials.TrustServiceUrl(@"https://facebook.botframework.com", DateTime.MaxValue);

                        if (BaseDB.czyPrzeklenstwo(activity.Text) == 1)
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",


                                buttons = new dynamic[]
                            {
                            new
                        {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                            },

                                quick_replies = new dynamic[]
                                   {
                                //new
                                //{oh
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Galeria",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Galeria",
                                //   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //},
                                                                new
                                {
                                    content_type = "text",
                                    title = "Sklep",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Sklep",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Video",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Video",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                   }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsFoto();
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }

                        else if (komenda.Contains("DEVELOPER_DEFINED_PAYLOAD_Odpowiedz") || activity.Text.Contains("DEVELOPER_DEFINED_PAYLOAD_Odpowiedz"))
                        {

                            if (komenda.Substring(35, 1) == "1")
                            {

                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();
                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",


                                    buttons = new dynamic[]
                                {
                            new
                        {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                                },

                                    quick_replies = new dynamic[]
                                       {
                                //new
                                //{oh
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Galeria",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Galeria",
                                //   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //},
                                                                new
                                {
                                    content_type = "text",
                                    title = "Sklep",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Sklep",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Video",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Video",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                       }
                                });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.Text = "Super dziękuję za oddanie głosu :)";
                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (komenda.Substring(35, 1) == "2")
                            {

                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();
                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",


                                    buttons = new dynamic[]
                                {
                            new
                        {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                                },

                                    quick_replies = new dynamic[]
                                       {
                                //new
                                //{oh
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Galeria",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Galeria",
                                //   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //},
                                                                new
                                {
                                    content_type = "text",
                                    title = "Sklep",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Sklep",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Video",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Video",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                       }
                                });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.Text = "Super dziękuję za oddanie głosu :)";
                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (komenda.Substring(35, 1) == "3")
                            {

                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();
                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",


                                    buttons = new dynamic[]
                                {
                            new
                        {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                                },

                                    quick_replies = new dynamic[]
                                       {
                                //new
                                //{oh
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Galeria",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Galeria",
                                //   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //},
                                                                new
                                {
                                    content_type = "text",
                                    title = "Sklep",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Sklep",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Video",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Video",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                       }
                                });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.Text = "Super dziękuję za oddanie głosu :)";
                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (komenda.Substring(35, 1) == "4")
                            {

                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();
                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",


                                    buttons = new dynamic[]
                                {
                            new
                        {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                                },

                                    quick_replies = new dynamic[]
                                       {
                                //new
                                //{oh
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Galeria",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Galeria",
                                //   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //},
                                                                new
                                {
                                    content_type = "text",
                                    title = "Sklep",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Sklep",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Video",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Video",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                       }
                                });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.Text = "Super dziękuję za oddanie głosu :)";
                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                                       {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Galeria",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Galeria",
                                //   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Sklep",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Sklep",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Video",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Video",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                       }
                            });
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsAktualnosci(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }


                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Sklep" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_Sklep")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                                       {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Szaliki",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Szaliki",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Galeria",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Galeria",
                                //   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //},
                                                                new
                                {
                                    content_type = "text",
                                    title = "Kubki",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Kubki",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Koszulki repliki",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_KoszulkiRepliki",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                       }
                            });
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            message.Text = "Polecane produkty";
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsSklep(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }


                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Szaliki" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_Szaliki")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                                       {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Galeria",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Galeria",
                                //   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //},
                                                                new
                                {
                                    content_type = "text",
                                    title = "Sklep",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Sklep",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Video",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Video",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                       }
                            });
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            message.Text = "Szaliki";
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsSzaliki(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }

                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Kubki" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_Kubki")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                                       {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Galeria",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Galeria",
                                //   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //},
                                                                new
                                {
                                    content_type = "text",
                                    title = "Sklep",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Sklep",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Video",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Video",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                       }
                            });
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            message.Text = "Kubki";
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsKubki(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }

                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_KoszulkiRepliki" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_KoszulkiRepliki")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                                       {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Galeria",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Galeria",
                                //   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //},
                                                                new
                                {
                                    content_type = "text",
                                    title = "Sklep",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Sklep",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Video",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Video",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                       }
                            });
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            message.Text = "Koszulki repliki";
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsKoszulkiRepliki(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }

                        else
                             if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Galeria" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_Galeria")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",


                                buttons = new dynamic[]
                            {
                            new
                        {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                            },

                                quick_replies = new dynamic[]
                                   {
                                //new
                                //{oh
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Galeria",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Galeria",
                                //   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //},
                                                                new
                                {
                                    content_type = "text",
                                    title = "Sklep",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Sklep",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Video",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Video",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                   }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            message.Attachments = BaseGETMethod.GetCardsAttachmentsGaleria(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else
                                 if (komenda == "DEVELOPER_DEFINED_PAYLOAD_Video" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_Video")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",


                                buttons = new dynamic[]
                            {
                            new
                            {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                            },

                                quick_replies = new dynamic[]
                               {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Galeria",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Galeria",
                                //   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //},
                                                                new
                                {
                                    content_type = "text",
                                    title = "Sklep",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Sklep",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Video",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Video",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                                                                               }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            message.Attachments = BaseGETMethod.GetCardsAttachmentsVideo(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else
                                     if (komenda == "USER_DEFINED_PAYLOAD" || activity.Text == "USER_DEFINED_PAYLOAD")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Galeria",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Galeria",
                                //   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //},
                                                                new
                                {
                                    content_type = "text",
                                    title = "Sklep",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Sklep",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                       new
                                {
                                    content_type = "text",
                                    title = "Video",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Video",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsPowitanie();
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                            message.Text = @"Witaj " + userAccount.Name.Substring(0, userAccount.Name.IndexOf(" ")) + @", jestem asystentem do kontaktu ze stronami internetowymi klubu Skra Bełchatów. Raz dziennie powiadomię Cię o aktualnościach z życia klubu. Ponadto spodziewaj się powiadomień w formie komunikatów, bądź innych informacji przekazywanych przez administratora dotyczących szczególnie ważnych dla kibiców wydarzeń.   
";
                            message.Attachments = null;
                            // message.Attachments = GetCardsAttachments(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);

                            //                            message.Text = @"Współpraca między nami jest bardzo prosta.Wydajesz mi polecenia, a ja za Ciebie wykonuje robotę.
                            //Zaznacz tylko w rozwijanym menu lub skorzystaj z podpowiedzi, która sekcja cię interesuje, a ja automatycznie połączę Cię z aktualnościami z wybranej sekcji.
                            //";

                            //                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }

                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_POWIADOMIENIA" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_POWIADOMIENIA" || activity.Text == "Powiadomienia")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();
                            byte czyPowiadomienia = BaseDB.czyPowiadomienia(userAccount.Id);
                            if (czyPowiadomienia == 0)
                            {
                                message.Text = "Opcja automatycznych, codziennych powiadomień o aktualnościach  jest włączona. Jeśli nie chcesz otrzymywać powiadomień  możesz je wyłączyć.";
                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",
                                    //buttons = new dynamic[]
                                    // {
                                    //     new
                                    //     {
                                    //    type ="postback",
                                    //    title="Tytul",
                                    //    vslue = "tytul",
                                    //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                    //     }
                                    // },
                                    quick_replies = new dynamic[]
                                           {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualności",
                                ////       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Wyłącz powiadomienia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaWylacz",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Włącz",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaWlacz",
                                //   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //},

                                           }
                                });
                            }
                            else if (czyPowiadomienia == 1)
                            {
                                message.Text = "Opcja automatycznych, codziennych  powiadomień o aktualnościach jest wyłączona. Jeśli chcesz otrzymywać powiadomienia możesz je włączyć.";
                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",
                                    //buttons = new dynamic[]
                                    // {
                                    //     new
                                    //     {
                                    //    type ="postback",
                                    //    title="Tytul",
                                    //    vslue = "tytul",
                                    //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                    //     }
                                    // },
                                    quick_replies = new dynamic[]
           {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualności",
                                ////       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                //},
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Wyłącz",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaWylacz",
                                //    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                // //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Włącz powiadomienia",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaWlacz",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },

           }
                                });
                            }
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            //    message.Attachments = BaseGETMethod.GetCardsAttachmentsNajnowsze(ref hrefList, true);
                            //     message.Text = "W kazdej chwili możesz włączyć lub wyłączyć otrzymywanie powiadomień na swojego Messengera. Co chcesz zrobić z powiadomieniami? ";
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaWylacz" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaWylacz" || activity.Text == "Wyłącz")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Galeria",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Galeria",
                                //   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //},
                                                                new
                                {
                                    content_type = "text",
                                    title = "Sklep",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Sklep",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                    new
                                {
                                    content_type = "text",
                                    title = "Video",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Video",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            //  message.Attachments = BaseGETMethod.GetCardsAttachmentsNajnowsze(ref hrefList, true);
                            message.Text = "Zrozumiałem, wyłączyłem automatyczne, codzienne powiadomienia o aktualnościach.";
                            BaseDB.ChangeNotification(userAccount.Id, 1);
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaWlacz" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_PowiadomieniaWlacz" || activity.Text == "Wyłącz")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            // BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Galeria",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Galeria",
                                //   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //},
                                                                new
                                {
                                    content_type = "text",
                                    title = "Sklep",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Sklep",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                             new
                                {
                                    content_type = "text",
                                    title = "Video",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Video",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            //  message.Attachments = BaseGETMethod.GetCardsAttachmentsNajnowsze(ref hrefList, true);
                            message.Text = "Zrozumiałem, włączyłem automatyczne, codzienne powiadomienia o aktualnościach.";
                            BaseDB.ChangeNotification(userAccount.Id, 0);
                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }

                        else if (activity.Text.ToUpper().Contains("KIBICE") || activity.Text.ToUpper().Contains("KIBIC") || activity.Text.ToUpper().Contains("FANI"))
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();


                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Galeria",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Galeria",
                                //   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //},
                                                                new
                                {
                                    content_type = "text",
                                    title = "Sklep",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Sklep",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                              new
                                {
                                    content_type = "text",
                                    title = "Video",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Video",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });
                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            // message.Text = "Wybierz jedną z opcji";
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsFotoKibice();

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }

                        else
                                    if (komenda == "DEVELOPER_DEFINED_PAYLOAD_HELP" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_HELP")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Galeria",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Galeria",
                                //   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //},
                                                                new
                                {
                                    content_type = "text",
                                    title = "Sklep",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Sklep",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                             new
                                {
                                    content_type = "text",
                                    title = "Video",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Video",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            message.Attachments = BaseGETMethod.GetCardsAttachmentsPowitanie();
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                            message.Text = @"Witaj " + userAccount.Name.Substring(0, userAccount.Name.IndexOf(" ")) + @", jestem asystentem do kontaktu ze stronami internetowymi klubu Skra Bełchatów. Raz dziennie powiadomię Cię o aktualnościach z życia klubu. Ponadto spodziewaj się powiadomień w formie komunikatów, bądź innych informacji przekazywanych przez administratora dotyczących szczególnie ważnych dla kibiców wydarzeń.   
";
                            message.Attachments = null;
                            // message.Attachments = GetCardsAttachments(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);

                            //                            message.Text = @"Współpraca między nami jest bardzo prosta.Wydajesz mi polecenia, a ja za Ciebie wykonuje robotę.
                            //Zaznacz tylko w rozwijanym menu lub skorzystaj z podpowiedzi, która sekcja cię interesuje, a ja automatycznie połączę Cię z aktualnościami z wybranej sekcji.
                            //";

                            //                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }

                        else
                        {

                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            IList<Attachment> lista = BaseGETMethod.GetCardsAttachmentsInne(ref hrefList, true, activity.Text.ToString());

                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                   // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Galeria",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Galeria",
                                //   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //},
                                                                new
                                {
                                    content_type = "text",
                                    title = "Sklep",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Sklep",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                    new
                                {
                                    content_type = "text",
                                    title = "Video",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Video",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;

                            if (lista.Count > 0)
                            {
                                message.Text = "";
                                message.Attachments = lista;
                            }
                            else
                            {
                                message.Text = "Nie znalazłem informacji pasujących do wpisanego hasła. Wybierz jedną z opcji.";
                            }
                            // message.Attachments = BaseGETMethod.GetCardsAttachmentsGallery(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                    }
                }

                else
                {
                    HandleSystemMessage(activity);
                }
            }
            catch (Exception ex)
            {
                BaseDB.AddToLog("Wysylanie wiadomosci: " + ex.ToString());
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        public async static void CreateMessage(IList<Attachment> foto,string wiadomosc,string fromId)
        {
            try
            {
                BaseDB.AddToLog("Wywołanie metody CreateMessage");

                string uzytkownik = "";
                DataTable dt = BaseGETMethod.GetUser();

                try
                {
                    MicrosoftAppCredentials.TrustServiceUrl(@"https://facebook.botframework.com", DateTime.MaxValue);

                    IMessageActivity message = Activity.CreateMessageActivity();
                    message.ChannelData = JObject.FromObject(new
                    {
                        notification_type = "REGULAR",
                        //buttons = new dynamic[]
                        // {
                        //     new
                        //     {
                        //    type ="postback",
                        //    title="Tytul",
                        //    vslue = "tytul",
                        //    payload="DEVELOPER_DEFINED_PAYLOAD"
                        //     }
                        // },
                        quick_replies = new dynamic[]
  {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                   // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Galeria",
                                //    payload = "DEVELOPER_DEFINED_PAYLOAD_Galeria",
                                //   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                //},
                                                                new
                                {
                                    content_type = "text",
                                    title = "Sklep",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Sklep",
                                   //   image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                              new
                                {
                                    content_type = "text",
                                    title = "Video",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Video",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                 }
                    });

                    message.AttachmentLayout = null;

                    if (foto!=null && foto.Count > 0)
                    {
                        try
                        {
                            string filename = foto[0].ContentUrl.Substring(foto[0].ContentUrl.Length - 4, 3).Replace(".", "");

                            if (foto[0].ContentType.Contains("image")) foto[0].ContentUrl = "http://serwer1606926.home.pl/pub/" + filename + ".png";//since the baseaddress
                            else if (foto[0].ContentType.Contains("video")) foto[0].ContentUrl = "http://serwer1606926.home.pl/pub/" + filename + ".mp4";
                        }
                        catch
                        {

                        }
                        message.Attachments = foto;
                    }
                    
                    message.Text = wiadomosc;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        try
                        {
                            if (fromId != dt.Rows[i]["UserId"].ToString())
                            {

                                var userAccount = new ChannelAccount(name: dt.Rows[i]["UserName"].ToString(), id: dt.Rows[i]["UserId"].ToString());
                                uzytkownik = userAccount.Name;
                                var botAccount = new ChannelAccount(name: dt.Rows[i]["BotName"].ToString(), id: dt.Rows[i]["BotId"].ToString());
                                var connector = new ConnectorClient(new Uri(dt.Rows[i]["Url"].ToString()), "8fa14720-e758-4cc7-82fd-fd6ad145ec90", "oExpuWnvj4oDAQnYHSpVrCJ");
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id, isGroup: false);
                                //await connector.Conversations.SendToConversationAsync((Activity)message).ConfigureAwait(false);

                                var returne = await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                        }
                        catch (Exception ex)
                        {
                            BaseDB.AddToLog("Błąd wysyłania wiadomości do: " + uzytkownik + " " + ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    BaseDB.AddToLog("Błąd wysyłania wiadomości do: " + uzytkownik + " " + ex.ToString());
                }
            }
            catch (Exception ex)
            {
                BaseDB.AddToLog("Błąd wysłania wiadomosci: " + ex.ToString());
            }
        }



        public async static void CreateAnkieta(DataTable dtAnkieta,string fromId)
        {
            try
            {
                BaseDB.AddToLog("Wywołanie metody CreateAnkieta");

                string uzytkownik = "";
                DataTable dt = BaseGETMethod.GetUser();

                try
                {
                    MicrosoftAppCredentials.TrustServiceUrl(@"https://facebook.botframework.com", DateTime.MaxValue);

                    IMessageActivity message = Activity.CreateMessageActivity();
                    message.ChannelData = JObject.FromObject(new
                    {
                        notification_type = "REGULAR",
                        //buttons = new dynamic[]
                        // {
                        //     new
                        //     {
                        //    type ="postback",
                        //    title="Tytul",
                        //    vslue = "tytul",
                        //    payload="DEVELOPER_DEFINED_PAYLOAD"
                        //     }
                        // },
                        quick_replies = new dynamic[]
  {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = dtAnkieta.Rows[0]["Odpowiedz1"].ToString(),
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Odpowiedz1_"+dtAnkieta.Rows[0]["ID"],
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                   // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = dtAnkieta.Rows[0]["Odpowiedz2"].ToString(),
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Odpowiedz2_"+dtAnkieta.Rows[0]["ID"],
                           //         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = dtAnkieta.Rows[0]["Odpowiedz3"].ToString(),
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Odpowiedz3_"+dtAnkieta.Rows[0]["ID"],
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                               new
                                {
                                    content_type = "text",
                                    title = dtAnkieta.Rows[0]["Odpowiedz4"].ToString(),
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Odpowiedz4_"+dtAnkieta.Rows[0]["ID"],
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },

                                 }
                    });

                    message.AttachmentLayout = null;

                 


                    //var list = new List<Attachment>();
                    //if (foto != null)
                    //{
                    //    for (int i = 0; i < foto.Count; i++)
                    //    {
                    //        list.Add(GetHeroCard(
                    //       foto[i].ContentUrl, "", "",
                    //       new CardImage(url: foto[i].ContentUrl),
                    //       new CardAction(ActionTypes.OpenUrl, "", value: ""),
                    //       new CardAction(ActionTypes.OpenUrl, "", value: "https://www.facebook.com/sharer/sharer.php?u=" + "")));
                    //    }
                    //}

                    message.Text = dtAnkieta.Rows[0]["Tresc"].ToString();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        try
                        {
                            if (fromId != dt.Rows[i]["UserId"].ToString())
                            {

                                var userAccount = new ChannelAccount(name: dt.Rows[i]["UserName"].ToString(), id: dt.Rows[i]["UserId"].ToString());
                                uzytkownik = userAccount.Name;
                                var botAccount = new ChannelAccount(name: dt.Rows[i]["BotName"].ToString(), id: dt.Rows[i]["BotId"].ToString());
                                var connector = new ConnectorClient(new Uri(dt.Rows[i]["Url"].ToString()), "8fa14720-e758-4cc7-82fd-fd6ad145ec90", "oExpuWnvj4oDAQnYHSpVrCJ");
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id, isGroup: false);
                                //await connector.Conversations.SendToConversationAsync((Activity)message).ConfigureAwait(false);

                                var returne = await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                        }
                        catch (Exception ex)
                        {
                            BaseDB.AddToLog("Błąd wysyłania wiadomości do: " + uzytkownik + " " + ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    BaseDB.AddToLog("Błąd wysyłania wiadomości do: " + uzytkownik + " " + ex.ToString());
                }
            }
            catch (Exception ex)
            {
                BaseDB.AddToLog("Błąd wysłania wiadomosci: " + ex.ToString());
            }
        }


        public static void CallToChildThread()
        {
            try
            {
                Thread.Sleep(5000);
            }

            catch (ThreadAbortException e)
            {
                Console.WriteLine("Thread Abort Exception");
            }
            finally
            {
                Console.WriteLine("Couldn't catch the Thread Exception");
            }
        }






        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                BaseDB.DeleteUser(message.From.Id);
            }
            else
                if (message.Type == ActivityTypes.ConversationUpdate)
            {
            }
            else
                    if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
            }
            else
                        if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else
                            if (message.Type == ActivityTypes.Ping)
            {
            }
            else
                                if (message.Type == ActivityTypes.Typing)
            {
            }
            return null;
        }


     



     
    }
}
