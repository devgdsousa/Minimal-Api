using MinimalApi.Dominio.Entidades;

namespace Test.Domain.Entidades;

[TestClass]
public class AdministradorTest
{
    [TestMethod]
    public void TestarGetSetPropriedades()
    {
        //Arrange
        var adm = new Adm();

        //Act
        adm.Email = "teste@teste.com";
        adm.Senha = "teste";
        adm.Perfil = "Adm";
        adm.Id = 1;

        //Assert
        Assert.AreEqual(1,adm.Id);
        Assert.AreEqual("teste@teste.com",adm.Email);
        Assert.AreEqual("teste",adm.Senha);
        Assert.AreEqual("Adm",adm.Perfil);
    }
}

// run -> dotnet test