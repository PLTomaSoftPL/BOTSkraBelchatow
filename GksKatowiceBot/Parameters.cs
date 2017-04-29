using System;
using System.Collections.Generic;
using System.Linq;

namespace Parameters
{
    public class Parameters
    {
        public struct userDataStruct
        {
            public string userName;
            public string userId;
            public string ServiceUrl;
            public string botName;
            public string botId;
        }

        public static string firstArticcle;
        public static List<userDataStruct> listaAdresow = new List<userDataStruct>();
    }
}
