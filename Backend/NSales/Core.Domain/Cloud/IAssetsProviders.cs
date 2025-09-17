using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Cloud
{
    public interface IAssetsProviders
    {
        Bitmap GetBaseAssets(string baseFolder, string imgPath);
        Task<string> UploadFileToBlobAsync(string strFileName, byte[] fileData, string fileMimeType);
        Task DeleteFile(string strFileName);
    }
}
