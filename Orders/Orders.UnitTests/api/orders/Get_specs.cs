using FluentAssertions;
using Orders.Commands;
using System.Net.Http.Json;
using Xunit;

namespace Orders.api.orders;

public class Get_specs
{
    [Fact]
    public async Task Sut_correctly_filters_orders_by_user_id()
    {
        // Arrange
        HttpClient client = OrdersServer.Create().CreateClient();

        Guid shopId = Guid.NewGuid();
        Guid itemId = Guid.NewGuid();
        decimal price = 100000;

        List<PlaceOrder> commands = new()
        {
            new PlaceOrder(UserId: Guid.NewGuid(), shopId, itemId, price),
            new PlaceOrder(UserId: Guid.NewGuid(), shopId, itemId, price),
            new PlaceOrder(UserId: Guid.NewGuid(), shopId, itemId, price),
        };

        await Task.WhenAll(from command in commands
                           let orderId = Guid.NewGuid()
                           let uri = $"api/orders/{orderId}/place-order"
                           orderby orderId
                           select client.PostAsJsonAsync(uri, command));

        Guid userId = commands[0].UserId;

        // Act
        string queryUri = $"api/orders?user-id={userId}";
        HttpResponseMessage response = await client.GetAsync(queryUri);

        // Assert
        Order[]? orders = await response.Content.ReadFromJsonAsync<Order[]>();
        orders.Should().OnlyContain(x => x.UserId == userId);
    }
   
}