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
    Task<IEnumerable<AxleLoadReport>> GetDateWise (Station station, DateTime startDate, DateTime endDate);
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
            //Duplicate error
            if (ex.Number == 2601)
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
            //Duplicate error
            if (ex.Number == 2601)
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
    public async Task<IEnumerable<AxleLoadReport>> GetDateWise(Station station, DateTime startDate, DateTime endDate)
    {
        string query = @"
    DECLARE @Stations TABLE(AutoId INT IDENTITY(1,1), StationId INT);
    DECLARE @DateStart DATE = @DateStartParam;
    DECLARE @DateEnd DATE = @DateEndParam;
    DECLARE @NumberOfAxle INT = @NumberOfAxleParam;
    DECLARE @Wheelbase INT = @WheelbaseParam;
    DECLARE @ClassStatus INT = @ClassStatusParam;
    DECLARE @CheckWeightCalculation BIT = @CheckWeightCalculationParam;

    -- Insert Station ID
    INSERT INTO @Stations(StationId) VALUES (@StationId);

    -- Define a CTE for all dates within the range
    ;WITH Dates AS (
        SELECT @DateStart AS Datetime
        UNION ALL
        SELECT DATEADD(DAY, 1, Datetime)
        FROM Dates
        WHERE DATEADD(DAY, 1, Datetime) <= @DateEnd
    )

    -- Main query with LEFT JOIN to ensure all dates and axle numbers are included
    SELECT 
        D.Datetime,
        ISNULL(AL.NumberOfAxle, @NumberOfAxle) AS NumberOfAxle,
        ISNULL(SUM(AL.Count), 0) AS TotalVehicle,
        ISNULL(SUM(AL.Axle1), 0) AS Axle1,
        ISNULL(SUM(AL.Axle2), 0) AS Axle2,
        ISNULL(SUM(AL.Axle3), 0) AS Axle3,
        ISNULL(SUM(AL.Axle4), 0) AS Axle4,
        ISNULL(SUM(AL.Axle5), 0) AS Axle5,
        ISNULL(SUM(AL.Axle6), 0) AS Axle6,
        ISNULL(SUM(AL.Axle7), 0) AS Axle7,
        ISNULL(SUM(AL.AxleRemaining), 0) AS AxleRemaining,
        ISNULL(SUM(AL.GrossVehicleWeight), 0) AS GrossVehicleWeight,
        ISNULL(SUM(AL.IsOverloaded), 0) AS TotalOverloaded  -- Total overloaded vehicles
    FROM 
        Dates D
    LEFT JOIN (
        SELECT 
            CONVERT(DATE, DateTime) AS Datetime,
            NumberOfAxle,
            COUNT(1) AS Count,
            SUM(Axle1) AS Axle1,
            SUM(Axle2) AS Axle2,
            SUM(Axle3) AS Axle3,
            SUM(Axle4) AS Axle4,
            SUM(Axle5) AS Axle5,
            SUM(Axle6) AS Axle6,
            SUM(Axle7) AS Axle7,
            SUM(AxleRemaining) AS AxleRemaining,
            SUM(GrossVehicleWeight) AS GrossVehicleWeight,
            SUM(CASE WHEN IsOverloaded = 1 THEN 1 ELSE 0 END) AS IsOverloaded
        FROM 
            AxleLoad AS AL 
        INNER JOIN 
            @Stations S ON AL.StationId = S.StationId
        WHERE 
            DateTime >= @DateStart 
            AND DateTime <= @DateEnd
            AND NumberOfAxle = (CASE WHEN @NumberOfAxle = 0 THEN NumberOfAxle ELSE @NumberOfAxle END)
            AND Wheelbase = (CASE WHEN @Wheelbase = 0 THEN Wheelbase ELSE @Wheelbase END)
            AND ClassStatus = (CASE WHEN @ClassStatus = 0 THEN ClassStatus ELSE @ClassStatus END)
            AND (@CheckWeightCalculation = 1 
                 AND Axle1 + Axle2 + Axle3 + Axle4 + Axle5 + Axle6 + Axle7 + AxleRemaining = GrossVehicleWeight)
        GROUP BY 
            CONVERT(DATE, DateTime),
            NumberOfAxle
    ) AL ON D.Datetime = AL.Datetime
    GROUP BY 
        D.Datetime,
        AL.NumberOfAxle
    ORDER BY 
        D.Datetime,
        AL.NumberOfAxle;
    ";

        return await _db.LoadData<AxleLoadReport, dynamic>(
            query,
            new
            {
                StationId = station.StationId,  
                DateStart = startDate, 
                DateEnd = endDate,  
                NumberOfAxle = 0,  
                Wheelbase = 0,  
                ClassStatus = 0,  
                CheckWeightCalculation = 1  
            });
    }
}
