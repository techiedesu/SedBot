using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SedBot.Telegram.Types.Generated;

[JsonSerializable(typeof(ApiResponse<bool>))]
[JsonSerializable(typeof(ApiResponse<WebhookInfo>))]
[JsonSerializable(typeof(ApiResponse<User>))]
[JsonSerializable(typeof(ApiResponse<Update[]>))]
[JsonSerializable(typeof(ReqS.SendMessage))]
[JsonSerializable(typeof(ReqS.SendVoice))]
public partial class ReqJsonSerializerContext : JsonSerializerContext
{
    public static JsonSerializerOptions CreateDefaultOptions()
    {
        var res = new JsonSerializerOptions
        {
            TypeInfoResolver = JsonSerializer.IsReflectionEnabledByDefault
                ? new DefaultJsonTypeInfoResolver()
                : Default
        };
        
        res.Converters.Add(new UnionConverter());
        
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

