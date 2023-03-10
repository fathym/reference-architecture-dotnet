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
        public async Task TestFathymPackage()
        {
            var blah = new Fathym.Address()
            {
                City = "Denver",
                Country = "USA"
            };

            var place = $"{blah.City}, {blah.Country}";
            Console.WriteLine(place);
        }

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

        [TestMethod]
        public async Task FlattenUnflatten()
        {
            var modelStr = @"{""Hey"":""World"", ""Complex"": { ""Another"": ""hello"", ""Now"": [ { ""An"": ""Array"" } ] } }";

            var model = modelStr.FromJSON<FathymJSONTestsModel>();

            var flat = model.Flatten();

            Assert.AreEqual("World", flat["Hey"]);
            Assert.AreEqual("hello", flat["Complex.Another"]);
            Assert.AreEqual("Array", flat["Complex.Now[0].An"]);

            var unflat = flat.Unflatten<JsonNode>();

            Assert.AreEqual("World", unflat["Hey"].ToString());
            Assert.AreEqual("hello", unflat["Complex"]["Another"].ToString());
            Assert.AreEqual("Array", unflat["Complex"]["Now"][0]["An"].ToString());
        }
    }
}