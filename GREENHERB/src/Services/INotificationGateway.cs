namespace GREENHERB.src.Services;

/// <summary>
/// Gateway para envio de notificações.
/// Abstrai a comunicação com o sistema de notificações (email, SMS, push, etc).
/// Sprint 6: Duplo (Stub/Mock) para testes de envio de notificações.
/// </summary>
public interface INotificationGateway
{
    /// <summary>
    /// Envia uma notificação por email.
    /// </summary>
    /// <param name="recipientEmail">Email do destinatário</param>
    /// <param name="subject">Assunto da notificação</param>
    /// <param name="message">Corpo da mensagem</param>
    /// <returns>True se enviado com sucesso, false caso contrário</returns>
    Task<bool> SendEmailNotificationAsync(string recipientEmail, string subject, string message);

    /// <summary>
    /// Envia uma notificação por SMS.
    /// </summary>
    /// <param name="phoneNumber">Número de telefone do destinatário</param>
    /// <param name="message">Conteúdo da mensagem SMS</param>
    /// <returns>True se enviado com sucesso, false caso contrário</returns>
    Task<bool> SendSmsNotificationAsync(string phoneNumber, string message);

    /// <summary>
    /// Envia uma notificação push para um usuário.
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <param name="title">Título da notificação</param>
    /// <param name="message">Corpo da notificação</param>
    /// <returns>True se enviado com sucesso, false caso contrário</returns>
    Task<bool> SendPushNotificationAsync(int userId, string title, string message);

    /// <summary>
    /// Envia uma notificação de alerta sobre condições críticas de um lote.
    /// </summary>
    /// <param name="batchId">ID do lote com problema</param>
    /// <param name="alertType">Tipo de alerta (ex: "TemperatureHigh", "HumidityLow")</param>
    /// <param name="alertMessage">Descrição detalhada do problema</param>
    /// <returns>True se notificação foi processada, false caso contrário</returns>
    Task<bool> SendBatchAlertAsync(int batchId, string alertType, string alertMessage);

    /// <summary>
    /// Verifica o status da conexão com o serviço de notificações.
    /// </summary>
    /// <returns>True se conectado, false caso contrário</returns>
    Task<bool> IsServiceAvailableAsync();

    /// <summary>
    /// Obtém o histórico de notificações enviadas.
    /// </summary>
    /// <param name="limit">Número máximo de notificações a retornar</param>
    /// <returns>Lista de notificações recentes</returns>
    Task<IEnumerable<NotificationLog>> GetSentNotificationsAsync(int limit = 100);
}

/// <summary>
/// Registro de uma notificação enviada.
/// </summary>
public class NotificationLog
{
    public int Id { get; set; }
    public string RecipientIdentifier { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty; // Email, SMS, Push
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
