using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp.Classes
{
    internal class SessionManager
    {
        public static User CurrentUser { get; private set; }
        public static bool IsLoggedIn => CurrentUser != null;
        public static void SignIn(User user)
        {
            CurrentUser = user;
        }
        public static void SignOut()
        {
            CurrentUser = null;
        }
    }
}
