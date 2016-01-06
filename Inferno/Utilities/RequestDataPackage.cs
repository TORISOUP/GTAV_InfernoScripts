using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Inferno
{
    [DataContract]
    internal class RequestDataPackage
    {
        private DataContractJsonSerializer jsonSerializer;

        public RequestDataPackage(string text)
        {
            jsonSerializer = new DataContractJsonSerializer(typeof(RequestDataPackage));
            this.name = "";
            this.isInterrupted = true;
            this.text = text;
            this.tag = "";
            this.emotion = "";
        }

        public RequestDataPackage()
        {
            ;
        }

        public string ToJson()
        {
            string result = "";
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

        /// <summary>
        /// キャラクタのアニメーション
        /// </summary>
        [DataMember]
        public string emotion;

        /// <summary>
        /// コメントのカラー
        /// </summary>
        [DataMember]
        public string tag;

        /// <summary>
        /// 読み上げるメッセージ
        /// </summary>
        [DataMember]
        public string text;

        /// <summary>
        /// コメント投稿者
        /// </summary>
        [DataMember]
        public string name;

        /// <summary>
        /// 運営コメントかどうか
        /// </summary>
        [DataMember]
        public bool isInterrupted;
    }
}
