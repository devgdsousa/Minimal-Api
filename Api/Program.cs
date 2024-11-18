using Microsoft.EntityFrameworkCore;
using MinimalApi.Infraestrutura.Db;
using MinimalApi.DTOs;
using minimalApi.Dominio.Interfaces;
using minimalApi.Dominio.Servicos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using minimal_api.Dominio.ModelViews;
using MinimalApi.Dominio.Entidades;
using minimalApi.Dominio.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.OpenApi.Models;
using System.Data;


#region Builder
var builder = WebApplication.CreateBuilder(args);


var key = builder.Configuration.GetSection("Jwt").ToString();
if(string.IsNullOrEmpty(key)) key = "12345";
builder.Services.AddAuthentication(option=>{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option=>{
   option.TokenValidationParameters = new TokenValidationParameters{
     ValidateLifetime = true,
     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
     ValidateIssuer = false,
     ValidateAudience = false
   };
});
builder.Services.AddAuthorization();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o Token JWT aqui"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});



builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();
builder.Services.AddDbContext<DbContexto>(options =>{
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql"))
    );
});

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
#endregion

#region Adms

string GerarTokenJwt(Adm administrador){
  if(string.IsNullOrEmpty(key)) return string.Empty;
  var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
  var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
  
  var claims = new List<Claim>(){
    new Claim("Email", administrador.Email),
    new Claim("Perfil", administrador.Perfil),
    new Claim(ClaimTypes.Role, administrador.Perfil)
  };
  var token = new JwtSecurityToken(
    claims: claims,
    expires : DateTime.Now.AddDays(1),
    signingCredentials : credentials
  );
  return new JwtSecurityTokenHandler().WriteToken(token);
}


app.MapPost("/administradores/login", ([FromBody]LoginDTO loginDTO, IAdministradorServico administradorServico )=>{
    var admin = administradorServico.Login(loginDTO);
    if(admin!= null){
        string token = GerarTokenJwt(admin);
        return Results.Ok(new AdministradorLogado{
          Email = admin.Email,
          Perfil = admin.Perfil,
          Token = token
        });
    }else{
        return Results.Unauthorized();
    }
}).AllowAnonymous().WithTags("Adms");


app.MapGet("/administradores", ([FromQuery]int? pagina, IAdministradorServico administradorServico )=>{
       var adms = new List<AdministradorModelViews>();
       var administradores = administradorServico.Todos(pagina);
       foreach(var adm in administradores){
         adms.Add(new AdministradorModelViews{
          Id = adm.Id,
          Email = adm.Email,
          Perfil = adm.Perfil
         });
       }
       return Results.Ok(adms);
}).RequireAuthorization().
RequireAuthorization(new AuthorizeAttribute{Roles = "Adm"}).WithTags("Adms");


app.MapGet("/administradores/{id}", ([FromRoute]int id, IAdministradorServico administradorServico)=>{
    var administrador = administradorServico.BuscarPorID(id);
    if(administrador ==null) return Results.NotFound();
    
    return Results.Ok(new AdministradorModelViews{
          Id = administrador.Id,
          Email = administrador.Email,
          Perfil = administrador.Perfil
         });
    }).RequireAuthorization().
    RequireAuthorization(new AuthorizeAttribute{Roles = "Adm"}).WithTags("Adms").WithTags("Adms");


app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico )=>{
   var validacao = new ErrosValidacao{
        Mensagens = new List<string>()
    };
    if(string.IsNullOrEmpty(administradorDTO.Email))
      validacao.Mensagens.Add("Email não pode ser vazio");
    if(string.IsNullOrEmpty(administradorDTO.Senha))
      validacao.Mensagens.Add("Senha não pode ser vazia");
    if(administradorDTO.Perfil == null)
    validacao.Mensagens.Add("Perfil não pode ser vazio");
    if(validacao.Mensagens.Count>0)
       return Results.BadRequest(validacao);   
    
    var administrador = new Adm{
       Email = administradorDTO.Email,
       Senha = administradorDTO.Senha,
       Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
    };
    administradorServico.Incluir(administrador);
    return Results.Created($"/administrador/{administrador.Id}", new AdministradorModelViews{
          Id = administrador.Id,
          Email = administrador.Email,
          Perfil = administrador.Perfil
         });
}).RequireAuthorization().
RequireAuthorization(new AuthorizeAttribute{Roles = "Adm"}).WithTags("Adms").WithTags("Adms");
#endregion

#region Veiculos
ErrosValidacao validaDTO(VeiculoDTO veiculoDTO){
  var validacao = new ErrosValidacao{
    Mensagens = new List<string>()
  };
    if(string.IsNullOrEmpty(veiculoDTO.Nome))
      validacao.Mensagens.Add("O nome não pode ser vazio");

    if(string.IsNullOrEmpty(veiculoDTO.Marca))
      validacao.Mensagens.Add("A Marca não pode ficar em branco");
    
    if(veiculoDTO.Ano< 1950)
      validacao.Mensagens.Add("Veículo muito antigo, aceito apenas anos acima de 1950");

    return validacao;
    
}
app.MapPost("/veiculos", ([FromBody]VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico)=>{
    var validacao = validaDTO(veiculoDTO);
    if(validacao.Mensagens.Count>0)
       return Results.BadRequest(validacao);
    
    var veiculo = new Veiculo{
       Nome = veiculoDTO.Nome,
       Marca = veiculoDTO.Marca,
       Ano = veiculoDTO.Ano
    };
    veiculoServico.Incluir(veiculo);
    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
    }).RequireAuthorization().
    RequireAuthorization(new AuthorizeAttribute{Roles = "Adm, Editor"}).WithTags("Adms").WithTags("Veiculo");


app.MapGet("/veiculos", ([FromQuery]int? pagina, IVeiculoServico veiculoServico)=>{
    var veiculos = veiculoServico.Todos(pagina);
    
    return Results.Ok(veiculos);
    }).RequireAuthorization().WithTags("Veiculo");



app.MapGet("/veiculos/{id}", ([FromRoute]int id, IVeiculoServico veiculoServico)=>{
    var veiculo = veiculoServico.BuscarPorID(id);
    if(veiculo ==null) return Results.NotFound();
    
    return Results.Ok(veiculo);
    }).RequireAuthorization().
    RequireAuthorization(new AuthorizeAttribute{Roles = "Adm,Editor"}).WithTags("Adms").WithTags("Veiculo");



app.MapPut("/veiculos/{id}", ([FromRoute]int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico)=>{
    var veiculo = veiculoServico.BuscarPorID(id);
    if(veiculo ==null) return Results.NotFound();
   
    var validacao = validaDTO(veiculoDTO);
    if(validacao.Mensagens.Count>0)
       return Results.BadRequest(validacao);
    
    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;
    veiculoServico.atualizar(veiculo);

    return Results.Ok(veiculo);
    }).RequireAuthorization().
    RequireAuthorization(new AuthorizeAttribute{Roles = "Adm"}).WithTags("Adms").WithTags("Veiculo");



app.MapDelete("/veiculos/{id}", ([FromRoute]int id,IVeiculoServico veiculoServico)=>{
    var veiculo = veiculoServico.BuscarPorID(id);
    if(veiculo ==null) return Results.NotFound();
    
    veiculoServico.Apagar(veiculo);
    
    return Results.NoContent();
    }).RequireAuthorization().
    RequireAuthorization(new AuthorizeAttribute{Roles = "Adm"}).WithTags("Adms").WithTags("Veiculo");
#endregion

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.Run();
