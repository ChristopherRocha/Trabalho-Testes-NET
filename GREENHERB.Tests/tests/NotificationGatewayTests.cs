using GREENHERB.src.Services;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;

namespace GREENHERB.Tests.tests;

/// <summary>
/// Testes para o Gateway de Notificações (MockNotificationGateway).
/// Sprint 6: Validar que o gateway implementa corretamente a lógica de envio de notificações.
/// </summary>
public class NotificationGatewayTests
{
    private readonly MockNotificationGateway _notificationGateway;
    private readonly Mock<ILogger<MockNotificationGateway>> _mockLogger;

    public NotificationGatewayTests()
    {
        _mockLogger = new Mock<ILogger<MockNotificationGateway>>();
        _notificationGateway = new MockNotificationGateway(_mockLogger.Object);
    }

    #region Testes de Envio de Email

    [Fact]
    public async Task SendEmailNotificationAsync_WithValidData_ReturnsTrue()
    {
        // Arrange
        string email = "user@example.com";
        string subject = "Alerta de Temperatura";
        string message = "A temperatura está crítica";

        // Act
        var result = await _notificationGateway.SendEmailNotificationAsync(email, subject, message);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SendEmailNotificationAsync_WithValidData_LogsNotification()
    {
        // Arrange
        string email = "user@example.com";
        string subject = "Teste";
        string message = "Mensagem de teste";

        // Act
        var result = await _notificationGateway.SendEmailNotificationAsync(email, subject, message);
        var sentNotifications = await _notificationGateway.GetSentNotificationsAsync();

        // Assert
        Assert.True(result);
        Assert.NotEmpty(sentNotifications);
        var notification = sentNotifications.First();
        Assert.Equal(email, notification.RecipientIdentifier);
        Assert.Equal("Email", notification.NotificationType);
        Assert.Equal(subject, notification.Subject);
        Assert.True(notification.Success);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendEmailNotificationAsync_WithEmptyEmail_ReturnsFalse(string email)
    {
        // Act
        var result = await _notificationGateway.SendEmailNotificationAsync(email, "Assunto", "Mensagem");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SendEmailNotificationAsync_WhenServiceUnavailable_ReturnsFalse()
    {
        // Arrange
        _notificationGateway.SetServiceAvailability(false);

        // Act
        var result = await _notificationGateway.SendEmailNotificationAsync("user@example.com", "Assunto", "Mensagem");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SendEmailNotificationAsync_WhenServiceUnavailable_LogsFailure()
    {
        // Arrange
        _notificationGateway.SetServiceAvailability(false);
        string email = "user@example.com";

        // Act
        await _notificationGateway.SendEmailNotificationAsync(email, "Assunto", "Mensagem");
        var sentNotifications = await _notificationGateway.GetSentNotificationsAsync();

        // Assert
        var notification = sentNotifications.First();
        Assert.False(notification.Success);
        Assert.NotNull(notification.ErrorMessage);
    }

    #endregion

    #region Testes de Envio de SMS

    [Fact]
    public async Task SendSmsNotificationAsync_WithValidPhoneNumber_ReturnsTrue()
    {
        // Arrange
        string phoneNumber = "9876543210";
        string message = "Alerta de temperatura crítica";

        // Act
        var result = await _notificationGateway.SendSmsNotificationAsync(phoneNumber, message);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SendSmsNotificationAsync_WithValidPhoneNumber_LogsNotification()
    {
        // Arrange
        string phoneNumber = "1234567890";
        string message = "Teste SMS";

        // Act
        var result = await _notificationGateway.SendSmsNotificationAsync(phoneNumber, message);
        var sentNotifications = await _notificationGateway.GetSentNotificationsAsync();

        // Assert
        Assert.True(result);
        var notification = sentNotifications.First();
        Assert.Equal(phoneNumber, notification.RecipientIdentifier);
        Assert.Equal("SMS", notification.NotificationType);
        Assert.Equal(message, notification.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendSmsNotificationAsync_WithEmptyPhone_ReturnsFalse(string phoneNumber)
    {
        // Act
        var result = await _notificationGateway.SendSmsNotificationAsync(phoneNumber, "Mensagem");

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("123")]      // Muito curto
    [InlineData("12345")]    // Muito curto
    public async Task SendSmsNotificationAsync_WithInvalidPhoneLength_ReturnsFalse(string phoneNumber)
    {
        // Act
        var result = await _notificationGateway.SendSmsNotificationAsync(phoneNumber, "Mensagem");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SendSmsNotificationAsync_WhenServiceUnavailable_ReturnsFalse()
    {
        // Arrange
        _notificationGateway.SetServiceAvailability(false);

        // Act
        var result = await _notificationGateway.SendSmsNotificationAsync("1234567890", "Mensagem");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Testes de Notificações Push

    [Fact]
    public async Task SendPushNotificationAsync_WithValidUserId_ReturnsTrue()
    {
        // Arrange
        int userId = 1;
        string title = "Alerta Crítico";
        string message = "Verificar lote imediatamente";

        // Act
        var result = await _notificationGateway.SendPushNotificationAsync(userId, title, message);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SendPushNotificationAsync_WithValidUserId_LogsNotification()
    {
        // Arrange
        int userId = 5;
        string title = "Teste";
        string message = "Mensagem push";

        // Act
        var result = await _notificationGateway.SendPushNotificationAsync(userId, title, message);
        var sentNotifications = await _notificationGateway.GetSentNotificationsAsync();

        // Assert
        Assert.True(result);
        var notification = sentNotifications.First();
        Assert.Contains($"User_{userId}", notification.RecipientIdentifier);
        Assert.Equal("Push", notification.NotificationType);
        Assert.True(notification.Success);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task SendPushNotificationAsync_WithInvalidUserId_ReturnsFalse(int userId)
    {
        // Act
        var result = await _notificationGateway.SendPushNotificationAsync(userId, "Título", "Mensagem");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SendPushNotificationAsync_WhenServiceUnavailable_ReturnsFalse()
    {
        // Arrange
        _notificationGateway.SetServiceAvailability(false);

        // Act
        var result = await _notificationGateway.SendPushNotificationAsync(1, "Título", "Mensagem");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Testes de Alertas de Lote

    [Fact]
    public async Task SendBatchAlertAsync_WithValidData_ReturnsTrue()
    {
        // Arrange
        int batchId = 1;
        string alertType = "TemperatureHigh";
        string alertMessage = "Temperatura acima do limite";

        // Act
        var result = await _notificationGateway.SendBatchAlertAsync(batchId, alertType, alertMessage);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SendBatchAlertAsync_WithValidData_LogsNotification()
    {
        // Arrange
        int batchId = 3;
        string alertType = "HumidityLow";
        string alertMessage = "Umidade crítica";

        // Act
        var result = await _notificationGateway.SendBatchAlertAsync(batchId, alertType, alertMessage);
        var sentNotifications = await _notificationGateway.GetSentNotificationsAsync();

        // Assert
        Assert.True(result);
        var notification = sentNotifications.First();
        Assert.Contains($"Batch_{batchId}", notification.RecipientIdentifier);
        Assert.Equal(alertType, notification.NotificationType);
        Assert.Equal(alertMessage, notification.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SendBatchAlertAsync_WithInvalidBatchId_ReturnsFalse(int batchId)
    {
        // Act
        var result = await _notificationGateway.SendBatchAlertAsync(batchId, "AlertType", "Message");

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendBatchAlertAsync_WithEmptyAlertType_ReturnsFalse(string alertType)
    {
        // Act
        var result = await _notificationGateway.SendBatchAlertAsync(1, alertType, "Mensagem");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SendBatchAlertAsync_WhenServiceUnavailable_ReturnsFalse()
    {
        // Arrange
        _notificationGateway.SetServiceAvailability(false);

        // Act
        var result = await _notificationGateway.SendBatchAlertAsync(1, "AlertType", "Mensagem");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Testes de Status do Serviço

    [Fact]
    public async Task IsServiceAvailableAsync_DefaultState_ReturnsTrue()
    {
        // Act
        var result = await _notificationGateway.IsServiceAvailableAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsServiceAvailableAsync_AfterSetUnavailable_ReturnsFalse()
    {
        // Arrange
        _notificationGateway.SetServiceAvailability(false);

        // Act
        var result = await _notificationGateway.IsServiceAvailableAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsServiceAvailableAsync_AfterSetAvailable_ReturnsTrue()
    {
        // Arrange
        _notificationGateway.SetServiceAvailability(false);
        _notificationGateway.SetServiceAvailability(true);

        // Act
        var result = await _notificationGateway.IsServiceAvailableAsync();

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Testes de Histórico de Notificações

    [Fact]
    public async Task GetSentNotificationsAsync_WithNoNotifications_ReturnsEmpty()
    {
        // Act
        var result = await _notificationGateway.GetSentNotificationsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSentNotificationsAsync_AfterSendingNotifications_ReturnsAll()
    {
        // Arrange
        await _notificationGateway.SendEmailNotificationAsync("email@test.com", "Subj1", "Msg1");
        await _notificationGateway.SendSmsNotificationAsync("1234567890", "SMS Message");
        await _notificationGateway.SendPushNotificationAsync(1, "Title", "Push Message");

        // Act
        var result = await _notificationGateway.GetSentNotificationsAsync();

        // Assert
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetSentNotificationsAsync_WithLimit_Respects_MaxResults()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            await _notificationGateway.SendEmailNotificationAsync($"email{i}@test.com", "Subject", "Message");
        }

        // Act
        var result = await _notificationGateway.GetSentNotificationsAsync(limit: 5);

        // Assert
        Assert.Equal(5, result.Count());
    }

    [Fact]
    public async Task GetSentNotificationsAsync_ReturnsInReverseChronological_Order()
    {
        // Arrange
        await _notificationGateway.SendEmailNotificationAsync("email1@test.com", "Subj1", "Msg1");
        await Task.Delay(100); // Pequeno delay
        await _notificationGateway.SendEmailNotificationAsync("email2@test.com", "Subj2", "Msg2");

        // Act
        var result = await _notificationGateway.GetSentNotificationsAsync();
        var list = result.ToList();

        // Assert
        Assert.True(list[0].SentAt >= list[1].SentAt);
    }

    #endregion

    #region Testes de Contadores

    [Fact]
    public async Task GetSuccessfulNotificationCount_WithOnlySuccessfulNotifications_ReturnsCorrectCount()
    {
        // Arrange
        await _notificationGateway.SendEmailNotificationAsync("email@test.com", "Subj", "Msg");
        await _notificationGateway.SendPushNotificationAsync(1, "Title", "Message");

        // Act
        var count = _notificationGateway.GetSuccessfulNotificationCount();

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task GetFailedNotificationCount_WithMixedResults_ReturnsOnlyFailed()
    {
        // Arrange
        await _notificationGateway.SendEmailNotificationAsync("email@test.com", "Subj", "Msg");
        _notificationGateway.SetServiceAvailability(false);
        await _notificationGateway.SendEmailNotificationAsync("email@test.com", "Subj", "Msg");

        // Act
        var failedCount = _notificationGateway.GetFailedNotificationCount();

        // Assert
        Assert.Equal(1, failedCount);
    }

    #endregion

    #region Testes de Limpeza e Reset

    [Fact]
    public async Task ClearNotificationHistory_RemovesAllNotifications()
    {
        // Arrange
        await _notificationGateway.SendEmailNotificationAsync("email@test.com", "Subj", "Msg");
        await _notificationGateway.SendPushNotificationAsync(1, "Title", "Message");

        // Act
        _notificationGateway.ClearNotificationHistory();
        var result = await _notificationGateway.GetSentNotificationsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Reset_RestoresDefaultState()
    {
        // Arrange
        _notificationGateway.SetServiceAvailability(false);
        await _notificationGateway.SendEmailNotificationAsync("email@test.com", "Subj", "Msg");

        // Act
        _notificationGateway.Reset();
        var isAvailable = await _notificationGateway.IsServiceAvailableAsync();
        var notifications = await _notificationGateway.GetSentNotificationsAsync();

        // Assert
        Assert.True(isAvailable);
        Assert.Empty(notifications);
    }

    #endregion

    #region Testes de Integração

    [Fact]
    public async Task MultipleNotificationTypes_AllTrackedCorrectly()
    {
        // Arrange & Act
        await _notificationGateway.SendEmailNotificationAsync("email@test.com", "Email Subject", "Email Body");
        await _notificationGateway.SendSmsNotificationAsync("1234567890", "SMS Message");
        await _notificationGateway.SendPushNotificationAsync(1, "Push Title", "Push Message");
        await _notificationGateway.SendBatchAlertAsync(1, "TemperatureHigh", "Alert Description");

        // Assert
        var notifications = await _notificationGateway.GetSentNotificationsAsync();
        Assert.Equal(4, notifications.Count());
        Assert.Single(notifications, n => n.NotificationType == "Email");
        Assert.Single(notifications, n => n.NotificationType == "SMS");
        Assert.Single(notifications, n => n.NotificationType == "Push");
        Assert.Single(notifications, n => n.NotificationType == "TemperatureHigh");
    }

    [Fact]
    public async Task ServiceRecovery_AfterBecomingUnavailable()
    {
        // Arrange
        _notificationGateway.SetServiceAvailability(false);
        var failedResult = await _notificationGateway.SendEmailNotificationAsync("email@test.com", "Subj", "Msg");
        Assert.False(failedResult);

        // Act
        _notificationGateway.SetServiceAvailability(true);
        var successResult = await _notificationGateway.SendEmailNotificationAsync("email@test.com", "Subj", "Msg");

        // Assert
        Assert.True(successResult);
        var notifications = await _notificationGateway.GetSentNotificationsAsync();
        Assert.Equal(2, notifications.Count());
        Assert.Single(notifications, n => n.Success);
        Assert.Single(notifications, n => !n.Success);
    }

    #endregion
}
