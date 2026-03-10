using NSales.Domain.Interfaces.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSales.Domain.Interfaces.Factory
{
    public interface IOrderDomainFactory
    {
        IOrderModel BuildOrderModel();
    }
}
