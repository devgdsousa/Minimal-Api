using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.Entidades;

namespace MinimalApi.Infraestrutura.Db;

public class DbContexto : DbContext
{
    private readonly IConfiguration _configuracaoAppSettings;    
    public DbContexto(IConfiguration configuracaoAppSettings){
        _configuracaoAppSettings = configuracaoAppSettings;
    }
    public DbSet<Adm> Administradores {get;set;} = default!;
    public DbSet<Veiculo> Veiculos {get;set;} = default!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Adm>().HasData(
            new Adm{
                 Id = 1,
                 Email = "adm@teste.com",
                 Senha = "12345",
                 Perfil = "Adm"
            }
        ) ;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(!optionsBuilder.IsConfigured){
            var stringConexao = _configuracaoAppSettings.GetConnectionString("MySql")?.ToString();
                if(!string.IsNullOrEmpty(stringConexao)){
                    optionsBuilder.UseMySql(stringConexao, 
                    ServerVersion.AutoDetect(stringConexao));
                }
        }
    }
}

