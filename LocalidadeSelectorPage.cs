using System.Globalization;

namespace DemografiaLocalApp;

// Seletor pesquisável compartilhado por estados e municípios.
public sealed class LocalidadeSelectorPage<T> : ContentPage where T : class
{
    private sealed record Opcao(T Item, string Texto);

    public LocalidadeSelectorPage(string titulo, string dica, List<T> itens,
        Func<T, string> descricao, Action<T> selecionar)
    {
        Title = titulo;
        this.SetAppThemeColor(BackgroundColorProperty, Color.FromArgb("#F4F6FA"), Color.FromArgb("#10151E"));
        var busca = new SearchBar { Placeholder = dica, Margin = new Thickness(12, 4) };
        var quantidade = new Label { Margin = new Thickness(20, 8), FontSize = 13 };
        var lista = new CollectionView
        {
            SelectionMode = SelectionMode.Single,
            VerticalScrollBarVisibility = ScrollBarVisibility.Always,
            EmptyView = "Nenhuma localidade encontrada. Revise a busca.",
            ItemTemplate = new DataTemplate(() =>
            {
                var rotulo = new Label { FontSize = 17, Padding = new Thickness(20, 16) };
                rotulo.SetBinding(Label.TextProperty, nameof(Opcao.Texto));
                return rotulo;
            })
        };
        void Filtrar()
        {
            var termo = busca.Text?.Trim() ?? "";
            var opcoes = itens.Where(item => CultureInfo.GetCultureInfo("pt-BR").CompareInfo
                .IndexOf(descricao(item), termo, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0)
                .Select(item => new Opcao(item, descricao(item))).ToList();
            lista.ItemsSource = opcoes;
            quantidade.Text = $"{opcoes.Count} localidades · toque para selecionar ou role a lista";
        }
        busca.TextChanged += (_, _) => Filtrar();
        var fechando = false;
        lista.SelectionChanged += async (_, args) =>
        {
            if (fechando || args.CurrentSelection.FirstOrDefault() is not Opcao opcao) return;
            fechando = true;
            busca.Unfocus();
            await Navigation.PopModalAsync();
            selecionar(opcao.Item);
        };
        var fechar = new ToolbarItem { Text = "Cancelar" };
        fechar.Clicked += async (_, _) =>
        {
            if (fechando) return;
            fechando = true;
            await Navigation.PopModalAsync();
        };
        ToolbarItems.Add(fechar);
        var grade = new Grid { RowDefinitions = { new RowDefinition(GridLength.Auto),
            new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) } };
        grade.Add(busca); grade.Add(quantidade, 0, 1); grade.Add(lista, 0, 2);
        Content = grade;
        Filtrar();
    }
}
