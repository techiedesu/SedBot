using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using SedBot.Telegram.BotApi.Types.CoreTypes;

namespace SedBot.Telegram.BotApi.Types.Generated;

[JsonSerializable(typeof(ApiResponse<bool>))]
[JsonSerializable(typeof(ApiResponse<WebhookInfo>))]
[JsonSerializable(typeof(ApiResponse<User>))]
[JsonSerializable(typeof(ApiResponse<Update[]>))]
[JsonSerializable(typeof(Req.SendMessage))]
[JsonSerializable(typeof(Req.SendVoice))]
[JsonSerializable(typeof(ApiResponse<Message>))]
[JsonSerializable(typeof(BotConfig))]
[JsonSerializable(typeof(IRequestBase<Message>))]
[JsonSerializable(typeof(IRequestBase<bool>))]
[JsonSerializable(typeof(ApiResponseError))]
public sealed partial class ReqJsonSerializerContext : JsonSerializerContext
{
    public static void Apply(JsonSerializerOptions s)
    {
        s.TypeInfoResolver = Default;
        s.Converters.Add(new ChatIdUnionConverter());
    }

    [SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
    public static JsonSerializerOptions CreateDefaultOptions()
    {
        var res = new JsonSerializerOptions();
        
        Apply(res);
        var fsharpOptions =
            JsonFSharpOptions.Default()
                .WithTypes(
                    JsonFSharpTypes.Unions
                    | JsonFSharpTypes.Records
                    | JsonFSharpTypes.Tuples
                    | JsonFSharpTypes.Lists
                    | JsonFSharpTypes.Sets
                    | JsonFSharpTypes.Maps
                    | JsonFSharpTypes.Options
                    | JsonFSharpTypes.ValueOptions)
                .WithSkippableOptionFields()
                .WithUnionTagCaseInsensitive()
                .WithUnionUnwrapFieldlessTags()
                .WithAllowNullFields();
        fsharpOptions.AddToJsonSerializerOptions(res);
        
        res.UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip;
        res.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
        res.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        
        return res;
    }
}

