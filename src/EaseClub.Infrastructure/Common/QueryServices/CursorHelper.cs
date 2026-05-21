using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EaseClub.Infrastructure.Common.QueryServices
{

    public static class CursorHelper
    {
        public static string Encode<T>(T cursor)
        {
            var json = JsonSerializer.Serialize(cursor);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        public static T Decode<T>(string cursor)
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            return JsonSerializer.Deserialize<T>(json)!;
        }
    }

}
