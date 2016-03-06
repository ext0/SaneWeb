using Newtonsoft.Json;
using SaneWeb.Data;
using SaneWeb.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SaneWeb.Data
{
    [Serializable()]
    public abstract class Model<T> where T : Model<T>
    {
        private int id;

        public Model() //generate UNIQUE ID
        {
            int id = Utility.randomNumber();
            while (!DBReferences.checkIdUnique<T>(DBReferences.findDBStoring<T>(), id))
            {
                id = Utility.randomNumber();
            }
            this.id = id;
       }

        public int getId()
        {
            return id;
        }
    }
}
