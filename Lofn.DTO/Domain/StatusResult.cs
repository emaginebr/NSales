using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lofn.DTO.Domain
{
    public class StatusResult
    {
        public bool Sucesso { get; set; } = true;
        public string Mensagem { get; set; }
        public IEnumerable<string> Erros { get; set; }
    }
}
