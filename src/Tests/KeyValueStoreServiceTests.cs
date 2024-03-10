using System.Threading.Tasks;

namespace StepNet_Tests
{
    /*
    // KeyValueStoreServiceTests.cs

    [TestClass]
    public class KeyValueStoreServiceTests
    {
        private KeyValueStoreService _service;

        [TestInitialize]
        public void SetUp()
        {
            _service = new KeyValueStoreService();
        }

        [TestMethod]
        public async Task Set_SetsAKeyValuePair()
        {
            var request = new KeyValueRequest { Key = "test", Value = "value" };
            var response = await _service.Set(request, null);
            Assert.AreEqual("test", response.Key);
            Assert.AreEqual("value", response.Value);
            Assert.IsTrue(response.Success);
        }

        [TestMethod]
        public async Task Get_GetsAKeyValuePair()
        {
            var setRequest = new KeyValueRequest { Key = "test", Value = "value" };
            await _service.Set(setRequest, null);

            var getRequest = new KeyRequest { Key = "test" };
            var getResponse = await _service.Get(getRequest, null);
            Assert.AreEqual("test", getResponse.Key);
            Assert.AreEqual("value", getResponse.Value);
            Assert.IsTrue(getResponse.Success);
        }

        [TestMethod]
        public async Task Delete_DeletesAKeyValuePair()
        {
            var setRequest = new KeyValueRequest { Key = "test", Value = "value" };
            await _service.Set(setRequest, null);

            var deleteRequest = new KeyRequest { Key = "test" };
            var deleteResponse = await _service.Delete(deleteRequest, null);
            Assert.AreEqual("test", deleteResponse.Key);
            Assert.AreEqual("value", deleteResponse.Value);
            Assert.IsTrue(deleteResponse.Success);
        }
    }
    */
}
