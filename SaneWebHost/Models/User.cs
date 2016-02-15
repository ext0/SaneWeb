using SaneWeb.Resources.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaneWebHost.Models
{
    [Table("Sessions")]
    public class Sessions
    {
        [DatabaseValue("sessionToken", 64)]
        public String sessionToken { get; }
    }
}
