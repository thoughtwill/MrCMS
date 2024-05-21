using System.Collections.Generic;
using System.Threading.Tasks;
using MrCMS.Batching.Entities;
using MrCMS.Services.ImportExport.DTOs;

namespace MrCMS.Services.ImportExport
{
    public interface IImportWebpagesService
    {
        //void ImportDocumentsFromDTOs(IEnumerable<DocumentImportDTO> items);
        //Webpage ImportDocument(WebpageImportDTO dto);
        Task<Batch> CreateBatch(List<WebpageImportDTO> items, bool autoStart = true);
    }
}