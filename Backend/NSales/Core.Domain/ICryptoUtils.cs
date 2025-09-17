using System;
using System.Threading.Tasks;

namespace Core.Domain
{
    public interface ICryptoUtils
    {
        bool CheckPersonalSignature(string phrase, string signature, string userAddress);
        Task<string> TesteConnection(string contractAddress);
    }
}
