using System;

namespace Azi.Amazon.CloudDrive.Tests
{
    public static class AmazonSecret
    {
        // TODO Update to your values or create environment variables
        public static string ClientId = Environment.GetEnvironmentVariable("ACDAPI_CLIENTID");
        public static string ClientSecret = Environment.GetEnvironmentVariable("ACDAPI_CLIENTSECRET");
    }
}