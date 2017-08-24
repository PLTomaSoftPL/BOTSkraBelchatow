using GksKatowiceBot.Helpers;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace GksKatowiceBot.Controllers
{
    public class ThreadClass
    {
        public async static void SendThreadMessage(DataRow dr,DataTable dtWiadomosci,IList<Attachment> items)
        {
            try
            {
                 //   BaseDB.AddToLog("Wywołanie metody SendThreadMessage");



                    string uzytkownik = "";

                    if (items.Count > 0)
                    {
                        try
                        {
                            MicrosoftAppCredentials.TrustServiceUrl(@"https://facebook.botframework.com", DateTime.MaxValue);

                            IMessageActivity message = Activity.CreateMessageActivity();
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                quick_replies = new dynamic[]
                                    {
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Aktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                               // new
                               // {
                               //     content_type = "text",
                               //     title = "Galeria",
                               //     payload = "DEVELOPER_DEFINED_PAYLOAD_Galeria",
                               ////       image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                               // },
                               new
                                {
                                    content_type = "text",
                                    title = "Sklep",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Sklep",
                               //       image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
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

                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            message.Attachments = items;
                            try
                            {
                                var userAccount = new ChannelAccount(name: dr["UserName"].ToString(), id: dr["UserId"].ToString());
                                uzytkownik = userAccount.Name;
                                var botAccount = new ChannelAccount(name: dr["BotName"].ToString(), id: dr["BotId"].ToString());
                                var connector = new ConnectorClient(new Uri(dr["Url"].ToString()), "8fa14720-e758-4cc7-82fd-fd6ad145ec90", "oExpuWnvj4oDAQnYHSpVrCJ");
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id, isGroup: false);
                                await connector.Conversations.SendToConversationAsync((Activity)message).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                BaseDB.AddToLog("Błąd wysyłania wiadomości do: " + uzytkownik + " " + ex.ToString());
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
                BaseDB.AddToLog("Błąd wysłania wiadomosci: " + ex.ToString());
            }
        }
    }
}