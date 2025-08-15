using Foody.Models;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;


namespace Foody
{
    [TestFixture]
    public class FoodyTests
    {
        private RestClient client;
        private static string? createdFoodId;
        //your link here
        private const string baseUrl = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86";

        [OneTimeSetUp]
        public void Setup()
        {
            // your credentials
            string token = GetJwtToken("vasilena456", "vasilena456");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);

            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;

        }

        //Assert examples

        //Example for status code
        // Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

        //Example for Message
        // Assert.That(response.Content, Does.Contain("No food revues..."));



        //Change names of all tests

        [Test, Order(1)]
        public void CreateFood_ShouldReturnCreated()
        {
            var food = new
            {
                name = "New Food",
                description = "This is a test food description",
                url = ""
            };
            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(food);

            var response = client.Execute(request);

            // Print response for debugging
            TestContext.WriteLine("Create response: " + response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), "Expected status code 201 Created.");

            // Try to extract foodId from the response
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            // If the response is wrapped, adjust accordingly
            if (json.TryGetProperty("foodId", out var idProp))
            {
                createdFoodId = idProp.GetString() ?? string.Empty;
            }
            else if (json.TryGetProperty("data", out var dataProp) && dataProp.TryGetProperty("foodId", out var nestedIdProp))
            {
                createdFoodId = nestedIdProp.GetString() ?? string.Empty;
            }
            else
            {
                createdFoodId = string.Empty;
            }

            Assert.That(createdFoodId, Is.Not.Null.And.Not.Empty, "createdFoodId must be present in the response.");
        }

        [Test, Order(2)]

        public void EditFoodTitle_ShouldReturnOk()
        {
            var changes = new[]
            {
                new { path = "/name", op = "replace", value = "Updated food name" }
            };

            var request = new RestRequest($"/api/Food/Edit/{createdFoodId}", Method.Patch);

            request.AddJsonBody(changes);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            Assert.That(response.Content, Does.Contain("Successfully edited"));

        }

        [Test, Order(3)]

        public void GetAllFoods_ShouldReturnList()
        {
            var request = new RestRequest("/api/Food/All", Method.Get);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var foods = JsonSerializer.Deserialize<List<FoodDTO>>(response.Content);

            Assert.That(foods, Is.Not.Empty);
        }



        [Test, Order(4)]

        public void DeleteFood_ShoudReturnOk()
        {
            var request = new RestRequest($"/api/Food/Delete/{createdFoodId}", Method.Delete);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            Assert.That(response.Content, Does.Contain("Deleted successfully!"));

        }


        [Test, Order(5)]

        public void CreateFood_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var foodNew = new
            {
                Name = "",
                Description = ""

            };

            var request = new RestRequest("api/Food/Create", Method.Post);
            request.AddJsonBody(foodNew);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }


        [Test, Order(6)]

        public void EditNonExistingFood_ShouldReturnNotFound()
        {
            string fakeId = "123";

            var changes = new[]
            {
                new { path = "/name", op = "replace", value = "New title" }
            };

            var request = new RestRequest($"/api/Food/Edit/{fakeId}", Method.Patch);
            request.AddJsonBody(changes);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            Assert.That(response.Content, Does.Contain("No food revues..."));

        }

        [Test, Order(7)]

        public void DeleteNonExistingFood_ShouldReturnBadRequest()
        {
            string fakeId = "123";

            var request = new RestRequest($"/api/Food/Delete/{fakeId}", Method.Delete);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            Assert.That(response.Content, Does.Contain("Unable to delete this food revue!"));

        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}