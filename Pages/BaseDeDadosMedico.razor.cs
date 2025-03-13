using CentralBemEstarAssemblyIOS.Models;
using CentralBemEstarAssemblyIOS;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

public static class BlazorInterop
{
    public static IJSRuntime _jsRuntime; // Para armazenar a instância de IJSRuntime

    // Método para inicializar o BlazorInterop com IJSRuntime
    public static void Initialize(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    [JSInvokable] // Permite ser chamado pelo JavaScript
    public static async Task<string> GetBaseDeDadosBanco()
    {
         // Agora usando a instância injetada
        DatabaseService _databaseService = new DatabaseService(_jsRuntime);
        var baseDeDados = await _databaseService.ListarBaseDeDadosAsync();

        Dictionary<string, Dictionary<string, double>> listaComDadosPrincipal = new Dictionary<string, Dictionary<string, double>>();

        foreach (var linha in baseDeDados)
        {
            var keyValuePairs = new Dictionary<string, double>
            {
                {"insulina", linha.Insulina},
                {"cho", linha.CHO},
                {"meta", linha.Meta},
                {"fs", linha.FS}
            };
            listaComDadosPrincipal[linha.Refeicao] = keyValuePairs;
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true, // Formatar JSON de maneira legível
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Permitir caracteres especiais
        };

        return JsonSerializer.Serialize(listaComDadosPrincipal, options);
    }

    [JSInvokable] // Permite ser chamado pelo JavaScript
    public static async Task<int> AtualizarDadosViaJson(string JsonAAtualizar)
    {
        DatabaseService _databaseService = new DatabaseService(_jsRuntime);
        return await _databaseService.AtualizarDadosPorJson(JsonAAtualizar);
    }

    [JSInvokable] // Permite ser chamado pelo JavaScript
    public static async Task<int> AtualizarConfig(string LinkGoogleSheet)
    {
        DatabaseService _databaseService = new DatabaseService(_jsRuntime);
        return await _databaseService.SalvarPlanilhaGoogleSheet(LinkGoogleSheet);
    }

    [JSInvokable] // Permite ser chamado pelo JavaScript
    public static async Task<Configs> GetConfig()
    {
        DatabaseService _databaseService = new DatabaseService(_jsRuntime);
        return await _databaseService.GetConfig();
    }

    [JSInvokable] // Permite ser chamado pelo JavaScript
    public static async Task<string> UltimoDowloadDeInformacoaDoGoogleSheet()
    {
        DatabaseService _databaseService = new DatabaseService(_jsRuntime);
        return await _databaseService.GetUltimaAtualizacaoDoGoogleSheet();
    }

    [JSInvokable] // Permite ser chamado pelo JavaScript
    public static async Task<bool> BaixarDadosDaPlanilhaGoogleSheet()
    {
        DatabaseService _databaseService = new DatabaseService(_jsRuntime);
        return await _databaseService.BaixarDadosDoGoogleSheetEInserirNoBanco();
    }
}
