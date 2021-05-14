using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FDevsQuiz.Command;
using FDevsQuiz.Model;
using Microsoft.AspNetCore.Mvc;

namespace FDevsQuiz.Controllers
{

    [Controller]
    [Route(("usuarios"))]
    public class UsuariosController : ControllerBase
    {

        private ICollection<T> CarregarDados<T>()
        {
            using var openStream = System.IO.File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "usuarios.json"));
            return JsonSerializer.DeserializeAsync<ICollection<T>>(openStream, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }).Result;
        }

        private async Task SalvarDados(ICollection<Usuario> dados)
        {
            using var openStream = System.IO.File.Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "usuarios.json"));
            await JsonSerializer.SerializeAsync(openStream, dados, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }


        [HttpGet]
        public ActionResult <ICollection<dynamic>> Usuarios()
        {
            var usuarios = CarregarDados<dynamic>();
            return Ok(usuarios);
        }

        [HttpGet("{id}")]
        public ActionResult<dynamic> Usuario([FromRoute] long id)
        {
            var usuarios = CarregarDados<Usuario>();

            var usuario = usuarios.Where(u => u.CodigoUsuario == id).FirstOrDefault();

            return Ok(usuario);
        }

        [HttpPost]

        public async Task<ActionResult<Usuario>> Adicionar([FromBody] UsuarioCommand command)
        {
            if (command == null)
                return BadRequest("Mal formatado");

            if (string.IsNullOrEmpty(command.NomeUsuario))
                throw new Exception("Nome do usuário obrigatório.");

            var usuarios = CarregarDados<Usuario>();
            var usuario = new Usuario()
            {
                NomeUsuario = command.NomeUsuario,
                Email = command.Email,
                Pontuacao = 0,
                ImagemUrl = command.ImagemUrl
            };

            usuario.CodigoUsuario = usuarios.Select(u => u.CodigoUsuario).ToList().Max() + 1;
            usuarios.Add(usuario);

            using var createStream = System.IO.File.Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "usuarios.json"));
            await JsonSerializer.SerializeAsync(createStream, usuarios, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return Created("usuarios/{id}", usuario);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar([FromRoute] long id, [FromBody] UsuarioCommand command)
        {
            if (command == null)
                return BadRequest("Mal formatado");

            if (string.IsNullOrEmpty(command.NomeUsuario))
                throw new Exception("Nome do usuário obrigatório.");

            var usuarios = CarregarDados<Usuario>();
            var usuario = usuarios.Where(u => u.CodigoUsuario == id).FirstOrDefault();

            if (usuario == null)
                return NotFound("Não encontrado");

            usuario.NomeUsuario = command.NomeUsuario;
            usuario.Email = command.Email;
            usuario.Pontuacao = command.Pontuacao;
            usuario.ImagemUrl = command.ImagemUrl;

            await SalvarDados(usuarios);

            return NoContent();
            
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Excluir([FromRoute] long id)
        {
            var usuarios = CarregarDados<Usuario>();
            usuarios = usuarios.Where(u => u.CodigoUsuario != id).ToList();

            await SalvarDados(usuarios);

            return NoContent();
        }
    }
}
