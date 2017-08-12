using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SkraBelchatowBot.Helpers
{
    public class ClassHelpers
    {
        public class Sender
        {
            public string id { get; set; }
        }

        public class Recipient
        {
            public string id { get; set; }
        }

        public class Postback
        {
            public string payload { get; set; }
            public string title { get; set; }
        }

        public class RootObject
        {
            public Sender sender { get; set; }
            public Recipient recipient { get; set; }
            public long timestamp { get; set; }
            public Postback postback { get; set; }
        }
    }
}