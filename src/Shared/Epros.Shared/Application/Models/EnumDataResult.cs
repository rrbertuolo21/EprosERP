using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Epros.Shared.Application.Models
{
    /// <summary>
    /// Item de resultado padrão de um membro de enum para dropdowns do frontend.
    /// Porte fiel do legado <c>EnumDataResult</c> (Id = valor numérico, Descricao = nome do membro,
    /// DescricaoFormatada = atributo [Description] quando presente).
    /// </summary>
    public class EnumDataResult
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string DescricaoFormatada { get; set; } = string.Empty;

        public EnumDataResult() { }

        public EnumDataResult(int id, string descricao, string descricaoFormatada)
        {
            Id = id;
            Descricao = descricao;
            DescricaoFormatada = descricaoFormatada;
        }
    }

    /// <summary>
    /// Helper de reflexão para expor membros de enums (valor + Description) de forma genérica.
    /// Substitui os ~15 <c>*EnumsController</c> do legado por 1 controller genérico.
    /// </summary>
    public static class EnumReflectionHelper
    {
        /// <summary>Obtém os membros de um enum tipado (valor, nome e Description).</summary>
        public static IEnumerable<EnumDataResult> ObterMembros<T>() where T : struct, Enum
            => ObterMembros(typeof(T));

        /// <summary>Obtém os membros de um enum via <see cref="Type"/> (para uso por reflexão/domínio).</summary>
        public static IEnumerable<EnumDataResult> ObterMembros(Type enumType)
        {
            if (enumType == null) throw new ArgumentNullException(nameof(enumType));
            if (!enumType.IsEnum) throw new ArgumentException($"O tipo '{enumType.Name}' não é um enum.", nameof(enumType));

            var resultado = new List<EnumDataResult>();
            foreach (var valor in Enum.GetValues(enumType))
            {
                var nome = valor.ToString() ?? string.Empty;
                var numerico = Convert.ToInt32(valor);
                resultado.Add(new EnumDataResult(numerico, nome, ObterDescription(enumType, nome)));
            }
            return resultado;
        }

        private static string ObterDescription(Type enumType, string nome)
        {
            var field = enumType.GetField(nome);
            if (field == null) return nome;
            var attr = field.GetCustomAttribute<DescriptionAttribute>();
            return attr == null ? nome : attr.Description;
        }
    }
}
