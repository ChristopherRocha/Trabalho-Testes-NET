using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GREENHERB.src.Models;
using GREENHERB.src.Services;
using Microsoft.Extensions.Configuration;

// Teste isolado para RenewTokenAsync
class Program
{
    static async Task Main()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "havLMo4mSPwWO5LlLjTYWsw0VpVamc03suk0OfQMiv6",
                ["Jwt:Issuer"] = "GREENHERB",
                ["Jwt:Audience"] = "GREENHERBUsers",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();

        var users = new Dictionary<string, User>();
        var authService = new AuthService(config, users);

        try
        {
            // Teste 1: RenewTokenAsync com token válido
            Console.WriteLine("Teste 1: RenewTokenAsync com token válido");
            await authService.RegisterAsync("johndoe", "john@example.com", "password123", UserRole.Tecnico);
            var authResult = await authService.AuthenticateAsync("johndoe", "password123");
            
            if (authResult == null)
            {
                Console.WriteLine("❌ FALHOU: Não foi possível autenticar");
                return;
            }

            var newToken = await authService.RenewTokenAsync(authResult.Token);
            
            if (newToken == null)
            {
                Console.WriteLine("❌ FALHOU: Token renovado é null");
                return;
            }

            if (newToken == authResult.Token)
            {
                Console.WriteLine("❌ FALHOU: Token renovado é igual ao original");
                return;
            }

            Console.WriteLine("✅ PASSOU: Token foi renovado com sucesso");
            Console.WriteLine($"  Token original: {authResult.Token.Substring(0, 20)}...");
            Console.WriteLine($"  Token renovado: {newToken.Substring(0, 20)}...");

            // Teste 2: RenewTokenAsync com token inválido
            Console.WriteLine("\nTeste 2: RenewTokenAsync com token inválido");
            var result = await authService.RenewTokenAsync("invalid.token.here");

            if (result != null)
            {
                Console.WriteLine("❌ FALHOU: Token inválido deveria retornar null");
                return;
            }

            Console.WriteLine("✅ PASSOU: Token inválido retornou null corretamente");

            Console.WriteLine("\n🎉 TODOS OS TESTES PASSARAM!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERRO: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
