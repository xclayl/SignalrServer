namespace SignalRServer.Models
{
    public class EnvVarConfig
    {
        public string Token_Generator_Shared_Secret { get; set; }
        public string Token_Symmetric_Key_Base64 { get; set; }
        public string Rest_Api_Cors_Origins { get; set; }
        public string Hubs_Cors_Origins { get; set; }
        public string Allow_Anonymous { get; set; }

        public EnvVarConfig Clone() => (EnvVarConfig)this.MemberwiseClone();
    }
}
