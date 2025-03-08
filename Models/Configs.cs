using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CentralBemEstarAssemblyIOS.Models
{
    public class Configs
    {
        [Key]
        //Esse model apenas pode ter uma entidade, logo sempre em todas as chamadas esse campo deve ser igual
        public String IdIdentificao { get; set; }
        public String LinkGoogleSheet { get; set; }

    }
}
