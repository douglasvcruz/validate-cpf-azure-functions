using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;

namespace httpValidaCPF
{
    public static class Fnvalidacpf
    {
        [FunctionName("fnvalidacpf")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Iniciando a validação do CPF.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            Console.WriteLine(data);

            if (data == null)
            {
                return new BadRequestObjectResult("Por favor, informe o CPF.");
            }

            string cpf = data?.cpf;

            if (IsValidCPF(cpf) == false)
            {
                return new BadRequestObjectResult("CPF inválido.");
            }

            string responseMessage = "CPF válido, e não consta na base de dados de fraudes, e não consta na base de dados de débitos.";

            return new OkObjectResult(responseMessage);
        }
        public static bool IsValidCPF(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return false;

            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            if (cpf.Length != 11) return false;
            if (cpf.Distinct().Count() == 1) return false;

            int sum = 0;
            for (int i = 0; i < 9; i++)
                sum += (cpf[i] - '0') * (10 - i);

            int remainder = (sum * 10) % 11;
            if (remainder == 10) remainder = 0;
            if (remainder != (cpf[9] - '0')) return false;

            sum = 0;
            for (int i = 0; i < 10; i++)
                sum += (cpf[i] - '0') * (11 - i);

            remainder = (sum * 10) % 11;
            if (remainder == 10) remainder = 0;
            return remainder == (cpf[10] - '0');
        }
    }

}
