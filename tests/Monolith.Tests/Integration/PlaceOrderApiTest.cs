using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Monolith.Tests.Integration;

[Collection("api")]
public class PlaceOrderApiTest(MonolithApiFixture fixture)
{
    [Fact]
    public async Task PlaceOrder_ReturnsCreated()
    {
        var client = fixture.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            customerName = "John Doe",
            totalAmount = 99.99m
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var id = body.GetProperty("id").GetGuid();
        Assert.NotEqual(Guid.Empty, id);

        var getResponse = await client.GetAsync($"/api/orders/{id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }
}
