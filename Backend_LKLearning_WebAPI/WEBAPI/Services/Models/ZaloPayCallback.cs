using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

public class ZaloPayCallback
{
    [Required]
    [JsonProperty("data")]
    public string Data { get; set; } = string.Empty;

    [Required]
    [JsonProperty("mac")]
    public string Mac { get; set; } = string.Empty;

    [JsonProperty("type")]
    public int Type { get; set; }
}

public class ZaloPayCallbackData
{
    [JsonProperty("app_id")]
    public int AppId { get; set; }

    [JsonProperty("app_trans_id")]
    public string AppTransId { get; set; } = string.Empty;

    [JsonProperty("zp_trans_id")]
    public string ZpTransId { get; set; } = string.Empty;

    [JsonProperty("order_id")]
    public string OrderId { get; set; } = string.Empty;

    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("amount")]
    public long Amount { get; set; }

    [JsonProperty("app_user")]
    public string AppUser { get; set; } = string.Empty;

    [JsonProperty("app_time")]
    public long AppTime { get; set; }

    [JsonProperty("embed_data")]
    public string EmbedData { get; set; } = string.Empty;

    [JsonProperty("item")]
    public string Item { get; set; } = string.Empty;

    [JsonProperty("server_time")]
    public long ServerTime { get; set; }

    [JsonProperty("channel")]
    public int Channel { get; set; }

    [JsonProperty("merchant_user_id")]
    public string MerchantUserId { get; set; } = string.Empty;

    [JsonProperty("zp_user_id")]
    public string ZpUserId { get; set; } = string.Empty;

    [JsonProperty("user_fee_amount")]
    public long UserFeeAmount { get; set; }

    [JsonProperty("discount_amount")]
    public long DiscountAmount { get; set; }
}