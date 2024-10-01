using BOL;
using BOL.Models;
using Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services;
public interface IDocumentService
{
    Task<IEnumerable<Document>> GetDocuments();
    Task<IEnumerable<Document>> GetByUser(User user);
    Task<int> InsertDocument(Document document);
    Task<bool> UpdateDocument(Document document);
    Task<bool> DeleteDocument(Document document);
}
public class DocumentService : IDocumentService
{
    private readonly ISqlDataAccess _db;

    public DocumentService(ISqlDataAccess db)
    {
        _db = db;
    }
    public async Task<IEnumerable<Document>> GetDocuments() =>
        await _db.LoadData<Document, dynamic>("SELECT * FROM Document", new { });
    public async Task<IEnumerable<Document>> GetByUser(User user)
    {
        string sql = "SELECT * FROM Document WHERE UserId=@UserId";
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { "@UserId", user.Id }
        };

        return await _db.LoadData<Document, dynamic>(sql, param);
    }

    public async Task<int> InsertDocument(Document document)
    {
        string sql = @"INSERT INTO Document(FileName,FileLocation,Description,UserId,Date)
                        OUTPUT INSERTED.Id
                        VALUES (@FileName,@FileLocation,@Description,@UserId,@Date)";
        int documentId = await _db.ExecuteScalar<int>(sql, document);
        return documentId;
    }

    public async Task<bool> UpdateDocument(Document document)
    {
        string sql = @"UPDATE Document SET FileName=@FileName, FileLocation=@FileLocation, Description=@Description, UserId=@UserId, Date=@Date
                       WHERE Id=@Id";
        return await _db.SaveData<Document>(sql, document);
    }
    public async Task<bool> DeleteDocument(Document document)
    {
        string sql = "DELETE FROM Document WHERE Id=@Id";
        int count = await _db.DeleteData<Document, object>(sql, new { document.Id });
        return count > 0;
    }
}
