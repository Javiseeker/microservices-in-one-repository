using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AMR.Shared;

public static class Constants
{
    public const string VersionHeaderName = "Api-Version";
    public static string ApiName { get; set; } = "API";
    public static string AssemblyDirectory
    {
        get
        {
            string codeBase = Assembly.GetExecutingAssembly().Location!;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path)!;
        }
    }

    public static class Policies
    {
        public static Dictionary<RolesEnum, string[]> Roles { get; private set; } = new()
            {
                { RolesEnum.GeneralUserReadPolicy, new string[]{ "General.Read" } },
                { RolesEnum.GeneralUserWritePolicy, new string[]{ "General.Write" } }
            };

        public static string GetRole(RolesEnum roleValue)
        {
            return roleValue.ToString();
        }

        public enum RolesEnum
        {
            GeneralUserReadPolicy,
            GeneralUserWritePolicy
        }
    }
}
