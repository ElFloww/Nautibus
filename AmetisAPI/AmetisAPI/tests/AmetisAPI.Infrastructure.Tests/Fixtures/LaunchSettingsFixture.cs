using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AmetisAPI.Infrastructure.Tests.Fixtures
{
    /// <summary>
    /// Fixture for launchsettings. Permit to load variables from json files
    /// </summary>
    public class LaunchSettingsFixture : IDisposable
    {
        private const string Launchsettings = "Properties\\launchSettings.json";
        private const string Profiles = "profiles";
        private const string Environmentvariables = "environmentVariables";

        public LaunchSettingsFixture()
        {
            // if there is launchSettings.json file in project
            if (File.Exists(Launchsettings))
            {
                //read file and get variables
                using StreamReader v_File = File.OpenText(Launchsettings);
                JsonTextReader v_Reader = new(v_File);
                JObject v_JObject = JObject.Load(v_Reader);

                //get environmentVariables from launchSettings.json files
                IEnumerable<JProperty> v_Variables = v_JObject
                    ?.GetValue(Profiles)
                    //select a proper profile here
                    ?.SelectMany(p_Profiles => p_Profiles.Children())
                    .SelectMany(p_Profile => p_Profile.Children<JProperty>())
                    .Where(p_Prop => p_Prop.Name == Environmentvariables)
                    .SelectMany(p_Prop => p_Prop.Value.Children<JProperty>()) ?? [];

                foreach (JProperty v_Variable in v_Variables)
                {
                    //if the variable doesn't exist in the environment variables
                    if (Environment.GetEnvironmentVariable(v_Variable.Name) == null)
                        Environment.SetEnvironmentVariable(v_Variable.Name, v_Variable.Value.ToString());
                }
            }
        }

        /// <summary>
        /// dispose the class
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool p_Disposing)
        {
            //CleanUp
        }
    }
}