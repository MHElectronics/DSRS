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
    Task<IEnumerable<Document>> GetDocuments(bool onlyPublished = false);
    Task<IEnumerable<Document>> GetByUser(User user);
    Task<bool> InsertDocument(Document document);
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
    public async Task<IEnumerable<Document>> GetDocuments(bool onlyPublished = false)
    {
        string sql = "SELECT * FROM Document";
        Dictionary<string, object> param = new Dictionary<string, object>();

        if (onlyPublished)
        {
            sql += " WHERE IsPublished=@IsPublished";
            param.Add("@IsPublished", true);
        }

        return await _db.LoadData<Document, dynamic>(sql, param);
    }
    public async Task<IEnumerable<Document>> GetByUser(User user)
    {
        string sql = "SELECT * FROM Document WHERE UserId=@UserId";
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { "@UserId", user.Id }
        };

        return await _db.LoadData<Document, dynamic>(sql, param);
    }

    public async Task<bool> InsertDocument(Document document)
    {
        string sql = @"INSERT INTO Document(FileName,FileLocation,Description,UserId,IsPublished,Date)
            VALUES (@FileName,@FileLocation,@Description,@UserId,@IsPublished,@Date)";
        return await _db.SaveData<Document>(sql, document);
    }

    public async Task<bool> UpdateDocument(Document document)
    {
        string sql = @"UPDATE Document SET FileName=@FileName, FileLocation=@FileLocation, Description=@Description, UserId=@UserId, IsPublished=@IsPublished, Date=@Date
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
