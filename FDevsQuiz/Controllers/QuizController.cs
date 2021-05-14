using FDevsQuiz.Command;
using FDevsQuiz.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FDevsQuiz.Controllers
{
    [ApiController]
    [Route("quizzes")]
    public class QuizController : ControllerBase
    {
        
        private ICollection<T> CarregarDados<T>(string arquivo)
        {
            using var openStream = System.IO.File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", $"{arquivo}.json"));
            return JsonSerializer.DeserializeAsync<ICollection<T>>(openStream, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }).Result;
        }

        private async Task SalvarDados<T>(ICollection<T> dados, string arquivo)
        {
            using var openStream = System.IO.File.Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", $"{arquivo}.json"));
            await JsonSerializer.SerializeAsync(openStream, dados, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        //Quiz - Base
        [HttpGet]
        public ActionResult<ICollection<Quiz>> Quizzes()
        {
            var quizzes = CarregarDados<Quiz>("quizzes");
            return Ok(quizzes);
        }

        [HttpGet("{id}")]
        public ActionResult<Quiz> Quiz([FromRoute] long id)
        {
            var quizzes = CarregarDados<Quiz>("quizzes");

            var quiz = quizzes.Where(q => q.Codigo == id).FirstOrDefault();

            return Ok(quiz);
        }

        [HttpPost("{id}")]
        public async Task<ActionResult<Quiz>> AdicionarQuiz([FromBody] QuizCommand command)
        {
            if (command == null)
                return BadRequest("Mal formatado");

            if (string.IsNullOrEmpty(command.Titulo))
                throw new Exception("Nome do quiz é obrigatório.");

            var quizzes = CarregarDados<Quiz>("quizzes");
            var quiz = new Quiz()
            {
                Titulo = command.Titulo,
                Nivel = command.Nivel,
                Respostas = 0,
                ImagemUrl = command.ImagemUrl
            };

            quiz.Codigo = quizzes.Select(q => q.Codigo).ToList().Max() + 1;
            quizzes.Add(quiz);

            return Created("quizzes/{id}", quiz);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarQuiz([FromRoute] long id, [FromBody] QuizCommand command)
        {
            if (command == null)
                return BadRequest("Mal formatado");

            if (string.IsNullOrEmpty(command.Titulo))
                throw new Exception("Título do quiz obrigatório.");

            var quizzes = CarregarDados<Quiz>("quizzes");
            var quiz = quizzes.Where(q => q.Codigo == id).FirstOrDefault();

            if (quiz == null)
                return NotFound("Não encontrado");

            quiz.Titulo = command.Titulo;
            quiz.Nivel = command.Nivel;
            quiz.Respostas = command.Respostas;
            quiz.ImagemUrl = command.ImagemUrl;

            await SalvarDados(quizzes, "quizzes");

            return NoContent();
         }

        [HttpDelete("{id}")]
        public async Task<IActionResult> ExcluirQuiz([FromRoute] long id)
        {
            var quizzes = CarregarDados<Quiz>("quizzes");
            quizzes = quizzes.Where(u => u.Codigo != id).ToList();

            await SalvarDados(quizzes, "quizzes");

            return NoContent();
        }

        // Perguntas

        [HttpGet("perguntas")]
        public ActionResult<ICollection<Pergunta>> Perguntas()
        {
            var perguntas = CarregarDados<Pergunta>("perguntas");
            return Ok(perguntas);
        }

        [HttpGet("perguntas/{id}")]
        public ActionResult<Quiz> Pergunta([FromRoute] long id)
        {
            var perguntas = CarregarDados<Pergunta>("perguntas");

            var pergunta = perguntas.Where(p => p.CodigoPergunta == id).FirstOrDefault();

            return Ok(pergunta);
        }

        [HttpPost("perguntas/{id}")]
        public async Task<ActionResult<Quiz>> AdicionarPergunta([FromBody] PerguntaCommand command)
        {
            if (command == null)
                return BadRequest("Mal formatado");

            if (string.IsNullOrEmpty(command.TextoPergunta))
                throw new Exception("A questão é obrigatória.");

            var perguntas = CarregarDados<Pergunta>("perguntas");
            var pergunta = new Pergunta()
            {
                TextoPergunta = command.TextoPergunta

            };

            pergunta.CodigoPergunta = perguntas.Select(p => p.Codigo).ToList().Max() + 1;
            perguntas.Add(pergunta);

            using var createStream = System.IO.File.Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "quizzes.json"));
            await JsonSerializer.SerializeAsync(createStream, perguntas, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return Created("perguntas/{id}", pergunta);
        }

        [HttpPut("perguntas/{id}")]
        public async Task<IActionResult> AtualizarPergunta([FromRoute] long id, [FromBody] PerguntaCommand command)
        {
            if (command == null)
                return BadRequest("Mal formatado");

            if (string.IsNullOrEmpty(command.TextoPergunta))
                throw new Exception("A questão é obrigatória.");

            var perguntas = CarregarDados<Pergunta>("perguntas");
            var pergunta = perguntas.Where(p => p.CodigoPergunta == id).FirstOrDefault();

            if (pergunta == null)
                return NotFound("Não encontrado");

            pergunta.TextoPergunta = command.TextoPergunta;
            await SalvarDados(perguntas, "perguntas");

            return NoContent();
        }

        [HttpDelete("perguntas/{id}")]
        public async Task<IActionResult> ExcluirPergunta([FromRoute] long id)
        {
            var perguntas = CarregarDados<Pergunta>("perguntas");
            perguntas = perguntas.Where(p => p.CodigoPergunta != id).ToList();

            await SalvarDados(perguntas, "perguntas");

            return NoContent();
        }

        // Alternativas

        [HttpGet("alternativas")]
        public ActionResult<ICollection<Alternativa>> Alternativas()
        {
            var alternativas = CarregarDados<Alternativa>("alternativas");
            return Ok(alternativas);
        }

        [HttpGet("alternativas/{id}")]
        public ActionResult<Quiz> Alternativa([FromRoute] long id)
        {
            var alternativas = CarregarDados<Alternativa>("alternativas");

            var alternativa = alternativas.Where(a => a.CodigoAlternativa == id).FirstOrDefault();

            return Ok(alternativa);
        }

        [HttpPost("alternativas/{id}")]
        public async Task<ActionResult<Quiz>> AdicionarAlternativa([FromBody] AlternativaCommand command)
        {
            if (command == null)
                return BadRequest("Mal formatado");

            if (string.IsNullOrEmpty(command.TextoAlternativa))
                throw new Exception("O texto da alternativa é obrigatório.");

            var alternativas = CarregarDados<Alternativa>("alternativas");
            var alternativa = new Alternativa()
            {
                TextoAlternativa = command.TextoAlternativa,
                Certa = command.Certa

            };

            alternativa.CodigoAlternativa = alternativas.Select(p => p.Codigo).ToList().Max() + 1;
            alternativas.Add(alternativa);

            using var createStream = System.IO.File.Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "quizzes.json"));
            await JsonSerializer.SerializeAsync(createStream, alternativas, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return Created("alternativas/{id}", alternativa);
        }

        [HttpPut("alternativas/{id}")]
        public async Task<IActionResult> AtualizarAlternativa([FromRoute] long id, [FromBody] AlternativaCommand command)
        {
            if (command == null)
                return BadRequest("Mal formatado");

            if (string.IsNullOrEmpty(command.TextoAlternativa))
                throw new Exception("O texto da alternativa é obrigatório.");

            var alternativas = CarregarDados<Alternativa>("alternativas");
            var alternativa = alternativas.Where(a => a.CodigoAlternativa == id).FirstOrDefault();

            if (alternativa == null)
                return NotFound("Não encontrado");

            alternativa.TextoAlternativa = command.TextoAlternativa;
            alternativa.Certa = command.Certa;

            await SalvarDados(alternativas, "alternativas");

            return NoContent();
        }

        [HttpDelete("alternativas/{id}")]
        public async Task<IActionResult> ExcluirAlternativa([FromRoute] long id)
        {
            var alternativas = CarregarDados<Alternativa>("alternativas");
            alternativas = alternativas.Where(p => p.CodigoAlternativa != id).ToList();

            await SalvarDados(alternativas, "alternativas");

            return NoContent();
        }
    }
}
