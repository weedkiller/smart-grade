using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FirestormSW.SmartGrade.Database.Paging
{
    public class PagedObject<T>
    {
        [JsonPropertyName("meta")]
        public PagedObjectMeta Meta { get; set; }
        
        [JsonPropertyName("data")]
        public List<T> Data { get; set; }
    }
}