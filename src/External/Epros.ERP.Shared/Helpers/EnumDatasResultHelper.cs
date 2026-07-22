using System.ComponentModel;
using System.Reflection;

namespace Epros.ERP.Shared.Helpers
{
    public class EnumDatasResultHelper
    {
        public static IEnumerable<EnumDataResult> ObterEnumGenericoResultDto<T>() where T : struct, Enum
        {
            ICollection<EnumDataResult> dados = new List<EnumDataResult>();
            foreach (T e in Enum.GetValues<T>())
            {
                string descricao = GetEnumDescription(e);
                dados.Add(new EnumDataResult(e.GetHashCode(), e.ToString(), descricao));
            }
            return dados;
        }

        public static string GetEnumDescription<T>(T valor) where T : Enum
        {
            FieldInfo? field = valor.GetType().GetField(valor.ToString());
            DescriptionAttribute? attribute = (DescriptionAttribute?)field!.GetCustomAttribute(typeof(DescriptionAttribute));
            return attribute == null ? valor.ToString() : attribute.Description;
        }
    }

    public class EnumDataResult
    {
        public EnumDataResult(int id, string descricao, string descricaoFormatada = "")
        {
            Id = id;
            Descricao = descricao;
            DescricaoFormatada = string.IsNullOrEmpty(descricaoFormatada) ? descricao : descricaoFormatada;
        }

        public int Id { get; set; }
        public string Descricao { get; set; }
        public string DescricaoFormatada { get; set; }
    }
}
