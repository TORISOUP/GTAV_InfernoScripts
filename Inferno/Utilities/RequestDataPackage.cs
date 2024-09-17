using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Inferno
{
    [DataContract]
    internal class RequestDataPackage
    {
        private readonly DataContractJsonSerializer jsonSerializer;

        /// <summary>
        /// キャラクタのアニメーション
        /// </summary>
        [DataMember] public string emotion;

        /// <summary>
        /// 運営コメントかどうか
        /// </summary>
        [DataMember] public bool isInterrupted;

        /// <summary>
        /// コメント投稿者
        /// </summary>
        [DataMember] public string name;

        /// <summary>
        /// コメントのカラー
        /// </summary>
        [DataMember] public string tag;

        /// <summary>
        /// 読み上げるメッセージ
        /// </summary>
        [DataMember] public string text;

        public RequestDataPackage(string text)
        {
            jsonSerializer = new DataContractJsonSerializer(typeof(RequestDataPackage));
            name = "";
            isInterrupted = true;
            this.text = text;
            tag = "";
            emotion = "";
        }

        public RequestDataPackage()
        {
            ;
        }

        public string ToJson()
        {
            var result = "";
            using (var stream = new MemoryStream())
            {
                jsonSerializer.WriteObject(stream, this);

                stream.Position = 0;
                var reader = new StreamReader(stream);
                result = reader.ReadToEnd();
            }

            return result;
        }

        public static RequestDataPackage FromJson(string json)
        {
            var jsonSerializer = new DataContractJsonSerializer(typeof(RequestDataPackage));
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return (RequestDataPackage)jsonSerializer.ReadObject(stream);
            }
        }
    }
}