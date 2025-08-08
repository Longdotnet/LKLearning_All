using Newtonsoft.Json;

namespace WEBAPI.Services.Models
{
    public class InfoBipMessage
    {
        [JsonProperty("messages")]
        public List<Message> messages { get; set; }
    }

    public class Message
    {
        [JsonProperty("from")]
        public string from { get; set; }

        [JsonProperty("destinations")]
        public List<Destination> destinations { get; set; }

        [JsonProperty("text")]
        public string text { get; set; }
    }

    public class Destination
    {
        [JsonProperty("to")]
        public string to { get; set; }
    }

    public class InfoBipEmailMessage
    {
        [JsonProperty("messages")]
        public List<EmailMessage> messages { get; set; }
    }

    public class EmailMessage
    {
        [JsonProperty("sender")]
        public string sender { get; set; }

        [JsonProperty("destinations")]
        public List<EmailDestination> destinations { get; set; }

        [JsonProperty("content")]
        public EmailContent content { get; set; }
    }

    public class EmailDestination
    {
        [JsonProperty("to")]
        public List<ToDestination> to { get; set; }
    }

    public class ToDestination
    {
        [JsonProperty("destination")]
        public string destination { get; set; }
    }

    public class EmailContent
    {
        [JsonProperty("subject")]
        public string subject { get; set; }

        [JsonProperty("text")]
        public string text { get; set; }

        [JsonProperty("html")]
        public string html { get; set; }
    }
}