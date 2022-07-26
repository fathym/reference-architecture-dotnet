using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Fathym.Tests
{
    public class FathymJSONTestsModel : MetadataModel
    {
    }

    [TestClass]
    public class FathymJSONTests
    {
        [TestMethod]
        public async Task BasicExtensionData()
        {
            var modelStr = @"{""Hey"":""World""}";

            var model = modelStr.FromJSON<FathymJSONTestsModel>();

            Assert.AreEqual(model?.Metadata?["Hey"]?.As<string>(), "World");

            model.Metadata["Hey"] = true;

            var newModelStr = model.ToJSON();

            var expectedModelStr = modelStr.Replace(@"""World""", "true");

            Assert.AreEqual(expectedModelStr, newModelStr);

            model = newModelStr.FromJSON<FathymJSONTestsModel>();

            Assert.AreEqual(model?.Metadata?["Hey"]?.As<bool>(), true);

            var newtonStr = Newtonsoft.Json.JsonConvert.SerializeObject(model);

            //Assert.AreEqual(expectedModelStr, newtonStr);

            model = Newtonsoft.Json.JsonConvert.DeserializeObject<FathymJSONTestsModel>(newModelStr);

            Assert.AreEqual(model?.Metadata?["Hey"]?.As<bool>(), true);
        }
    }
}