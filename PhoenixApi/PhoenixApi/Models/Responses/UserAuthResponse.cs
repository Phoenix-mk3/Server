﻿namespace PhoenixApi.Models.Responses
{
    public class UserAuthResponse
    {
        public Guid UserId { get; set; }
        public string TemporaryAuthToken { get; set; }
    }
}
