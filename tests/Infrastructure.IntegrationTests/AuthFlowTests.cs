using System.Net;
using System.Net.Http.Json;
using Application.Orders.Dtos;
using Application.Patients.Dtos;
using FluentAssertions;
using WebApi.Contracts.Auth;
using WebApi.Contracts.Orders;
using WebApi.Middleware;

namespace Infrastructure.IntegrationTests;

public class AuthFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private const string RefreshCookieName = "refreshToken";

    private readonly CustomWebApplicationFactory _factory;

    public AuthFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task FullFlow_Login_Crud_Refresh_Logout()
    {
        using var client = _factory.CreateClient();

        // 1. Login with demo credentials
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("demo", "demo"));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        loginBody.Should().NotBeNull();
        var accessToken = loginBody!.AccessToken;
        var refreshToken = CookieHelper.ExtractCookieValue(loginResponse, RefreshCookieName);

        // 2. Access protected endpoint: 5 seeded patients
        using var getPatientsRequest = new HttpRequestMessage(HttpMethod.Get, "/api/patients");
        CookieHelper.AttachBearerToken(getPatientsRequest, accessToken);
        var patientsResponse = await client.SendAsync(getPatientsRequest);
        patientsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var patients = await patientsResponse.Content.ReadFromJsonAsync<List<PatientDto>>();
        patients.Should().HaveCount(5);
        var patientId = patients![0].Id;

        // 3. Create an order for the first patient
        using var createOrderRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/patients/{patientId}/orders")
        {
            Content = JsonContent.Create(new OrderRequest("服用普拿疼"))
        };
        CookieHelper.AttachBearerToken(createOrderRequest, accessToken);
        var createOrderResponse = await client.SendAsync(createOrderRequest);
        createOrderResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await createOrderResponse.Content.ReadFromJsonAsync<OrderDto>();
        order.Should().NotBeNull();

        // 4. Update the order
        using var updateOrderRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/orders/{order!.Id}")
        {
            Content = JsonContent.Create(new OrderRequest("改為服用維他命"))
        };
        CookieHelper.AttachBearerToken(updateOrderRequest, accessToken);
        var updateOrderResponse = await client.SendAsync(updateOrderRequest);
        updateOrderResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedOrder = await updateOrderResponse.Content.ReadFromJsonAsync<OrderDto>();
        updatedOrder!.Message.Should().Be("改為服用維他命");

        // 5. Refresh: old refresh token rotates to a new one
        using var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        CookieHelper.AttachCookie(refreshRequest, RefreshCookieName, refreshToken);
        var refreshResponse = await client.SendAsync(refreshRequest);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var newRefreshToken = CookieHelper.ExtractCookieValue(refreshResponse, RefreshCookieName);
        newRefreshToken.Should().NotBe(refreshToken);
        var newAccessToken = (await refreshResponse.Content.ReadFromJsonAsync<AccessTokenResponse>())!.AccessToken;

        // the old refresh token must no longer work (rotation revokes it)
        using var reuseOldTokenRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        CookieHelper.AttachCookie(reuseOldTokenRequest, RefreshCookieName, refreshToken);
        var reuseOldTokenResponse = await client.SendAsync(reuseOldTokenRequest);
        reuseOldTokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var reuseError = await reuseOldTokenResponse.Content.ReadFromJsonAsync<ErrorResponse>();
        reuseError!.ErrorCode.Should().Be("REFRESH_INVALID");

        // 6. Logout revokes the current refresh token AND blacklists the current access token
        using var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        CookieHelper.AttachCookie(logoutRequest, RefreshCookieName, newRefreshToken);
        CookieHelper.AttachBearerToken(logoutRequest, newAccessToken);
        var logoutResponse = await client.SendAsync(logoutRequest);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var refreshAfterLogoutRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        CookieHelper.AttachCookie(refreshAfterLogoutRequest, RefreshCookieName, newRefreshToken);
        var refreshAfterLogoutResponse = await client.SendAsync(refreshAfterLogoutRequest);
        refreshAfterLogoutResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // the access token that was still valid at logout time must stop working immediately,
        // instead of waiting out its natural expiry
        using var useOldAccessTokenAfterLogoutRequest = new HttpRequestMessage(HttpMethod.Get, "/api/patients");
        CookieHelper.AttachBearerToken(useOldAccessTokenAfterLogoutRequest, newAccessToken);
        var useOldAccessTokenAfterLogoutResponse = await client.SendAsync(useOldAccessTokenAfterLogoutRequest);
        useOldAccessTokenAfterLogoutResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401WithInvalidCredentialsCode()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("demo", "wrong-password"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        body!.ErrorCode.Should().Be("INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401WithTokenExpiredCode()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/patients");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        body!.ErrorCode.Should().Be("TOKEN_EXPIRED");
    }
}
