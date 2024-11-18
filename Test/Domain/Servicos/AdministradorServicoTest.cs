using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using minimalApi.Dominio.Servicos;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Infraestrutura.Db;

namespace Test.Domain.Servicos;

[TestClass]
public class AdministradorServicoTest
{


  private DbContexto CriarContextoDeTeste()
{   
    //Forçando a realizar teste atraves do appsettings.json da raiz do projeto Test
    var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    var path = Path.GetFullPath(Path.Combine(assemblyPath ?? "","..","..",".."));
    
    var builder = new ConfigurationBuilder()
        .SetBasePath(path?? Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();

    var configuration = builder.Build();

    return new DbContexto(configuration);
}


    [TestMethod]
    public void TestandoSalvarAdministrador()
    {
        //Arrange
        var context = CriarContextoDeTeste();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");
        var adm = new Adm();
        adm.Email = "teste@teste.com";
        adm.Senha = "teste";
        adm.Perfil = "Adm";
        adm.Id = 1;
        var administradorServico = new AdministradorServico(context);
        
        //Act
        administradorServico.Incluir(adm);
       
        
        //Assert
        Assert.AreEqual(1,adm.Id = 1);
    
    }

      [TestMethod]
    public void TestBuscarPorId()
    {
        //Arrange
        var context = CriarContextoDeTeste();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");
        var adm = new Adm();
        adm.Email = "teste@teste.com";
        adm.Senha = "teste";
        adm.Perfil = "Adm";
        adm.Id = 1;
        var administradorServico = new AdministradorServico(context);
        
        //Act
        administradorServico.Incluir(adm);
        var admDoBanco = administradorServico.BuscarPorID(adm.Id);
       
        
        //Assert
        Assert.AreEqual(1, admDoBanco.Id);
    
    }
}

// run -> dotnet test