using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWMS.Influx.Module.Services
{
    public class EnvironmentVariableService
    {
        public static int GetRequiredIntFromENV(string env)
        {
            var envString = Environment.GetEnvironmentVariable(env);
            return envString == null ? throw new ArgumentException($"{env} not provided in EnvironmentVariables!") : int.Parse(envString);
        }
        public static string GetRequiredStringFromENV(string env)
        {
            var envString = Environment.GetEnvironmentVariable(env);
            return envString ?? throw new ArgumentNullException(env, $"{env} not provided in EnvironmentVariables!");
        }
    }
}
