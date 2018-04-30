namespace GrunCS
{
    using Newtonsoft.Json;

    [JsonObject]
    public class Config
    {
        [JsonProperty]
        public string[] References { get; private set; } = new string[0];
        
        [JsonProperty]
        public string[] Include { get; private set; } = new string[0];
    }
}