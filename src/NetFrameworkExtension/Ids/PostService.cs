using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace MapIdeaHub.BirSign.NetFrameworkExtension.Ids
{
    public class PostService<T>
    {
        public T Post(string Url, Dictionary<string, string> Headers, object BodyDto, string contentType = "application/json")
        {
            WebRequest request = WebRequest.Create(Url);
            foreach (var data in Headers)
            {
                request.Headers.Add(data.Key, data.Value);
            }
            request.Method = "POST";
            request.ContentType = contentType;
            string postData = JsonConvert.SerializeObject(BodyDto, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            var resp = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<T>(resp);
        }
    }
}
