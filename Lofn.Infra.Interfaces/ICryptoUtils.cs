using System;
using System.Threading.Tasks;

namespace Lofn.Infra.Interfaces
{
    public interface ICryptoUtils
    {
        bool CheckPersonalSignature(string phrase, string signature, string userAddress);
        Task<string> TesteConnection(string contractAddress);
    }
}
