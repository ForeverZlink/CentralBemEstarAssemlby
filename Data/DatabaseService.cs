using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CentralBemEstarAssemblyIOS.Models;
using Microsoft.JSInterop;


public class DatabaseService
{
    private  IJSRuntime _jsRuntime;

    public DatabaseService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;

    }
    public async Task InitializeAsync(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        // Adicione a lógica de inicialização necessária aqui.
    }
    public async Task InicializarBancoDeDados()
    {
        await _jsRuntime.InvokeVoidAsync("indexedDBHelper.initializeDatabase");
    }


    // Salvar o usuário no IndexedDB
    public async Task<int> SalvarUsuarioAsync(BaseDeDadosMedicos usuario)
    {
        await _jsRuntime.InvokeVoidAsync("indexedDB.saveItem", "usuarios", usuario.Refeicao, usuario);
        return 1; // Retorna 1 como sucesso (já que IndexedDB não tem resposta direta)
    }

    // Salvar o link da planilha Google Sheet
    public async Task<int> SalvarPlanilhaGoogleSheet(string config)
    {
        var configs = await GetConfig();
        if(configs == null)
        {
            configs = new Configs() { LinkGoogleSheet = config,IdIdentificao=Guid.NewGuid().ToString() };
        }
        else
        {
            configs.LinkGoogleSheet = config.Trim();
        }

        await _jsRuntime.InvokeVoidAsync("indexedDBHelper.saveItem", "configs", "UNICO", configs);
        return 1;
    }

    // Atualizar os dados de refeições no IndexedDB
    public async Task<int> AtualizarDadosAsync(List<BaseDeDadosMedicos> listaComRefeicoesParaAtualizar)
    {
        foreach (var item in listaComRefeicoesParaAtualizar)
        {
            await _jsRuntime.InvokeVoidAsync("indexedDB.saveItem", "usuarios", item.Refeicao, item);
        }
        return 1;
    }

    // Atualizar dados a partir de um JSON
    public async Task<int> AtualizarDadosPorJson(string json)
    {
        var jsonTratado = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, double>>>(json);
        List<BaseDeDadosMedicos> baseDeDadosMedicosAtualizado = new List<BaseDeDadosMedicos>();

        foreach (var linha in jsonTratado)
        {
            BaseDeDadosMedicos baseDeDadosMedicos = new BaseDeDadosMedicos
            {
                Refeicao = linha.Key,
                Insulina = linha.Value["insulina"],
                CHO = linha.Value["cho"],
                Meta = linha.Value["meta"],
                FS = linha.Value["fs"]
            };
            baseDeDadosMedicosAtualizado.Add(baseDeDadosMedicos);
        }

        foreach (var item in baseDeDadosMedicosAtualizado)
        {
            await _jsRuntime.InvokeVoidAsync("indexedDB.saveItem", "usuarios", item.Refeicao, item);
        }
        return 1;
    }

    // Inserir dados iniciais
    public async Task InserirDadosIniciais()
    {
        var totalRegistros = await _jsRuntime.InvokeAsync<int>("indexedDB.getItemCount", "usuarios");
        var linkGoogleSheet = await _jsRuntime.InvokeAsync<int>("indexedDB.getItemCount", "configs");

        if (totalRegistros == 0)
        {
            var configuracoesPadrao = new List<BaseDeDadosMedicos>
            {
                new BaseDeDadosMedicos { Refeicao = "CAFÉ" },
                new BaseDeDadosMedicos { Refeicao = "ALMOÇO" },
                new BaseDeDadosMedicos { Refeicao = "LANCHE" },
                new BaseDeDadosMedicos { Refeicao = "JANTAR" },
                new BaseDeDadosMedicos { Refeicao = "CEIA" }
            };
            foreach (var item in configuracoesPadrao)
            {
                await _jsRuntime.InvokeVoidAsync("indexedDB.saveItem", "usuarios", item.Refeicao, item);
            }
        }

        if (linkGoogleSheet == 0)
        {
            Configs configs = new Configs { IdIdentificao = "UNICO" };
            await _jsRuntime.InvokeVoidAsync("indexedDB.saveItem", "configs", "UNICO", configs);
        }
        else
        {
            await BaixarDadosDoGoogleSheetEInserirNoBanco();
        }
    }

    // Baixar os dados do Google Sheets e inserir no IndexedDB
    public async Task<bool> BaixarDadosDoGoogleSheetEInserirNoBanco()
    {
        var configuracaoGoogleSheet = await _jsRuntime.InvokeAsync<Configs>("indexedDB.getItem", "configs", "UNICO");
        try
        {
            var partes = configuracaoGoogleSheet.LinkGoogleSheet.Split('/');
            var id = partes[5];
            string urlTransformadaParaCSV = $"https://docs.google.com/spreadsheets/d/{id}/export?format=csv";
            var httpClient = new HttpClient();
            var resposta = await httpClient.GetStringAsync(urlTransformadaParaCSV);

            var linhas = resposta.Split("\n");
            var contador = 0;
            List<BaseDeDadosMedicos> baseDeDadosParaAdicionarNoBanco = new List<BaseDeDadosMedicos>();

            foreach (var linha in linhas)
            {
                if (contador == 0)
                {
                    contador++;
                    continue;
                }

                var colunas = linha.Replace("\r", "").Split(",");
                string refeicao = colunas[0];
                double insulina = Convert.ToDouble(colunas[1].Replace(".", ","));
                double cho = Convert.ToDouble(colunas[2].Replace(".", ","));
                double meta = Convert.ToDouble(colunas[3].Replace(".", ","));
                double fs = Convert.ToDouble(colunas[4].Replace(".", ","));

                BaseDeDadosMedicos baseDeDadosMedicos = new BaseDeDadosMedicos
                {
                    Refeicao = refeicao,
                    Insulina = insulina,
                    CHO = cho,
                    Meta = meta,
                    FS = fs,
                    UltimaAtualizacaoServicoExterno = DateTime.Now
                };
                baseDeDadosParaAdicionarNoBanco.Add(baseDeDadosMedicos);
                await _jsRuntime.InvokeVoidAsync("indexedDB.saveItem", "usuarios", refeicao, baseDeDadosMedicos);
                contador++;
            }

            var todosOsDados = await _jsRuntime.InvokeAsync<List<BaseDeDadosMedicos>>("indexedDB.getAllItems", "usuarios");
            var itensParaRemover = todosOsDados
                .Where(a1 => !baseDeDadosParaAdicionarNoBanco.Any(a2 => a1.Refeicao == a2.Refeicao))
                .ToList();

            foreach (var item in itensParaRemover)
            {
                await _jsRuntime.InvokeVoidAsync("indexedDB.deleteItem", "usuarios", item.Refeicao);
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    // Obter a configuração
    public async Task<Configs> GetConfig()
    {
        return await _jsRuntime.InvokeAsync<Configs>("indexedDBHelper.getItem", "configs", "UNICO");
    }

    // Listar todos os usuários
    public async Task<List<BaseDeDadosMedicos>> ListarBaseDeDadosAsync()
    {
        return await _jsRuntime.InvokeAsync<List<BaseDeDadosMedicos>>("indexedDBHelper.getAllItems", "usuarios");
    }

    // Obter a última atualização
    public async Task<string> GetUltimaAtualizacaoDoGoogleSheet()
    {
        var listaComLinhas = await _jsRuntime.InvokeAsync<List<BaseDeDadosMedicos>>("indexedDB.getAllItems", "usuarios");
        var data = listaComLinhas.OrderByDescending(x => x.UltimaAtualizacaoServicoExterno)
                                 .FirstOrDefault();

        if (data != null)
        {
            bool dataIgual = listaComLinhas.All(item => item.UltimaAtualizacaoServicoExterno.Date == data.UltimaAtualizacaoServicoExterno.Date);
            if (dataIgual)
            {
                return data.UltimaAtualizacaoServicoExterno.ToString("dd-MM-yyyy HH:mm:ss");
            }
            else
            {
                return "Não foi possível verificar a última atualização. Necessário verificar planilha";
            }
        }

        return "Sem dados.";
    }
}
