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
        /// <summary>
        /// Internal ID for the SQLite database
        /// </summary>
        private int ID;

        /// <summary>
        /// Creates a generic Model object with a random unique ID
        /// </summary>
        public Model()
        {
            int id = Utility.randomNumber();
            while (!DBReferences.CheckIdUnique<T>(DBReferences.FindDBStoring<T>(), id))
            {
                id = Utility.randomNumber();
            }
            this.ID = id;
        }

        /// <summary>
        /// Gets the ID for this Model
        /// </summary>
        /// <returns>A unique ID</returns>
        public int GetId()
        {
            return ID;
        }
    }
}
