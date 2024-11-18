using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using minimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.Entidades;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;

namespace minimalApi.Dominio.Servicos;

public class AdministradorServico : IAdministradorServico
{
    private readonly DbContexto _contexto;

    public AdministradorServico(DbContexto contexto){
         _contexto = contexto;
    }


    public Adm Incluir(Adm administrador)
    {
        _contexto.Administradores.Add(administrador);
        _contexto.SaveChanges();
        return administrador;
    }

    public Adm? Login(LoginDTO loginDTO)
    {
       var administrador = _contexto.Administradores.Where(a => 
       a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
       return administrador;
    }

    public List<Adm> Todos(int? pagina)
    {
      var query = _contexto.Administradores.AsQueryable();
      
       int itensPorPagina = 10;
       if(pagina != null)
          query = query.Skip(((int)pagina -1)* itensPorPagina).Take(itensPorPagina);
       
       return query.ToList();
    }

      public Adm? BuscarPorID(int Id)
    {
        return _contexto.Administradores.Where(v=>  v.Id == Id).FirstOrDefault();
    }
}
