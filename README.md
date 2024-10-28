# Embedeasy - Aplicação para Adicionar Legendas Internas em Vídeos

Este projeto é uma aplicação em **C# Windows Forms** que facilita a **incorporação de faixas de legendas internas** de vídeos no formato `.mkv`. Utilizando o **FFmpeg**, a aplicação oferece uma interface simples para escolher uma faixa de legenda embutida no vídeo e adicioná-la de forma prática como uma parte permanente do arquivo.

---

## Estrutura do Projeto

- **Program.cs**: Responsável por inicializar e executar o formulário principal da aplicação.
- **MainForm.cs**: Implementa a interface gráfica e gerencia a lógica do programa, incluindo a seleção de arquivos, listagem das faixas de legendas e a execução do processo de incorporação.
- **Subtitles.cs**: Define a classe `Subtitles` para representar uma faixa de legenda com `Id` e `Title`.

---

## Funcionalidades

- **Seleção de Arquivo de Vídeo**: Permite escolher vídeos no formato `.mkv`.
- **Listagem Automática de Faixas de Legendas Internas**: Carrega as faixas de legendas embutidas no vídeo e as exibe para seleção.
- **Incorporação Simples**: Incorpora a legenda selecionada no vídeo utilizando o FFmpeg.
- **Barra de Progresso**: Monitora o andamento do processo de incorporação.
- **Compatibilidade**: Salva o vídeo final no formato `.mp4` com a legenda incorporada.

---

## Pré-requisitos

1. **FFmpeg**: Certifique-se de que o FFmpeg está instalado e configurado no PATH do sistema.
2. **.NET Framework** ou **.NET Core**: Necessário para compilar e executar o projeto.

---

## Como Usar

1. **Selecionar o Vídeo**: Clique no botão **"Escolha o arquivo"** e selecione um vídeo no formato `.mkv`.
2. **Carregar Faixas de Legendas**: A aplicação exibirá as faixas de legenda internas disponíveis.
3. **Escolher Legenda**: Selecione a faixa desejada no **ComboBox**.
4. **Adicionar Legenda**: Clique em **"Adicionar Legenda"** para iniciar o processo.
5. **Acompanhar o Progresso**: A barra de progresso mostrará o status da incorporação.
6. **Vídeo Final**: O vídeo com a legenda incorporada será salvo com o sufixo `_Legendado.mp4` no mesmo diretório.

---

## Exemplo de Uso do Código

Aqui está uma amostra de como a função **EmbedySubtitle** utiliza o FFmpeg para incorporar a faixa de legenda selecionada:

```csharp
private async void EmbedySubtitle(Subtitles sub, string filePath)
{
    await Task.Run(() =>
    {
        Process cmd = new Process();
        cmd.StartInfo.FileName = "ffmpeg";
        cmd.StartInfo.ArgumentList.Add("-i");
        cmd.StartInfo.ArgumentList.Add(filePath);
        cmd.StartInfo.ArgumentList.Add("-vf");
        cmd.StartInfo.ArgumentList.Add($"subtitles='{filePath}':si={sub.Id},eq=saturation=0.8");
        cmd.StartInfo.ArgumentList.Add("-c:v");
        cmd.StartInfo.ArgumentList.Add("hevc_amf");
        cmd.StartInfo.ArgumentList.Add("-quality");
        cmd.StartInfo.ArgumentList.Add("balanced");
        cmd.StartInfo.ArgumentList.Add("-c:a");
        cmd.StartInfo.ArgumentList.Add("copy");
        cmd.StartInfo.ArgumentList.Add(Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + "_Legendado.mp4"));
        cmd.Start();
        cmd.WaitForExit();
    });
}
```

---

## Interface Gráfica (GUI)

- **TextBox**: Exibe o caminho do vídeo selecionado.
- **ComboBox**: Lista as faixas de legenda internas do vídeo.
- **Botões**:
  - **Escolha o arquivo**: Abre o diálogo para selecionar o vídeo.
  - **Adicionar Legenda**: Inicia a incorporação da faixa de legenda selecionada.
- **ProgressBar**: Indica o progresso do processo de incorporação.

---

## Contribuição

Sinta-se à vontade para contribuir com melhorias ou novas funcionalidades. Abra uma **issue** ou crie um **pull request** para discutir suas ideias.

---

## Licença

Este projeto é distribuído sob a licença MIT. Consulte o arquivo `LICENSE` para mais informações.