using SaneWeb.Data;
using SaneWeb.Resources.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaneWebHost.Models
{
    [Table("Users")]
    public class User : Model<User>
    {
        [DatabaseValue("sessionToken", 64)]
        public String sessionToken { get; set; }

        [DatabaseValue("username", 64)]
        public String username { get; set; }

        [DatabaseValue("password", 64)]
        public String password { get; set; }

        public User() : base()
        {

        }

        public User(String username, String password) : base()
        {
            this.username = username;
            this.password = password;
        }
    }
}
