using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace GksKatowiceBot.Helpers
{
    public class BaseDB
    {
        public static void AddToLog(string action)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "INSERT INTO LogSkraBelchatow (Tresc) VALUES ('" + action + " " + DateTime.Now.ToString() + "')";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                //SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                //SqlCommand cmd = new SqlCommand();
                //SqlDataReader reader;

                //cmd.CommandText = "INSERT INTO LogSkraBelchatow (Tresc) VALUES ('" + "Błąd dodawania wiadomosci do Loga" + " " + DateTime.Now.ToString() + "')";
                //cmd.CommandType = CommandType.Text;
                //cmd.Connection = sqlConnection1;

                //sqlConnection1.Open();
                //cmd.ExecuteNonQuery();

                //sqlConnection1.Close();
            }
        }
        public static void AddUser(string UserName, string UserId, string BotName, string BotId, string Url, byte flgTyp)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "IF NOT EXISTS(Select * from [dbo].[UserSkraBelchatow] where UserId='" + UserId + "')BEGIN INSERT INTO [dbo].[UserSkraBelchatow] (UserName,UserId,BotName,BotId,Url,flgPlusLiga,DataUtw,flgDeleted) VALUES ('" + UserName + "','" + UserId + "','" + BotName + "','" + BotId + "','" + Url + "','" + flgTyp.ToString() + "','" + DateTime.Now + "','0')END";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                AddToLog("Blad dodawania uzytkownika "+ex.ToString());
            }
        }
        public static object czyAdministrator(string UserId)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "sprawdzCzyAdministratorSkraBelchatow";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@userId", UserId);
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                var rowsAffected = cmd.ExecuteScalar();

                sqlConnection1.Close();

                return rowsAffected;
            }
            catch (Exception ex)
            {
                AddToLog("Blad sprawdzania uzytkownika czy admnistrator "+ex.ToString());
                return null;
            }
        }
        public static void DeleteUser(string UserId)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "Delete [dbo].[UserSkraBelchatow] where UserId='" + UserId + "'";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch
            {
                AddToLog("Blad usuwania uzytkownika: " + UserId);
            }
        }
        public static void AddWiadomoscPilka(List<System.Linq.IGrouping<string, string>> hrefList)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "INSERT INTO [dbo].[WiadomosciSkraBelchatow] (Nazwa,DataUtw,Wiadomosc1,Wiadomosc2,Wiadomosc3,Wiadomosc4,Wiadomosc5) VALUES ('" + "" + "','" + DateTime.Now + "','" + hrefList[0].Key + "','" + hrefList[1].Key + "','" + hrefList[2].Key + "','" + hrefList[3].Key + "','" + hrefList[4].Key + "')";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości: " + ex.ToString());
            }
        }
        public static void AddWiadomoscSiatka(List<System.Linq.IGrouping<string, string>> hrefList)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "INSERT INTO [dbo].[WiadomosciSkraBelchatow] (Nazwa,DataUtw,Wiadomosc1,Wiadomosc2,Wiadomosc3,Wiadomosc4,Wiadomosc5) VALUES ('" + "" + "','" + DateTime.Now + "','" + hrefList[0].Key + "','" + hrefList[1].Key + "','" + hrefList[2].Key + "','" + hrefList[3].Key + "','" + hrefList[4].Key + "')";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości Orlen: " + ex.ToString());
            }
        }
        public static void AddWiadomoscHokej(List<System.Linq.IGrouping<string, string>> hrefList)
        {
            try
            {
                SqlConnection sqlConnection1 = new SqlConnection("Server=tcp:plps.database.windows.net,1433;Initial Catalog=PLPS;Persist Security Info=False;User ID=tomasoft;Password=Tomason18,;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                cmd.CommandText = "INSERT INTO [dbo].[WiadomosciSkraBelchatow] (Nazwa,DataUtw,Wiadomosc1,Wiadomosc2,Wiadomosc3) VALUES ('" + "" + "','" + DateTime.Now + "','" + hrefList[0].Key + "','" + hrefList[1].Key + "','" + hrefList[2].Key + "')";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection1;

                sqlConnection1.Open();
                cmd.ExecuteNonQuery();

                sqlConnection1.Close();
            }
            catch (Exception ex)
            {
                AddToLog("Błąd dodawania wiadomości Orlen: " + ex.ToString());
            }
        }
    }
}