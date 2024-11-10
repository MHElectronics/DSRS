using BOL;
using BOL.CustomModels;
using Services.Helpers;
using System.Data.SqlClient;

namespace Services;

public interface IAxleLoadService
{
    Task<IEnumerable<LoadData>> Get(LoadData obj);
    Task<(bool, string)> Add(LoadData obj);
    Task<(bool,string)> Add(List<LoadData> obj);
    Task<bool> Delete(UploadedFile file);

    Task<IEnumerable<AxleLoadCount>> GetDateWiseCount(Station station, DateTime startDate, DateTime endDate);
    Task<IEnumerable<AxleLoadReport>> GetDateWise (List<Station> stations, DateTime startDate, DateTime endDate);
}

public class AxleLoadService(ISqlDataAccess _db) : IAxleLoadService
{
    public async Task<IEnumerable<LoadData>> Get(LoadData obj)
    {
        string query = @"SELECT TransactionNumber,LaneNumber,DateTime 
      ,PlateZone,PlateSeries,PlateNumber,NumberOfAxle,VehicleSpeed
      ,Axle1,Axle2,Axle3,Axle4,Axle5,Axle6,Axle7 
      ,AxleRemaining,GrossVehicleWeight,IsUnloaded,IsOverloaded 
      ,OverSizedModified,Wheelbase,ClassStatus,RecognizedBy,IsBRTAInclude,LadenWeight,UnladenWeight,ReceiptNumber,BillNumber
      ,Axle1Time,Axle2Time,Axle3Time,Axle4Time,Axle5Time,Axle6Time,Axle7Time
       FROM AxleLoad WHERE StationId=@StationId AND DATEDIFF(DAY,DateTime,@DateTime)=0";

        return await _db.LoadData<LoadData, object>(query, obj);
    }
    
    public async Task<(bool, string)> Add(LoadData obj)
    {
        bool isSuccess = false;
        string message = "";
        string query = @"INSERT INTO AxleLoad(StationId,TransactionNumber,LaneNumber,DateTime,PlateZone,PlateSeries,PlateNumber,VehicleId,NumberOfAxle,VehicleSpeed
            ,Axle1,Axle2,Axle3,Axle4,Axle5,Axle6,Axle7
            ,AxleRemaining,GrossVehicleWeight,IsUnloaded,IsOverloaded,OverSizedModified,Wheelbase,ClassStatus,RecognizedBy,IsBRTAInclude,LadenWeight,UnladenWeight,ReceiptNumber,BillNumber
            ,Axle1Time,Axle2Time,Axle3Time,Axle4Time,Axle5Time,Axle6Time,Axle7Time)

            VALUES(@StationId,@TransactionNumber,@LaneNumber,@DateTime,@PlateZone,@PlateSeries,@PlateNumber,@VehicleId,@NumberOfAxle,@VehicleSpeed
            ,@Axle1,@Axle2,@Axle3,@Axle4,@Axle5,@Axle6,@Axle7
            ,@AxleRemaining,@GrossVehicleWeight,@IsUnloaded,@IsOverloaded,@OverSizedModified,@Wheelbase,@ClassStatus,@RecognizedBy,@IsBRTAInclude,@LadenWeight,@UnladenWeight,@ReceiptNumber,@BillNumber
            ,@Axle1Time,@Axle2Time,@Axle3Time,@Axle4Time,@Axle5Time,@Axle6Time,@Axle7Time)";
        try
        {
            isSuccess = await _db.SaveData(query, obj);
        }
        catch (SqlException ex)
        {
            //Duplicate error // Duplicate Unique error Key 2601 // Dublicate Primary error key 2627 
            if (ex.Number == 2601 || ex.Number == 2627)
            {
                isSuccess = false;
                message = "Duplicate data";
            }
        }
        catch (Exception ex)
        {
            isSuccess = false;
            message = "Error: " + ex.Message;
        }
        return (isSuccess, message);
    }
    public async Task<(bool, string)> Add(List<LoadData> obj)
    {
        bool isSuccess = false;
        string message = "";
        string query = @"INSERT INTO AxleLoad(StationId,TransactionNumber,LaneNumber,DateTime,PlateZone,PlateSeries,PlateNumber,VehicleId,NumberOfAxle,VehicleSpeed
            ,Axle1,Axle2,Axle3,Axle4,Axle5,Axle6,Axle7
            ,AxleRemaining,GrossVehicleWeight,IsUnloaded,IsOverloaded,OverSizedModified,Wheelbase,ClassStatus,RecognizedBy,IsBRTAInclude,LadenWeight,UnladenWeight,ReceiptNumber,BillNumber
            ,Axle1Time,Axle2Time,Axle3Time,Axle4Time,Axle5Time,Axle6Time,Axle7Time)

            VALUES(@StationId,@TransactionNumber,@LaneNumber,@DateTime,@PlateZone,@PlateSeries,@PlateNumber,@VehicleId,@NumberOfAxle,@VehicleSpeed
            ,@Axle1,@Axle2,@Axle3,@Axle4,@Axle5,@Axle6,@Axle7
            ,@AxleRemaining,@GrossVehicleWeight,@IsUnloaded,@IsOverloaded,@OverSizedModified,@Wheelbase,@ClassStatus,@RecognizedBy,@IsBRTAInclude,@LadenWeight,@UnladenWeight,@ReceiptNumber,@BillNumber
            ,@Axle1Time,@Axle2Time,@Axle3Time,@Axle4Time,@Axle5Time,@Axle6Time,@Axle7Time)";

        try
        {
            isSuccess = await _db.SaveData(query, obj);
        }
        catch (SqlException ex)
        {
            //Duplicate error // Duplicate Unique error Key 2601 // Dublicate Primary error key 2627
            if (ex.Number == 2601 || ex.Number == 2627)
            {
                isSuccess = false;
                message = "Duplicate data";
            }
        }
        catch (Exception ex)
        {
            isSuccess = false;
            message = "Error: " + ex.Message;
        }
        return (isSuccess, message);
    }

    public async Task<bool> Delete(UploadedFile file)
    {
        string query = @"DELETE FROM AxleLoadProcess WHERE FileId=@FileId 
            DELETE FROM AxleLoad WHERE StationId=@StationId AND DATEDIFF(DAY,DateTime,@Date)=0";

        return await _db.SaveData(query, new { FileId = file.Id, file.StationId, file.Date });
    }

    public async Task<IEnumerable<AxleLoadCount>> GetDateWiseCount(Station station, DateTime startDate, DateTime endDate)
    {
        string query = @"SELECT CONVERT(DATE,DateTime) Date,SUM(CONVERT(INT, IsOverloaded)) Overload,COUNT(1) TotalVehicle
            FROM AxleLoad
            WHERE StationId=@StationId
            AND DATEDIFF(Day,DateTime,@StartDate)<=0
            AND DATEDIFF(Day,DateTime,@EndDate)>=0
            GROUP BY CONVERT(DATE,DateTime)
            ORDER BY CONVERT(DATE,DateTime)";

        return await _db.LoadData<AxleLoadCount, object>(query, new { station.StationId, startDate, endDate });
    }
    public async Task<IEnumerable<AxleLoadReport>> GetDateWise(List<Station> stations, DateTime startDate, DateTime endDate)
    {
        string stationIds = string.Join(",", stations.Select(s => s.StationId));
        string query = @"
    SELECT 
        COUNT(1) AS Count,
        DATEPART(WEEKDAY, DateTime) AS Weekday,
        NumberofAxle,
        SUM(Axle1) AS Axle1,
        SUM(Axle2) AS Axle2,
        SUM(Axle3) AS Axle3,
        SUM(Axle4) AS Axle4,
        SUM(Axle5) AS Axle5,
        SUM(Axle6) AS Axle6,
        SUM(Axle7) AS Axle7,
        SUM(AxleRemaining) AS AxleRemaining,
        SUM(GrossVehicleWeight) AS GrossVehicleWeight
    FROM 
        AxleLoad AS AL
    WHERE 
        DateTime >= @DateStart 
        AND DateTime <= @DateEnd
        AND IsOverloaded = 1
        AND AL.StationId IN (" + stationIds + @")
        AND NumberOfAxle = (CASE WHEN @NumberOfAxle = 0 THEN NumberOfAxle ELSE @NumberOfAxle END)
        AND Wheelbase = (CASE WHEN @Wheelbase = 0 THEN Wheelbase ELSE @Wheelbase END)
        AND ClassStatus = (CASE WHEN @ClassStatus = 0 THEN ClassStatus ELSE @ClassStatus END)
        AND (@CheckWeightCalculation = 0 OR (Axle1 + Axle2 + Axle3 + Axle4 + Axle5 + Axle6 + Axle7 + AxleRemaining = GrossVehicleWeight))
    GROUP BY 
        DATEPART(WEEKDAY, DateTime),
        NumberOfAxle";

        return await _db.LoadData<AxleLoadReport, dynamic>(
            query,
            new
            {
                DateStart = startDate,
                DateEnd = endDate,
                NumberOfAxle = 0,
                Wheelbase = 0,
                ClassStatus = 0,
                CheckWeightCalculation = 1
            });
    }
}
