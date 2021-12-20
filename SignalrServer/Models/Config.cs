﻿namespace SignalrServer.Models
{
    public class Config
    {
        public string Token_Generator_Shared_Secret { get; set; }
        public string Token_Symmetric_Key_Base64 { get; set; }
        public string Rest_Api_Cors_Origins { get; set; }
        public string Hubs_Cors_Origins { get; set; }
    }
}
