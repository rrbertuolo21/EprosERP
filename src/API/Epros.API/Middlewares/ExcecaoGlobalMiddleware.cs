using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Epros.API.Middlewares
{
    public class ExcecaoGlobalMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExcecaoGlobalMiddleware> _logger;

        public ExcecaoGlobalMiddleware(RequestDelegate next, ILogger<ExcecaoGlobalMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu uma exceção não tratada na requisição {Path}", context.Request.Path);
                await TratarExcecaoAsync(context, ex);
            }
        }

        private static Task TratarExcecaoAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/problem+json";

            if (exception is Epros.Shared.Domain.Exceptions.OperacaoBloqueadaModoDemoException demoEx)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                var details = new ProblemDetails
                {
                    Status = (int)HttpStatusCode.Forbidden,
                    Title = "Operação Bloqueada",
                    Detail = demoEx.Message,
                    Instance = context.Request.Path
                };
                details.Extensions.Add("traceId", context.TraceIdentifier);
                return context.Response.WriteAsync(JsonSerializer.Serialize(details));
            }

            // REG-017 — LOCK OTIMISTA: conflito de concorrência (xmin divergente) vira 409 Conflict, não 500.
            // Sinaliza ao cliente "a linha mudou desde a sua leitura; recarregue e tente de novo" (last-write-wins
            // proibido nas entidades-chave). Mapeado aqui, no ponto central, para todo SaveChanges da aplicação.
            if (exception is Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                var conflict = new ProblemDetails
                {
                    Status = (int)HttpStatusCode.Conflict,
                    Title = "Conflito de concorrência",
                    Detail = "O registro foi alterado por outra operação desde a sua leitura. " +
                             "Recarregue os dados e refaça a alteração.",
                    Instance = context.Request.Path
                };
                conflict.Extensions.Add("traceId", context.TraceIdentifier);
                return context.Response.WriteAsync(JsonSerializer.Serialize(conflict));
            }

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var problemDetails = new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "Erro interno do servidor",
                Detail = "Ocorreu um erro inesperado no processamento da sua solicitação.",
                Instance = context.Request.Path
            };

            // Propaga o traceId para fins de correlação (ex: Loki/Tempo)
            var traceId = context.TraceIdentifier;
            problemDetails.Extensions.Add("traceId", traceId);

            var json = JsonSerializer.Serialize(problemDetails);
            return context.Response.WriteAsync(json);
        }
    }
}
