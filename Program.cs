var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapPost("/login", (MinimalApi.DTOs.LoginDTO loginDTO)=>{
    if(loginDTO.Email == "adm@teste.com" && loginDTO.Senha =="12345"){
        return Results.Ok("Logado com sucesso!");
    }else{
        return Results.Unauthorized();
    }
});

app.Run();
