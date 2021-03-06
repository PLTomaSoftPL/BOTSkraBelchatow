﻿using HtmlAgilityPack;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace GksKatowiceBot.Helpers
{
    public class BaseGETMethod
    {


        public static IList<Attachment> GetCardsAttachmentsAktualnosci(ref List<IGrouping<string, string>> hrefList, bool newUser = false, DataTable dtWiadomosci = null)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.skra.pl/pl/aktualnosci-przeglad";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "itemListPrimary";
                string xpath = String.Format("//div[@id='{0}']/div", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                hrefList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found")).Where(p => p.Contains("/pl/aktualnosci-przeglad/")).GroupBy(p => p.ToString())
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found")).Where(p => p.Contains("/media/k2"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("alt", "not found"))
                                  .ToList();

                response.Close();
                readStream.Close();

                int index = hrefList.Count;

                DataTable dt = dtWiadomosci;
                if (dt == null) dt = BaseDB.GetWiadomosci();

                if (newUser == true)
                {
                    index = hrefList.Count;
                    if (dt.Rows.Count == 0)
                    {
                        //    AddWiadomosc(hrefList);
                    }
                }

                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        List<int> deleteList = new List<int>();
                        var listTemp = new List<System.Linq.IGrouping<string, string>>();
                        var imageListTemp = new List<string>();
                        var titleListTemp = new List<string>();

                        for (int i = 0; i < hrefList.Count; i++)
                        {
                            if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc4"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc5"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc6"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc7"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc8"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc9"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc10"].ToString() != hrefList[i].Key)
                            {
                                listTemp.Add(hrefList[i]);
                                imageListTemp.Add(imgList[i].Replace("S.jpg", "XL.jpg"));
                                titleListTemp.Add(titleList[i].ToString().Replace("&quot;", ""));
                            }
                            listTemp2.Add(hrefList[i]);
                        }
                        hrefList = listTemp;
                        index = hrefList.Count;
                        imgList = imageListTemp;
                        //   titleList = titleListTemp;
                        //   AddWiadomosc(listTemp2);
                    }
                    else
                    {
                        index = hrefList.Count;
                        //   AddWiadomosc(hrefList);
                    }
                }

                for (int i = 0; i < hrefList.Count; i++)
                {
                    string link = "";
                    if (hrefList[i].Key.Contains("http"))
                    {
                        link = hrefList[i].Key;
                    }
                    else
                    {
                        link = "http://www.skra.pl/" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", "-"), "", "",
                        new CardImage(url: "http://www.skra.pl/" + imgList[i].Replace("S.jpg", "Xl.jpg")),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", "-"), "", "",
                        new CardImage(url: "http://www.skra.pl/" + imgList[i].Replace("S.jpg", "Xl.jpg")),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", " ").Replace("&rdquo;", ""), "", "",
                        new CardImage(url: "http://www.skra.pl" + imgList[i].Replace("S.jpg", "XL.jpg")),
                        new CardAction(ActionTypes.OpenUrl, "Więcej", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }


        public static IList<Attachment> GetCardsAttachmentsSklep(ref List<IGrouping<string, string>> hrefList, bool newUser = false, DataTable dtWiadomosci = null)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://sklep.skra.pl";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "featured-products_block_center";
                string xpath = String.Format("//div[@id='{0}']/div", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                hrefList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found")).Where(p => p.Contains("sklep.skra.pl") && !p.Contains("cart")).GroupBy(p => p.ToString())
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found")).Where(p => p.Contains("sklep.skra.pl"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("title", "not found")).Where(p=>p !="More" && p!="Zobacz" && p!="Dodaj do koszyka")
                                  .ToList();
                List<string> titleList2 = new List<string>();
                for(int i=0;i<titleList.Count;i++)
                {
                    if(i%2==0)
                    {
                        titleList2.Add(titleList[i]);
                    }
                }
                titleList = titleList2;
                var priceList = doc2.DocumentNode.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("price")).ToList();

                response.Close();
                readStream.Close();

                int index = hrefList.Count;

             

                for (int i = 0; i < hrefList.Count; i++)
                {
                    string link = "";
                    if (hrefList[i].Key.Contains("http"))
                    {
                        link = hrefList[i].Key;
                    }
                    else
                    {
                        link = "http://www.skra.pl/" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", "-"), "", "",
                        new CardImage(url: "http://www.skra.pl/" + imgList[i].Replace("S.jpg", "Xl.jpg")),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", "-"), "", "",
                        new CardImage(url: "http://www.skra.pl/" + imgList[i].Replace("S.jpg", "Xl.jpg")),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", " ").Replace("&rdquo;", "").Replace("amp;",""),"Cena: "+priceList[i].InnerText, "",
                        new CardImage(url: imgList[i].Replace("-home.jpg", "-large.jpg")),
                        new CardAction(ActionTypes.OpenUrl, "Zobacz", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }


        public static IList<Attachment> GetCardsAttachmentsKoszulkiRepliki(ref List<IGrouping<string, string>> hrefList, bool newUser = false, DataTable dtWiadomosci = null)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://sklep.skra.pl/59-koszulki-repliki";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "product_list";
                string xpath = String.Format("//ul[@id='{0}']", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person;
                }



                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                hrefList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found")).Where(p => p.Contains("sklep.skra.pl") && !p.Contains("cart")).GroupBy(p => p.ToString())
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found")).Where(p => p.Contains("sklep.skra.pl"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("title", "")).Where(p => p != "not found" && p != "Wyświetl" && p != "Dodaj do koszyka")
                                  .ToList();
                List<string> titleList2 = new List<string>();
                for (int i = 0; i < titleList.Count; i++)
                {
                    if (titleList[i] != "")
                    {
                        if (i % 2 == 0)
                        {
                            titleList2.Add(titleList[i]);
                        }
                    }
                }
                titleList = titleList2;
                var priceList = doc2.DocumentNode.Descendants("span").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("price")).ToList();

                response.Close();
                readStream.Close();

                int index = hrefList.Count;



                for (int i = 0; i < hrefList.Count; i++)
                {
                    string link = "";
                    if (hrefList[i].Key.Contains("http"))
                    {
                        link = hrefList[i].Key;
                    }
                    else
                    {
                        link = "http://www.skra.pl/" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", "-"), "", "",
                        new CardImage(url: "http://www.skra.pl/" + imgList[i].Replace("S.jpg", "Xl.jpg")),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", "-"), "", "",
                        new CardImage(url: "http://www.skra.pl/" + imgList[i].Replace("S.jpg", "Xl.jpg")),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", " ").Replace("&rdquo;", "").Replace("amp;", ""), "Cena: " + priceList[i].InnerText, "",
                        new CardImage(url: imgList[i].Replace("home.jpg", "large.jpg")),
                        new CardAction(ActionTypes.OpenUrl, "Zobacz", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }

        public static IList<Attachment> GetCardsAttachmentsKubki(ref List<IGrouping<string, string>> hrefList, bool newUser = false, DataTable dtWiadomosci = null)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://sklep.skra.pl/39-kubki";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "product_list";
                string xpath = String.Format("//ul[@id='{0}']", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person;
                }



                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                hrefList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found")).Where(p => p.Contains("sklep.skra.pl") && !p.Contains("cart")).GroupBy(p => p.ToString())
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found")).Where(p => p.Contains("sklep.skra.pl"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("title", "")).Where(p => p != "not found" && p != "Wyświetl" && p != "Dodaj do koszyka")
                                  .ToList();
                List<string> titleList2 = new List<string>();
                for (int i = 0; i < titleList.Count; i++)
                {
                    if (titleList[i] != "")
                    {
                        if (i % 2 == 0)
                        {
                            titleList2.Add(titleList[i]);
                        }
                    }
                }
                titleList = titleList2;
                var priceList = doc2.DocumentNode.Descendants("span").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("price")).ToList();

                response.Close();
                readStream.Close();

                int index = hrefList.Count;



                for (int i = 0; i < hrefList.Count; i++)
                {
                    string link = "";
                    if (hrefList[i].Key.Contains("http"))
                    {
                        link = hrefList[i].Key;
                    }
                    else
                    {
                        link = "http://www.skra.pl/" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", "-"), "", "",
                        new CardImage(url: "http://www.skra.pl/" + imgList[i].Replace("S.jpg", "Xl.jpg")),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", "-"), "", "",
                        new CardImage(url: "http://www.skra.pl/" + imgList[i].Replace("home.jpg", "large.jpg")),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", " ").Replace("&rdquo;", "").Replace("amp;", ""), "Cena: " + priceList[i].InnerText, "",
                        new CardImage(url: imgList[i].Replace("home.jpg", "large.jpg")),
                        new CardAction(ActionTypes.OpenUrl, "Zobacz", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }

        public static IList<Attachment> GetCardsAttachmentsSzaliki(ref List<IGrouping<string, string>> hrefList, bool newUser = false, DataTable dtWiadomosci = null)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://sklep.skra.pl/13-szaliki";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "product_list";
                string xpath = String.Format("//ul[@id='{0}']", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person;
                }



                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                hrefList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found")).Where(p => p.Contains("sklep.skra.pl") && !p.Contains("cart")).GroupBy(p => p.ToString())
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found")).Where(p => p.Contains("sklep.skra.pl"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("title","")).Where(p => p != "not found" && p != "Wyświetl" && p != "Dodaj do koszyka")
                                  .ToList();
                List<string> titleList2 = new List<string>();
                for (int i = 0; i < titleList.Count; i++)
                {
                    if (titleList[i] != "")
                    {
                        if (i % 2 == 0)
                        {
                            titleList2.Add(titleList[i]);
                        }
                    }
                }
                titleList = titleList2;
                var priceList = doc2.DocumentNode.Descendants("span").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("price")).ToList();

                response.Close();
                readStream.Close();

                int index = hrefList.Count;



                for (int i = 0; i < hrefList.Count; i++)
                {
                    string link = "";
                    if (hrefList[i].Key.Contains("http"))
                    {
                        link = hrefList[i].Key;
                    }
                    else
                    {
                        link = "http://www.skra.pl/" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", "-"), "", "",
                        new CardImage(url: "http://www.skra.pl/" + imgList[i].Replace("S.jpg", "Xl.jpg")),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", "-"), "", "",
                        new CardImage(url: "http://www.skra.pl/" + imgList[i].Replace("S.jpg", "Xl.jpg")),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", " ").Replace("&quoto;","").Replace("&rdquo;", "").Replace("amp;", "").Replace("&quot;",""), "Cena: " + priceList[i].InnerText, "",
                        new CardImage(url: imgList[i].Replace("home.jpg", "large.jpg")),
                        new CardAction(ActionTypes.OpenUrl, "Zobacz", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }

        public static IList<Attachment> GetCardsAttachmentsGaleria(ref List<IGrouping<string, string>> hrefList, bool newUser = false)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.skra.pl/pl/galeria/sezon-2016-2017/plusliga";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "teaser-item-bg";
                string xpath = String.Format("//div[@class='{0}']/div", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                hrefList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found")).Where(p => p.Contains("/pl/")).GroupBy(p => p.ToString())
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found")).Where(p => p.Contains("/media/zoo"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("alt", "not found"))
                                  .ToList();

                response.Close();
                readStream.Close();

                int index = hrefList.Count;

                DataTable dt = new DataTable();

                if (newUser == true)
                {
                    index = hrefList.Count;
                    if (dt.Rows.Count == 0)
                    {
                        //    AddWiadomosc(hrefList);
                    }
                }

                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        List<int> deleteList = new List<int>();
                        var listTemp = new List<System.Linq.IGrouping<string, string>>();
                        var imageListTemp = new List<string>();
                        var titleListTemp = new List<string>();

                        for (int i = 0; i < hrefList.Count; i++)
                        {
                            if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key)
                            {
                                listTemp.Add(hrefList[i]);
                                imageListTemp.Add("http://www.skra.pl/" + imgList[i].Replace("S.jpg", "Xl.jpg"));
                                titleListTemp.Add(titleList[i].ToString().Replace("&quot;", ""));
                            }
                            listTemp2.Add(hrefList[i]);
                        }
                        hrefList = listTemp;
                        index = hrefList.Count;
                        imgList = imageListTemp;
                        //   titleList = titleListTemp;
                        //   AddWiadomosc(listTemp2);
                    }
                    else
                    {
                        index = hrefList.Count;
                        //   AddWiadomosc(hrefList);
                    }
                }

                for (int i = 0; i < 6; i++)
                {
                    string link = "";
                    if (hrefList[i].Key.Contains("http"))
                    {
                        link = hrefList[i].Key;
                    }
                    else
                    {
                        link = "http://www.skra.pl/" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", "-"), "", "",
                        new CardImage(url: "http://www.skra.pl/" + imgList[i].Replace("S.jpg", "Xl.jpg")),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", "-"), "", "",
                        new CardImage(url: "http://www.skra.pl/" + imgList[i].Replace("S.jpg", "Xl.jpg")),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", "-"), "", "",
                        new CardImage(url: imgList[i]),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }


        public static IList<Attachment> GetCardsAttachmentsExtra(bool newUser = false, string urlAddress = "")
        {
            List<Attachment> list = new List<Attachment>();


            urlAddress = urlAddress.Replace("!!!", "");


            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                //string matchResultDivId = "itemImageBlock";
                //string xpath = String.Format("//figure[@class='{0}']/figure", matchResultDivId);
                //var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml);
                //string text = "";
                //foreach (var person in people)
                //{
                //    text += person;
                //}

                //HtmlDocument doc2 = new HtmlDocument();

                //doc2.LoadHtml(text);

                var imgList = doc.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found")).Where(p => p.Contains("/media/k2/"))
                                  .ToList();

                var titleList = doc.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("data-lightbox", "not found")).Where(p=>p!= "not found")
                                  .ToList();

                response.Close();
                readStream.Close();



                list.Add(GetHeroCard(
                titleList[0].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", "-"), "", "",
                new CardImage(url: "skra.pl"+imgList[0]),
                new CardAction(ActionTypes.OpenUrl, "Więcej", value: urlAddress),
                new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + urlAddress))
                );


            }


            return list;

        }

        public static IList<Attachment> GetCardsAttachmentsFoto()
        {
            List<Attachment> list = new List<Attachment>();
            // list.Add(GetHeroCard(
            // "", "", "",
            //new CardImage(url: "http://tomasoft.pl/pub/GKSKatowice/przeklenstwo.jpg"),
            //null,null)
            //            // new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
            //             );
            list.Add(new Attachment()
            {
                ContentUrl = "http://tomasoft.pl/pub/Skra/wulgaryzmy.jpg",
                ContentType = "image/jpg",
                Name = "przeklenstwo.jpg"
            });
            return list;
        }

        private static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction, CardAction cardAction2)
        {
            if (cardAction2 != null)
            {
                var heroCard = new HeroCard
                {
                    Title = title,
                    Subtitle = subtitle,
                    Text = text,
                    Images = new List<CardImage>() { cardImage },
                    Buttons = new List<CardAction>() { cardAction, cardAction2 },
                };

                return heroCard.ToAttachment();
            }
            else
            {
                var heroCard = new HeroCard
                {
                    Title = title,
                    Subtitle = subtitle,
                    Text = text,
                    Images = new List<CardImage>() { cardImage },
                    Buttons = new List<CardAction>() { cardAction },
                };

                return heroCard.ToAttachment();
            }
        }

        private static Attachment GetHeroCardExtra(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction, CardAction cardAction2, CardAction cardAction3)
        {
            if (cardAction2 != null)
            {
                var heroCard = new HeroCard
                {
                    Title = title,
                    Subtitle = subtitle,
                    Text = text,
                    Images = new List<CardImage>() { cardImage },
                    Buttons = new List<CardAction>() { cardAction, cardAction2,cardAction3 },
                };

                return heroCard.ToAttachment();
            }
            else
            {
                var heroCard = new HeroCard
                {
                    Title = title,
                    Subtitle = subtitle,
                    Text = text,
                    Images = new List<CardImage>() { cardImage },
                    Buttons = new List<CardAction>() { cardAction },
                };

                return heroCard.ToAttachment();
            }
        }
        public static DataTable GetWiadomosciPilka()
        {
            DataTable dt = new DataTable();

            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=SkraBelchatow;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();

                cmd.CommandText = "SELECT * FROM [dbo].[WiadomosciSkraBelchatow]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                sqlConnection1.Close();
                return dt;
            }
            catch
            {
                BaseDB.AddToLog("Błąd pobierania wiadomości");
                return null;
            }
        }


        public static DataTable GetWiadomosciHokej()
        {
            DataTable dt = new DataTable();

            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=SkraBelchatow;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();

                cmd.CommandText = "SELECT * FROM [dbo].[WiadomosciSkraBelchatow]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                sqlConnection1.Close();
                return dt;
            }
            catch
            {
                BaseDB.AddToLog("Błąd pobierania wiadomości");
                return null;
            }
        }
        public static DataTable GetUser()
        {
            DataTable dt = new DataTable();

            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=SkraBelchatow;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();

                cmd.CommandText = "SELECT * FROM [dbo].[UserSkraBelchatow] where flgDeleted=0";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                sqlConnection1.Close();
                return dt;
            }
            catch
            {
                BaseDB.AddToLog("Błąd pobierania użytkowników");
                return null;
            }
        }

        public static DataTable GetWiadomosciSiatka()
        {
            DataTable dt = new DataTable();

            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=SkraBelchatow;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();

                cmd.CommandText = "SELECT * FROM [dbo].[WiadomosciSkraBelchatow]";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                sqlConnection1.Close();
                return dt;
            }
            catch
            {
                BaseDB.AddToLog("Błąd pobierania wiadomości Orlen");
                return null;
            }
        }

        public static IList<Attachment> GetCardsAttachmentsVideo(ref List<IGrouping<string, string>> hrefList, bool newUser = false, byte rodzajStrony = 0)
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.skra.tv";
            // string urlAddress = "http://www.orlenliga.pl/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);

                string matchResultDivId = "match-height";
                string xpath = String.Format("//div[@class='{0}']/div", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                hrefList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found")).Where(p => p.Contains("/item/")).GroupBy(p => p.ToString())
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found")).Where(p => p.Contains("/media/zoo"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("alt", "not found"))
                                  .ToList();

                response.Close();
                readStream.Close();

                int index = hrefList.Count;

                DataTable dt = new DataTable();

                if (newUser == true)
                {
                    index = hrefList.Count;
                    if (dt.Rows.Count == 0)
                    {
                        //    AddWiadomosc(hrefList);
                    }
                }

                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        List<int> deleteList = new List<int>();
                        var listTemp = new List<System.Linq.IGrouping<string, string>>();
                        var imageListTemp = new List<string>();
                        var titleListTemp = new List<string>();

                        for (int i = 0; i < hrefList.Count; i++)
                        {
                            if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key)
                            {
                                listTemp.Add(hrefList[i]);
                                imageListTemp.Add("http://www.skra.pl/" + imgList[i].Replace("S.jpg", "L.jpg"));
                                titleListTemp.Add(titleList[i].ToString().Replace("&quot;", ""));
                            }
                            listTemp2.Add(hrefList[i]);
                        }
                        hrefList = listTemp;
                        index = hrefList.Count;
                        imgList = imageListTemp;
                        //   titleList = titleListTemp;
                        //   AddWiadomosc(listTemp2);
                    }
                    else
                    {
                        index = hrefList.Count;
                        //   AddWiadomosc(hrefList);
                    }
                }

                for (int i = 0; i < 6; i++)
                {
                    string link = "";
                    if (hrefList[i].Key.Contains("http"))
                    {
                        link = hrefList[i].Key;
                    }
                    else
                    {
                        link = "http://www.skra.tv/" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", "-"), "", "",
                        new CardImage(url: "http://www.skra.pl/" + imgList[i].Replace("S.jpg", "L.jpg")),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", "-"), "", "",
                        new CardImage(url: "http://www.skra.pl/" + imgList[i].Replace("S.jpg", "L.jpg")),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", "-"), "", "",
                        new CardImage(url: imgList[i].Replace("S.jpg", "L.jpg")),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj Video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;


        }

        public static IList<Attachment> GetCardsAttachmentsInne(ref List<IGrouping<string, string>> hrefList, bool newUser = false, string haslo = "")
        {
            List<Attachment> list = new List<Attachment>();

            string urlAddress = "http://www.skra.pl/pl/aktualnosci-przeglad/itemlist/search?searchword=" + haslo;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var listTemp2 = new List<System.Linq.IGrouping<string, string>>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                string data = readStream.ReadToEnd();

                if (data.Contains("No results found!")) return list;

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(data);



                string matchResultDivId = "searchK2Item";
                string xpath = String.Format("//div[@class='{0}']/div", matchResultDivId);
                var people = doc.DocumentNode.SelectNodes(xpath).Select(p => p.InnerHtml);
                string text = "";
                foreach (var person in people)
                {
                    text += person;
                }

                HtmlDocument doc2 = new HtmlDocument();

                doc2.LoadHtml(text);
                hrefList = doc2.DocumentNode.SelectNodes("//a")
                                  .Select(p => p.GetAttributeValue("href", "not found")).Where(p => p.Contains("/pl/aktualnosci-przeglad/")).GroupBy(p => p.ToString())
                                  .ToList();

                var imgList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("src", "not found")).Where(p => p.Contains("/media/k2"))
                                  .ToList();

                var titleList = doc2.DocumentNode.SelectNodes("//img")
                                  .Select(p => p.GetAttributeValue("alt", "not found"))
                                  .ToList();

                response.Close();
                readStream.Close();

                int index = hrefList.Count;

                DataTable dt = new DataTable();

                if (newUser == true)
                {
                    index = hrefList.Count;
                    if (dt.Rows.Count == 0)
                    {
                        //    AddWiadomosc(hrefList);
                    }
                }

                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        List<int> deleteList = new List<int>();
                        var listTemp = new List<System.Linq.IGrouping<string, string>>();
                        var imageListTemp = new List<string>();
                        var titleListTemp = new List<string>();

                        for (int i = 0; i < hrefList.Count; i++)
                        {
                            if (dt.Rows[dt.Rows.Count - 1]["Wiadomosc1"].ToString() != hrefList[i].Key && dt.Rows[dt.Rows.Count - 1]["Wiadomosc2"].ToString() != hrefList[i].Key &&
                                dt.Rows[dt.Rows.Count - 1]["Wiadomosc3"].ToString() != hrefList[i].Key)
                            {
                                listTemp.Add(hrefList[i]);
                                imageListTemp.Add("http://www.skra.pl/" + imgList[i].Replace("S.jpg", "Xl.jpg"));
                                titleListTemp.Add(titleList[i].ToString().Replace("&quot;", ""));
                            }
                            listTemp2.Add(hrefList[i]);
                        }
                        hrefList = listTemp;
                        index = hrefList.Count;
                        imgList = imageListTemp;
                        //   titleList = titleListTemp;
                        //   AddWiadomosc(listTemp2);
                    }
                    else
                    {
                        index = hrefList.Count;
                        //   AddWiadomosc(hrefList);
                    }
                }

                for (int i = 0; i < hrefList.Count; i++)
                {
                    string link = "";
                    if (hrefList[i].Key.Contains("http"))
                    {
                        link = hrefList[i].Key;
                    }
                    else
                    {
                        link = "http://www.skra.pl/" + hrefList[i].Key;
                        //link = "http://www.orlenliga.pl/" + hrefList[i].Key;
                    }

                    if (link.Contains("video"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", "-"), "", "",
                        new CardImage(url: "http://www.skra.pl/" + imgList[i].Replace("S.jpg", "Xl.jpg")),
                        new CardAction(ActionTypes.OpenUrl, "Oglądaj video", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                        if (link.Contains("gallery"))
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", "-"), "", "",
                        new CardImage(url: "http://www.skra.pl/" + imgList[i].Replace("S.jpg", "Xl.jpg")),
                        new CardAction(ActionTypes.OpenUrl, "Przeglądaj galerie", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }
                    else
                    {
                        list.Add(GetHeroCard(
                        titleList[i].ToString().Replace("&oacute;", "ó").Replace("&bdquo;", "-").Replace("&ndash;", "-"), "", "",
                        new CardImage(url: "http://www.skra.pl" + imgList[i].Replace("Generic.jpg", "XL.jpg")),
                        new CardAction(ActionTypes.OpenUrl, "Więcej", value: link),
                        new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
                        );
                    }

                    //  list.Add(new Microsoft.Bot.Connector.VideoCard(titleList[i], "", "",null)
                }
            }
            if (listTemp2.Count > 0)
            {
                hrefList = listTemp2;
            }

            return list;

        }

        public static IList<Attachment> GetCardsAttachmentsPowitanie()
        {
            List<Attachment> list = new List<Attachment>();
            // list.Add(GetHeroCard(
            // "", "", "",
            //new CardImage(url: "http://tomasoft.pl/pub/GKSKatowice/powitanie.jpg"),
            //null,null)
            //            // new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
            //             );
            list.Add(new Attachment()
            {
                ContentUrl = "http://tomasoft.pl/pub/Skra/powitanie.jpg",
                ContentType = "image/jpg",
                Name = "przeklenstwo.jpg"
            });
            return list;
        }

        public static IList<Attachment> GetCardsAttachmentsFotoKibice()
        {
            List<Attachment> list = new List<Attachment>();
            // list.Add(GetHeroCard(
            // "", "", "",
            //new CardImage(url: "http://tomasoft.pl/pub/GKSKatowice/przeklenstwo.jpg"),
            //null,null)
            //            // new CardAction(ActionTypes.OpenUrl, "Udostępnij", value: "https://www.facebook.com/sharer/sharer.php?u=" + link))
            //             );
            list.Add(new Attachment()
            {
                ContentUrl = "http://tomasoft.pl/pub/Skra/kibice.jpg",
                ContentType = "image/jpg",
                Name = "kibice.jpg"
            });
            return list;
        }

    }
}