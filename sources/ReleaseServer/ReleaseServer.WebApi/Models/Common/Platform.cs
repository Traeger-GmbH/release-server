//--------------------------------------------------------------------------------------------------
// <copyright file="Platform.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Fabian Traeger</author>
//--------------------------------------------------------------------------------------------------

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Represents a platform that consists of combination of operating system and architecture.
    /// </summary>
    public class Platform
    {
        #region ---------- Private constant fields ----------

        private const char Separator = '/';

        #endregion

        #region ---------- Public constructors ----------

        /// <summary>
        /// Creates a new Platform instance.
        /// </summary>
        /// <param name="operatingSystem"></param>
        /// <param name="architecture"></param>
        public Platform(string operatingSystem, string architecture)
        {
            this.OperatingSystem = operatingSystem;
            this.Architecture = architecture;
        }

        #endregion

        #region ---------- Public properties ----------

        /// <summary>
        /// 
        /// </summary>
        public string OperatingSystem { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Architecture { get; set; }

        #endregion

        #region ---------- Public static methodss ----------

        /// <summary>
        /// Tries to parse a platform from a string
        /// </summary>
        /// <returns></returns>
        public static bool TryParse(string s, out Platform result)
        {
            var items = s.Split(Separator);
            if (items.Length == 2) {
                result = new Platform(items[0], items[1]);
                return true;
            }
            else {
                result = null;
                return false;
            }
        }

        #endregion


        #region ---------- Public methodss ----------

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{this.OperatingSystem}{Separator}{this.Architecture}";
        }

        #endregion
    }
}