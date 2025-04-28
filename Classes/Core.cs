using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WpfApp.Classes
{
    internal class Core
    {
        private static DBEntities DBEntities;
        public static DBEntities DataBase = GetContext();
        public static DBEntities GetContext()
        {
            if (DBEntities == null)
                DBEntities = new DBEntities();
            return DBEntities;
        }
    }
}
