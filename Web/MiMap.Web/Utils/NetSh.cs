using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiMap.Web.Utils
{

    /// <summary>
    /// Executes NetSh commands
    /// 
    /// Originally from: https://github.com/NancyFx/Nancy/tree/master/src/Nancy.Hosting.Self/NetSh.cs
    /// </summary>
    public static class NetSh
    {
        private const string NetshCommand = "netsh";

        /// <summary>
        /// Add a url reservation
        /// </summary>
        /// <param name="url">Url to add</param>
        /// <param name="user">User to add the reservation for</param>
        /// <returns>True if successful, false otherwise.</returns>
        public static bool AddUrlAcl(string url, string user)
        {
            try
            {
                var arguments = GetParameters(url, user);

                return UacHelper.RunElevated(NetshCommand, arguments);
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal static string GetParameters(string url, string user)
        {
            return string.Format("http add urlacl url=\"{0}\" user=\"{1}\"", url, user);
        }
    }
}
