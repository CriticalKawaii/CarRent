using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp.Classes
{
    internal class Security
    {
        public Security() { }
        public string GetHash(string String)
        {
            using (var hash = SHA1.Create()) {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(String)).Select(s => s.ToString("X2")));
            }
        }
    }
}
