using minimalApi.Dominio.Enums;
namespace minimal_api.Dominio.ModelViews;

public record AdministradorModelViews
{
   public int Id {get; set;} = default!; 
   public string Email {get; set;} = default!; 

   public string Perfil {get; set;} = default!; 
}
