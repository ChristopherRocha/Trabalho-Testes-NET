namespace GREENHERB.src.Services;

/// <summary>
/// Mock/Stub do Gateway de Notificações.
/// Simula envio de notificações para testes.
/// Sprint 6: Duplo para testes do gateway de notificações.
/// </summary>
public class MockNotificationGateway : INotificationGateway
{
    private readonly ILogger<MockNotificationGateway> _logger;
    private readonly List<NotificationLog> _sentNotifications = new();
    private bool _isServiceAvailable = true;

    public MockNotificationGateway(ILogger<MockNotificationGateway> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Simula envio de notificação por email.
    /// </summary>
    public async Task<bool> SendEmailNotificationAsync(string recipientEmail, string subject, string message)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            _logger.LogWarning("Tentativa de enviar email com destinatário vazio");
            return false;
        }

        if (!_isServiceAvailable)
        {
            _logger.LogWarning("Serviço de notificação indisponível");
            LogNotification(recipientEmail, "Email", subject, message, false, "Serviço indisponível");
            return false;
        }

        try
        {
            // Simula envio bem-sucedido
            _logger.LogInformation("Email enviado para {email}: {subject}", recipientEmail, subject);
            LogNotification(recipientEmail, "Email", subject, message, true);
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email para {email}", recipientEmail);
            LogNotification(recipientEmail, "Email", subject, message, false, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Simula envio de notificação por SMS.
    /// </summary>
    public async Task<bool> SendSmsNotificationAsync(string phoneNumber, string message)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            _logger.LogWarning("Tentativa de enviar SMS com número vazio");
            return false;
        }

        if (!_isServiceAvailable)
        {
            _logger.LogWarning("Serviço de notificação indisponível");
            LogNotification(phoneNumber, "SMS", "SMS Notification", message, false, "Serviço indisponível");
            return false;
        }

        try
        {
            // Valida formato básico de número
            if (phoneNumber.Length < 10)
            {
                throw new InvalidOperationException("Número de telefone inválido");
            }

            _logger.LogInformation("SMS enviado para {phone}: {message}", phoneNumber, message);
            LogNotification(phoneNumber, "SMS", "SMS Notification", message, true);
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar SMS para {phone}", phoneNumber);
            LogNotification(phoneNumber, "SMS", "SMS Notification", message, false, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Simula envio de notificação push.
    /// </summary>
    public async Task<bool> SendPushNotificationAsync(int userId, string title, string message)
    {
        if (userId <= 0)
        {
            _logger.LogWarning("Tentativa de enviar push com userId inválido: {userId}", userId);
            return false;
        }

        if (!_isServiceAvailable)
        {
            _logger.LogWarning("Serviço de notificação indisponível");
            LogNotification($"User_{userId}", "Push", title, message, false, "Serviço indisponível");
            return false;
        }

        try
        {
            _logger.LogInformation("Notificação push enviada para usuário {userId}: {title}", userId, title);
            LogNotification($"User_{userId}", "Push", title, message, true);
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar push para usuário {userId}", userId);
            LogNotification($"User_{userId}", "Push", title, message, false, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Simula envio de alerta de lote.
    /// </summary>
    public async Task<bool> SendBatchAlertAsync(int batchId, string alertType, string alertMessage)
    {
        if (batchId <= 0)
        {
            _logger.LogWarning("Tentativa de enviar alerta de lote com ID inválido: {batchId}", batchId);
            return false;
        }

        if (string.IsNullOrWhiteSpace(alertType))
        {
            _logger.LogWarning("Tipo de alerta vazio para batch {batchId}", batchId);
            return false;
        }

        if (!_isServiceAvailable)
        {
            _logger.LogWarning("Serviço de notificação indisponível para alerta do batch {batchId}", batchId);
            LogNotification($"Batch_{batchId}", alertType, alertType, alertMessage, false, "Serviço indisponível");
            return false;
        }

        try
        {
            _logger.LogInformation(
                "Alerta de lote enviado para batch {batchId}: {type} - {message}",
                batchId, alertType, alertMessage);

            LogNotification($"Batch_{batchId}", alertType, alertType, alertMessage, true);
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar alerta para batch {batchId}", batchId);
            LogNotification($"Batch_{batchId}", alertType, alertType, alertMessage, false, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Verifica disponibilidade do serviço.
    /// </summary>
    public async Task<bool> IsServiceAvailableAsync()
    {
        _logger.LogInformation("Status do serviço de notificações: {status}", 
            _isServiceAvailable ? "Disponível" : "Indisponível");
        return await Task.FromResult(_isServiceAvailable);
    }

    /// <summary>
    /// Retorna o histórico de notificações enviadas.
    /// </summary>
    public async Task<IEnumerable<NotificationLog>> GetSentNotificationsAsync(int limit = 100)
    {
        var result = _sentNotifications.OrderByDescending(n => n.SentAt).Take(limit);
        _logger.LogInformation("Retornando {count} notificações do histórico", result.Count());
        return await Task.FromResult(result);
    }

    // ===== MÉTODOS AUXILIARES PARA TESTES =====

    /// <summary>
    /// Define manualmente a disponibilidade do serviço (para testes).
    /// </summary>
    public void SetServiceAvailability(bool isAvailable)
    {
        _isServiceAvailable = isAvailable;
        _logger.LogDebug("Disponibilidade do serviço alterada para: {status}", isAvailable);
    }

    /// <summary>
    /// Registra uma notificação no histórico.
    /// </summary>
    private void LogNotification(string recipientIdentifier, string notificationType, 
        string subject, string message, bool success, string? errorMessage = null)
    {
        var log = new NotificationLog
        {
            Id = _sentNotifications.Count + 1,
            RecipientIdentifier = recipientIdentifier,
            NotificationType = notificationType,
            Subject = subject,
            Message = message,
            Success = success,
            ErrorMessage = errorMessage,
            SentAt = DateTime.UtcNow
        };

        _sentNotifications.Add(log);
    }

    /// <summary>
    /// Limpa o histórico de notificações (para testes).
    /// </summary>
    public void ClearNotificationHistory()
    {
        _sentNotifications.Clear();
        _logger.LogDebug("Histórico de notificações limpo");
    }

    /// <summary>
    /// Obtém o número de notificações enviadas com sucesso.
    /// </summary>
    public int GetSuccessfulNotificationCount()
    {
        return _sentNotifications.Count(n => n.Success);
    }

    /// <summary>
    /// Obtém o número de notificações falhadas.
    /// </summary>
    public int GetFailedNotificationCount()
    {
        return _sentNotifications.Count(n => !n.Success);
    }

    /// <summary>
    /// Reseta o estado do mock (para testes).
    /// </summary>
    public void Reset()
    {
        _sentNotifications.Clear();
        _isServiceAvailable = true;
        _logger.LogInformation("MockNotificationGateway resetado");
    }
}
