using MinimalApi.Dominio.Entidades;
using MinimalApi.DTOs;

namespace minimalApi.Dominio.Interfaces;

public interface IAdministradorServico
{
    Adm? Login(LoginDTO loginDTO);
    List<Adm> Todos(int? pagina);
    Adm Incluir(Adm administrador);
    Adm? BuscarPorID(int id);
};
