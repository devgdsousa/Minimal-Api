using Microsoft.AspNetCore.Mvc.RazorPages;
using MinimalApi.Dominio.Entidades;
using MinimalApi.DTOs;

namespace minimalApi.Dominio.Interfaces;

public interface IVeiculoServico
{
    List<Veiculo> Todos(int? pagina, string? nome =null, string? marca = null);
    Veiculo? BuscarPorID(int Id);
    void Incluir(Veiculo veiculo);
    void atualizar(Veiculo veiculo);
    void Apagar(Veiculo veiculo);
}
