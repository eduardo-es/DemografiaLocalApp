# Nomes IBGE — Censo 2022

Aplicativo .NET MAUI para consultar os nomes mais comuns de homens e mulheres, além dos sobrenomes mais frequentes, em qualquer estado ou município do Brasil. Os dados vêm das APIs públicas do IBGE (Censo 2022).

## Integrantes do grupo

- **Eduardo Mendes Pereira**
- **Gabriel Tavares**

## Teste rápido: APK pronto

Para testar sem instalar o .NET ou o Android SDK, use o APK de depuração incluído neste repositório:

- **Arquivo:** [`apk/NomesIBGE-1.0-debug.apk`](apk/NomesIBGE-1.0-debug.apk)
- **Versão:** 1.0 (`versionCode` 1)
- **Pacote:** `com.companyname.demografialocalapp`
- **Tamanho:** aproximadamente 15 MB
- **Android mínimo:** 5.0 (API 21)
- **Arquiteturas:** `arm64-v8a` e `x86_64`
- **Assinatura:** Android Debug (somente para testes)

### Instalar no celular

1. Baixe o APK no celular ou transfira o arquivo para ele.
2. Abra o arquivo pelo gerenciador de arquivos ou pela pasta Downloads.
3. Se o Android bloquear a instalação, permita "instalar aplicativos desconhecidos" para o app usado para abrir o APK.
4. Toque em **Instalar** e depois em **Abrir**.
5. O aparelho precisa estar conectado à internet para consultar o IBGE.

O APK é uma build **Debug**, assinada com a chave de depuração do Android. Ele serve para testes e não deve ser publicado na Play Store.

### Instalar via ADB

Com o celular conectado por USB e a depuração USB habilitada:

```powershell
adb devices
adb install -r .\apk\NomesIBGE-1.0-debug.apk
```

Se houver mais de um dispositivo conectado, use `adb -s <serial> install -r ...`.

### Verificar o download

SHA-256 do APK incluído neste repositório:

```text
F1BD5DB86CD279AFB0486061F4E1E6B295DD5D8743DCF0CAF6E249EE4F757D09
```

No PowerShell:

```powershell
Get-FileHash .\apk\NomesIBGE-1.0-debug.apk -Algorithm SHA256
```

## Funcionalidades

- Consulta por **estado** ou por **município**.
- Carrega as 27 unidades federativas e todos os municípios da UF selecionada.
- Exibe listas separadas de nomes masculinos, nomes femininos e sobrenomes.
- Ordena cada lista pela frequência informada pelo IBGE.
- Consulta automaticamente ao trocar a localidade selecionada.
- Cancela requisições anteriores quando o usuário troca de estado ou município rapidamente.
- Mostra estados de carregamento, erro com botão **Tentar novamente** e aviso de "nenhum nome encontrado".
- Oculta a faixa de sobrenomes quando a API não retorna sobrenomes.
- Suporte a tema claro e escuro.
- Não exige chave, token ou cadastro: usa apenas as APIs públicas do IBGE.

## Como usar

1. Abra o aplicativo e aguarde o carregamento dos estados.
2. Escolha **Estado** ou **Município** no seletor superior.
3. No modo **Estado**, selecione uma UF para consultar os nomes de toda a unidade federativa.
4. No modo **Município**, selecione primeiro a UF e depois o município.
5. Role as listas de homens e mulheres e a faixa horizontal de sobrenomes.
6. Se ocorrer uma falha de conexão ou da API, toque em **Tentar novamente**.

## API do IBGE

O app usa os seguintes endpoints públicos:

| Uso | Método e endpoint |
| --- | --- |
| Lista de estados | `GET https://servicodados.ibge.gov.br/api/v1/localidades/estados` |
| Municípios de uma UF | `GET https://servicodados.ibge.gov.br/api/v1/localidades/estados/{id}/municipios` |
| Nomes por localidade | `GET https://servicodados.ibge.gov.br/api/v3/nomes/2022/localidade/{id}` |

O código da localidade é o código IBGE do estado (por exemplo, `35` para São Paulo) ou do município (por exemplo, `3550308` para a cidade de São Paulo).

A API é pública e pode ficar lenta ou indisponível ocasionalmente. O aplicativo não mantém cache nem modo offline; portanto, é necessária conexão com a internet.

## Estrutura do projeto

```text
DemografiaLocalApp/
├── Models/                         # DTOs do IBGE e conversores de JSON
├── Services/
│   ├── IIbgeApiService.cs          # Contrato da camada de acesso à API
│   ├── IbgeApiService.cs           # HttpClient, endpoints e tratamento de erros
│   └── IbgeApiException.cs         # Exceção com mensagem amigável
├── ViewModels/
│   └── MainViewModel.cs            # Estado da tela, comandos e concorrência
├── MainPage.xaml                   # Interface da tela principal
├── MainPage.xaml.cs                # Carga inicial
├── MauiProgram.cs                  # Injeção de dependências
├── AppShell.xaml                   # Shell sem menu lateral
├── Platforms/Android/              # Configuração específica do Android
├── apk/                            # APK Debug pronto para teste
└── DemografiaLocalApp.slnx         # Solução
```

O projeto segue uma separação simples em camadas:

- **Models:** classes que representam estados, municípios, regiões e contagens de nomes. Os nomes das propriedades JSON são mapeados com `[JsonPropertyName]`.
- **Services:** cliente HTTP assíncrono com `HttpClient`, `CancellationToken`, timeout e tradução de erros de rede/HTTP/JSON para `IbgeApiException`.
- **ViewModels:** `MainViewModel` usa CommunityToolkit.Mvvm, coleções observáveis, comandos assíncronos e cancelamento das consultas anteriores.
- **UI:** XAML com `Picker`, `CollectionView` e estados visuais de carregamento, erro, dica inicial e ausência de resultados.

## Compilar e executar

### Requisitos

- Windows 10/11 (o projeto também contém alvos para iOS/MacCatalyst, mas o foco testado é Android).
- .NET SDK 10.0 ou superior.
- Workload do .NET MAUI para Android.
- Android SDK e JDK instalados (o Visual Studio Installer pode instalar tudo).
- Emulador Android x86_64 ou celular Android com depuração USB.
- Internet para restaurar os pacotes NuGet e consultar o IBGE.

A forma mais simples no Windows é instalar o Visual Studio com a carga de trabalho **.NET Multi-platform App UI development** e marcar o componente **Android**.

### Instalar o workload pela linha de comando

```powershell
dotnet workload install maui-android
```

### Clonar

```powershell
git clone https://github.com/eduardo-es/DemografiaLocalApp.git
cd DemografiaLocalApp
```

### Compilar em Debug

```powershell
dotnet build .\DemografiaLocalApp.csproj -f net10.0-android -c Debug
```

O parâmetro `-f net10.0-android` é importante no Windows. Sem ele, o build pode tentar compilar também iOS e MacCatalyst e falhar por falta do toolchain correspondente.

O APK assinado é gerado em:

```text
bin/Debug/net10.0-android/com.companyname.demografialocalapp-Signed.apk
```

### Executar no emulador ou celular

Com um dispositivo Android disponível:

```powershell
dotnet build .\DemografiaLocalApp.csproj -t:Run -f net10.0-android -c Debug
```

Ou abra `DemografiaLocalApp.slnx` no Visual Studio, escolha um dispositivo Android e pressione **F5**.

### Gerar uma cópia do APK

Depois do build:

```powershell
Copy-Item .\bin\Debug\net10.0-android\com.companyname.demografialocalapp-Signed.apk .\apk\NomesIBGE-1.0-debug.apk
```

## Checklist de teste

- [ ] O app abre e carrega os 27 estados.
- [ ] No modo **Estado**, selecionar São Paulo exibe nomes masculinos, femininos e sobrenomes.
- [ ] No modo **Município**, selecionar São Paulo carrega a lista de municípios.
- [ ] Selecionar o município de São Paulo (`3550308`) exibe os nomes do município.
- [ ] Trocar rapidamente entre estados ou municípios não deixa resultados antigos na tela.
- [ ] Sem internet, aparece uma mensagem de erro e o botão **Tentar novamente** funciona.
- [ ] Uma localidade sem dados mostra o aviso de "nenhum nome encontrado".
- [ ] O tema escuro do Android é aplicado corretamente.
- [ ] A faixa de sobrenomes fica oculta quando a API não retorna sobrenomes.

## Solução de problemas

### `NETSDK1147` ou workload ausente

Instale o workload:

```powershell
dotnet workload install maui-android
```

Se estiver usando Visual Studio, abra o **Visual Studio Installer**, escolha **Modificar** e instale a carga de trabalho **.NET Multi-platform App UI development** com o componente Android.

### O build tenta iOS ou MacCatalyst

Use sempre o target Android:

```powershell
dotnet build .\DemografiaLocalApp.csproj -f net10.0-android -c Debug
```

### `adb` não é reconhecido

O `adb` fica normalmente em:

```text
%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe
```

Adicione essa pasta ao `PATH` ou use o terminal do Visual Studio, que costuma configurar o ambiente.

### O celular não aparece no `adb devices`

- Ative as **Opções do desenvolvedor** e a **Depuração USB**.
- Conecte o cabo e autorize a depuração no celular.
- Troque o modo USB para transferência de arquivos, se necessário.
- Execute `adb kill-server` e `adb start-server` e tente novamente.

### O APK não instala

- Confirme que o Android é 5.0 ou superior.
- O APK contém `arm64-v8a` e `x86_64`; aparelhos 32-bit antigos não são suportados.
- Permita a instalação de aplicativos desconhecidos para o app que está abrindo o arquivo.
- Se estiver atualizando uma instalação anterior, o APK usa a mesma assinatura Debug; em caso de conflito, desinstale a versão antiga e instale novamente.

### A consulta falha

- Confira a conexão com a internet.
- A API do IBGE pode estar temporariamente indisponível; use **Tentar novamente**.
- Em redes corporativas ou escolares, o acesso a `servicodados.ibge.gov.br` pode estar bloqueado.

### Build Release

A build Debug é a configuração validada para este projeto e é a usada no APK incluído. A build Release pode exigir configuração adicional de assinatura e do toolchain Android. Se ela falhar com `XAGNM7009`, tente:

```powershell
dotnet build .\DemografiaLocalApp.csproj -f net10.0-android -c Release -p:RuntimeIdentifier=android-arm64
```

## Limitações conhecidas

- O foco de teste é Android. Os alvos iOS e MacCatalyst existem no template, mas não foram validados neste ambiente.
- Não há armazenamento local, cache ou modo offline.
- A disponibilidade e a latência dependem da API pública do IBGE.
- O APK incluído é de depuração, não assinado para distribuição na Play Store.
- O identificador do aplicativo ainda é o genérico `com.companyname.demografialocalapp`; para publicar de verdade, troque por um identificador próprio.

## Dados

Fonte: **IBGE — Censo 2022**. Este projeto é independente e não possui vínculo oficial com o IBGE.
