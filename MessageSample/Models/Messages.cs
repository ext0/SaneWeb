using SaneWeb.Data;
using SaneWeb.Resources.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSample.Models
{
    [Table("Messages")]
    public class Message : Model<Message>
    {
        [DatabaseValue("data", 4096)]
        public String data { get; set; }

        [DatabaseValue("timestamp", 64)]
        public String timestamp { get; set; }

        [DatabaseValue("ip", 64)]
        public String ip { get; set; }

        public Message() : base()
        {

        }

        public Message(String data, String ip) : base()
        {
            this.data = data;
            this.ip = ip;
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            timestamp = t.TotalSeconds.ToString();
        }
    }
}
