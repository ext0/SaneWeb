using Newtonsoft.Json;
using SaneWeb.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            String db = DBReferences.findDBStoring<T>();
            int id = new Random().Next(0, int.MaxValue);
            while (!DBReferences.checkIdUnique<T>(db, id)) { }
            this.id = id;
        }

        public int getId()
        {
            return id;
        }

        public static T deepClone(T source)
        {
            String serialized = JsonConvert.SerializeObject(source);
            T obj = JsonConvert.DeserializeObject<T>(serialized);
            obj.id = source.id;
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}
