using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fathym.Tests
{
    public class FathymJSONTestsModel
    {
        [JsonExtensionData]
        public virtual Dictionary<string, JsonElement>? Metadata { get; set; }
    }

    [TestClass]
    public class FathymJSONTests
    {
        [TestMethod]
        public void BasicExtensionData()
        {
            var modelStr = @"{""Hey"":""World""}";

            var model = modelStr.FromJSON<FathymJSONTestsModel>();

            Assert.AreEqual(model?.Metadata?["Hey"].ToString(), "World");

            var newModelStr = model.ToJSON();

            Assert.AreEqual(modelStr, newModelStr);
        }
    }
}