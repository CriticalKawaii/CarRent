namespace WpfApp.Classes
{
    /// <summary>
    /// Contains operations to manage registered user.
    /// </summary>
    internal class SessionManager
    {
        public static User CurrentUser { get; private set; }
        public static bool IsLoggedIn => CurrentUser != null;
        public static void SignIn(User user) => CurrentUser = user;
        public static void SignOut() => CurrentUser = null;
    }
}