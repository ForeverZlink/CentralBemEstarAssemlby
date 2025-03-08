
using System.ComponentModel.DataAnnotations;

namespace CentralBemEstarAssemblyIOS.Models
{
    public class BaseDeDadosMedicos
    {
        [Key]
        public string Refeicao { get; set; }
        public Double Insulina { get; set; }
        public Double CHO { get; set; }
        public Double Meta { get; set; }
        public Double FS { get; set; }
        public DateTime UltimaAtualizacaoServicoExterno { get; set; }

    }
}
