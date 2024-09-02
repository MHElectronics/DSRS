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
    Task<IEnumerable<AxleLoadReport>> GetMonthlyOverloadedReport(List<Station> stations, DateTime startDate, DateTime endDate);

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
    public async Task<IEnumerable<AxleLoadReport>> GetMonthlyOverloadedReport(List<Station> stations, DateTime startDate, DateTime endDate)
    {
        string stationIds = string.Join(",", stations.Select(s => s.StationId));

        string query = @"
        DECLARE @Stations TABLE(AutoId INT IDENTITY(1,1),StationId INT)
        --DECLARE @DateStart DATE='2024-01-01'
        --,@DateEnd DATE='2024-09-12'
        --,@NumberOfAxle INT=0
        --,@Wheelbase INT=0
        --,@ClassStatus INT=0
        --,@WeightFilterColumn VARCHAR(100)=''
        --,@WeightMin INT=0
        --,@WeightMax INT=0
        --,@CheckWeightCalculation BIT=1

        INSERT INTO @Stations(StationId) VALUES (1),(13)
        CREATE TABLE #T(TotalVehicle INT DEFAULT 0,OverloadVehicle INT DEFAULT 0,[Month] INT,Axle1 INT DEFAULT 0,Axle2 INT DEFAULT 0,Axle3 INT DEFAULT 0,Axle4 INT DEFAULT 0,Axle5 INT DEFAULT 0,Axle6 INT DEFAULT 0,Axle7 INT DEFAULT 0,AxleRemaining INT DEFAULT 0,GrossVehicleWeight INT DEFAULT 0)

        INSERT INTO #T([Month],TotalVehicle,OverloadVehicle,Axle1,Axle2,Axle3,Axle4,Axle5,Axle6,Axle7,AxleRemaining,GrossVehicleWeight)
        SELECT 
        --AL.StationId,TransactionNumber,LaneNumber
        --,DATEPART(WEEKDAY,DateTime) Weekday
        DATEPART(MONTH,DateTime) Month
        --,DATEPART(DAY,DateTime) Day
        --,DATEPART(HOUR,DateTime) Hour
        ,COUNT(1) TotalVehicle
        ,SUM(CAST(IsOverloaded AS INT))
        --,PlateZone,PlateSeries,PlateNumber,VehicleId
        --,VehicleSpeed
        ,SUM(Axle1) Axle1,SUM(Axle2) Axle2,SUM(Axle3) Axle3,SUM(Axle4) Axle4,SUM(Axle5) Axle5,SUM(Axle6) Axle6,SUM(Axle7) Axle7
        ,SUM(AxleRemaining) AxleRemaining,SUM(GrossVehicleWeight) GrossVehicleWeight
        --,IsUnloaded,OverSizedModified
        --,IsOverloaded
        --,NumberOfAxle,Wheelbase,ClassStatus
        --,RecognizedBy,LadenWeight,UnladenWeight
        --,Axle1Time,Axle2Time,Axle3Time,Axle4Time,Axle5Time,Axle6Time,Axle7Time
        FROM AxleLoad AL INNER JOIN @Stations S ON AL.StationId=S.StationId
        WHERE DATEDIFF(DAY,DateTime,@DateStart)<=0
        AND DATEDIFF(DAY,DateTime,@DateEnd)>=0
        --AND IsOverloaded=1
        AND NumberOfAxle=(CASE WHEN @NumberOfAxle=0 THEN NumberOfAxle ELSE @NumberOfAxle END)
        AND Wheelbase=(CASE WHEN @Wheelbase=0 THEN Wheelbase ELSE @Wheelbase END)
        AND ClassStatus=(CASE WHEN @ClassStatus=0 THEN ClassStatus ELSE @ClassStatus END)
        --AND (@CheckWeightCalculation=1 AND Axle1+Axle2+Axle3+Axle4+Axle5+Axle6+Axle7+AxleRemaining=GrossVehicleWeight)
        GROUP BY 
        DATEPART(MONTH,DateTime)
        --DATEPART(WEEKDAY,DateTime)
        --,DATEPART(DAY,DateTime)
        --,DATEPART(HOUR,DateTime)

        DECLARE @Months TABLE(MonthNumber INT)
        INSERT INTO @Months VALUES(1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12)

        INSERT INTO #T(Month)
        SELECT MonthNumber
        FROM @Months
        WHERE MonthNumber NOT IN (SELECT Month FROM #T)


        SELECT *,DATENAME(month,DATEFROMPARTS(1900, Month, 1)) MonthName
        FROM #T
        ORDER BY Month


        DROP TABLE #T
        ";

        var parameters = new
        {
            DateStart = startDate,
            DateEnd = endDate,
            NumberOfAxle = 0,
            Wheelbase = 0,
            ClassStatus = 0,
            CheckWeightCalculation = 1
        };

        return await _db.LoadData<AxleLoadReport, dynamic>(query, parameters);
    }

}
