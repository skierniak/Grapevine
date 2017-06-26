namespace Grapevine.Core.Json
{
    public interface IGrapevineJsonProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        string Serialize<T>(T obj);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        T Deserialize<T>(string json);
    }

    public class JsonNotImplementedProvider : IGrapevineJsonProvider
    {
        public string Serialize<T>(T obj)
        {
            throw new System.NotImplementedException("An IGrapevineJsonProvider has not been registered");
        }

        public T Deserialize<T>(string json)
        {
            throw new System.NotImplementedException("An IGrapevineJsonProvider has not been registered");
        }
    }
}
