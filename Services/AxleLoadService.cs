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
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetYearlyOverloadedReport(ReportParameters reportParameters); 
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetMonthlyOverloadedReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetWeeklyOverloadedReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetHourlyOverloadedReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetYearlyVehicleReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetMonthlyVehicleReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetWeeklyVehicleReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetHourlyVehicleReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetYearlyOverweightReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetMonthlyOverweightReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetWeeklyOverweightReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetHourlyOverweightReport(ReportParameters reportParameters);
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
    public async Task<(IEnumerable<AxleLoadReport>,bool,string)> GetYearlyOverloadedReport(ReportParameters reportParameters)
    {
        string stationIds = "(" + string.Join("),(", reportParameters.Stations) + ")";
        bool isSuccess = false;
        string message = "";
        string query = @"
            DECLARE @Stations TABLE(AutoId INT IDENTITY(1,1), StationId INT)

            INSERT INTO @Stations(StationId) VALUES " + stationIds + @"

            DECLARE @Years TABLE([Year] INT)
            DECLARE @CurrentYear INT = YEAR(@DateStart)

            WHILE @CurrentYear <= YEAR(@DateEnd)
            BEGIN
                INSERT INTO @Years VALUES(@CurrentYear)
                SET @CurrentYear = @CurrentYear + 1
            END

            CREATE TABLE #T(
                TotalVehicle INT DEFAULT 0,
                OverloadVehicle INT DEFAULT 0,
                [Year] INT,
                Axle1 INT DEFAULT 0,
                Axle2 INT DEFAULT 0,
                Axle3 INT DEFAULT 0,
                Axle4 INT DEFAULT 0,
                Axle5 INT DEFAULT 0,
                Axle6 INT DEFAULT 0,
                Axle7 INT DEFAULT 0,
                AxleRemaining INT DEFAULT 0,
                GrossVehicleWeight INT DEFAULT 0
            )

            INSERT INTO #T([Year], TotalVehicle, OverloadVehicle, Axle1, Axle2, Axle3, Axle4, Axle5, Axle6, Axle7, AxleRemaining, GrossVehicleWeight)
            SELECT 
                Y.[Year],
                COUNT(AL.StationId) AS TotalVehicle,
                SUM(CAST(AL.IsOverloaded AS INT)) AS OverloadVehicle,
                SUM(AL.Axle1) AS Axle1,
                SUM(AL.Axle2) AS Axle2,
                SUM(AL.Axle3) AS Axle3,
                SUM(AL.Axle4) AS Axle4,
                SUM(AL.Axle5) AS Axle5,
                SUM(AL.Axle6) AS Axle6,
                SUM(AL.Axle7) AS Axle7,
                SUM(AL.AxleRemaining) AS AxleRemaining,
                SUM(AL.GrossVehicleWeight) AS GrossVehicleWeight
            FROM @Years Y
            LEFT JOIN AxleLoad AL ON YEAR(AL.DateTime) = Y.[Year]
                AND AL.StationId IN (SELECT StationId FROM @Stations)
                AND AL.DateTime BETWEEN @DateStart AND @DateEnd
                AND NumberOfAxle = (CASE WHEN @NumberOfAxle = 0 THEN NumberOfAxle ELSE @NumberOfAxle END)
                AND Wheelbase = (CASE WHEN @Wheelbase = 0 THEN Wheelbase ELSE @Wheelbase END)
                AND ClassStatus = (CASE WHEN @ClassStatus = 0 THEN ClassStatus ELSE @ClassStatus END)
            GROUP BY Y.[Year]

            SELECT *,
                CAST([Year] AS VARCHAR) AS DateUnitName
            FROM #T
            ORDER BY [Year]

            DROP TABLE #T
            ";

        var parameters = new
        {
            DateStart = reportParameters.DateStart,
            DateEnd = reportParameters.DateEnd,
            NumberOfAxle = reportParameters.NumberOfAxle,
            Wheelbase = reportParameters.Wheelbase,
            ClassStatus = reportParameters.ClassStatus,
            CheckWeightCalculation = reportParameters.CheckWeightCalculation
        };
        try
        {
            IEnumerable<AxleLoadReport> reports = await _db.LoadData<AxleLoadReport, dynamic>(query, parameters);
            isSuccess = true;
            return (reports, isSuccess, message);
        }
        catch (Exception ex)  
        {
            isSuccess = false;
            message = "Error: " + ex.Message;
        }
        return (null, isSuccess, message);
    }
    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetMonthlyOverloadedReport(ReportParameters reportParameters)
    {
        string stationIds = "(" + string.Join("),(", reportParameters.Stations) + ")";
        bool isSuccess = false;
        string message = "";
        string query = @"
            DECLARE @Stations TABLE(AutoId INT IDENTITY(1,1),StationId INT)

            INSERT INTO @Stations(StationId) VALUES " + stationIds +
                @" CREATE TABLE #T(TotalVehicle INT DEFAULT 0,OverloadVehicle INT DEFAULT 0,[DateUnit] INT,Axle1 INT DEFAULT 0,Axle2 INT DEFAULT 0,Axle3 INT DEFAULT 0,Axle4 INT DEFAULT 0,Axle5 INT DEFAULT 0,Axle6 INT DEFAULT 0,Axle7 INT DEFAULT 0,AxleRemaining INT DEFAULT 0,GrossVehicleWeight INT DEFAULT 0)

            INSERT INTO #T([DateUnit],TotalVehicle,OverloadVehicle,Axle1,Axle2,Axle3,Axle4,Axle5,Axle6,Axle7,AxleRemaining,GrossVehicleWeight)
            SELECT 
            DATEPART(MONTH,DateTime) AS DateUnit
            ,COUNT(1) AS TotalVehicle
            ,SUM(CAST(IsOverloaded AS INT)) AS OverloadVehicle
            ,SUM(Axle1) AS Axle1,SUM(Axle2) AS Axle2,SUM(Axle3) AS Axle3,SUM(Axle4) AS Axle4,SUM(Axle5) AS Axle5,SUM(Axle6) AS Axle6,SUM(Axle7) AS Axle7
            ,SUM(AxleRemaining) AS AxleRemaining,SUM(GrossVehicleWeight) AS GrossVehicleWeight
            FROM AxleLoad AL INNER JOIN @Stations S ON AL.StationId=S.StationId
            WHERE DATEDIFF(DAY,DateTime,@DateStart) <= 0
            AND DATEDIFF(DAY,DateTime,@DateEnd) >= 0
            AND NumberOfAxle = (CASE WHEN @NumberOfAxle = 0 THEN NumberOfAxle ELSE @NumberOfAxle END)
            AND Wheelbase = (CASE WHEN @Wheelbase = 0 THEN Wheelbase ELSE @Wheelbase END)
            AND ClassStatus = (CASE WHEN @ClassStatus = 0 THEN ClassStatus ELSE @ClassStatus END)
            GROUP BY 
            DATEPART(MONTH,DateTime)

    
            DECLARE @DateParts TABLE(MonthNumber INT)

            DECLARE @Min INT,@Max INT
            SELECT @Min=DATEPART(MONTH,@DateStart),@Max=DATEPART(MONTH,@DateEnd)

            INSERT INTO @DateParts
            SELECT N.number
            FROM master..spt_values as N
            WHERE N.number between @Min AND @Max
            AND N.type ='P'
            AND N.number>0

            INSERT INTO #T(DateUnit)
            SELECT MonthNumber
            FROM @DateParts
            WHERE MonthNumber NOT IN (SELECT DateUnit FROM #T)



            SELECT OverloadVehicle,TotalVehicle - OverloadVehicle TotalVehicle,DateUnit,Axle1,Axle2,Axle3,Axle4,Axle5,Axle6,Axle7,AxleRemaining,GrossVehicleWeight
            ,DATENAME(month, DATEFROMPARTS(1900, DateUnit, 1)) AS DateUnitName
            FROM #T
            ORDER BY DateUnit


            DROP TABLE #T
            ";

        var parameters = new
        {
            DateStart = reportParameters.DateStart,
            DateEnd = reportParameters.DateEnd,
            NumberOfAxle = reportParameters.NumberOfAxle,
            Wheelbase = reportParameters.Wheelbase,
            ClassStatus = reportParameters.ClassStatus,
            CheckWeightCalculation = reportParameters.CheckWeightCalculation
        };

        try
        {
            IEnumerable<AxleLoadReport> reports = await _db.LoadData<AxleLoadReport, dynamic>(query, parameters);
            isSuccess = true;
            return (reports, isSuccess, message);
        }
        catch (Exception ex)
        {
            isSuccess = false;
            message = "Error: " + ex.Message;
        }
        return (null, isSuccess, message);
    }
    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetWeeklyOverloadedReport(ReportParameters reportParameters)
    {
        string stationIds = "(" + string.Join("),(", reportParameters.Stations) + ")";
        bool isSuccess = false;
        string message = "";
        string query = @"
    DECLARE @Stations TABLE(AutoId INT IDENTITY(1,1), StationId INT)

    INSERT INTO @Stations(StationId) VALUES " + stationIds + @"

    DECLARE @DateRange TABLE([Date] DATE)
    DECLARE @CurrentDate DATE = @DateStart

    WHILE @CurrentDate <= @DateEnd
    BEGIN
        INSERT INTO @DateRange VALUES(@CurrentDate)
        SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate)
    END

    CREATE TABLE #T(
        TotalVehicle INT DEFAULT 0,
        OverloadVehicle INT DEFAULT 0,
        [DateUnit] INT,
        Axle1 INT DEFAULT 0,
        Axle2 INT DEFAULT 0,
        Axle3 INT DEFAULT 0,
        Axle4 INT DEFAULT 0,
        Axle5 INT DEFAULT 0,
        Axle6 INT DEFAULT 0,
        Axle7 INT DEFAULT 0,
        AxleRemaining INT DEFAULT 0,
        GrossVehicleWeight INT DEFAULT 0
    )

    INSERT INTO #T([DateUnit], TotalVehicle, OverloadVehicle, Axle1, Axle2, Axle3, Axle4, Axle5, Axle6, Axle7, AxleRemaining, GrossVehicleWeight)
    SELECT 
        DATEPART(WEEKDAY, AL.DateTime) AS DateUnit,
        COUNT(1) AS TotalVehicle,
        SUM(CAST(IsOverloaded AS INT)) AS OverloadVehicle,
        SUM(Axle1) AS Axle1,
        SUM(Axle2) AS Axle2,
        SUM(Axle3) AS Axle3,
        SUM(Axle4) AS Axle4,
        SUM(Axle5) AS Axle5,
        SUM(Axle6) AS Axle6,
        SUM(Axle7) AS Axle7,
        SUM(AxleRemaining) AS AxleRemaining,
        SUM(GrossVehicleWeight) AS GrossVehicleWeight
    FROM AxleLoad AL
    INNER JOIN @Stations S ON AL.StationId = S.StationId
    WHERE AL.DateTime BETWEEN @DateStart AND @DateEnd
        AND NumberOfAxle = (CASE WHEN @NumberOfAxle = 0 THEN NumberOfAxle ELSE @NumberOfAxle END)
        AND Wheelbase = (CASE WHEN @Wheelbase = 0 THEN Wheelbase ELSE @Wheelbase END)
        AND ClassStatus = (CASE WHEN @ClassStatus = 0 THEN ClassStatus ELSE @ClassStatus END)
    GROUP BY DATEPART(WEEKDAY, AL.DateTime)

    SELECT *,
        DATENAME(WEEKDAY, DATEADD(DAY, DateUnit - 1, 0)) AS DateUnitName
    FROM #T
    ORDER BY 
        CASE 
            WHEN DateUnit = 7 THEN 1  -- Saturday
            WHEN DateUnit = 1 THEN 2  -- Sunday
            WHEN DateUnit = 2 THEN 3  -- Monday
            WHEN DateUnit = 3 THEN 4  -- Tuesday
            WHEN DateUnit = 4 THEN 5  -- Wednesday
            WHEN DateUnit = 5 THEN 6  -- Thursday
            WHEN DateUnit = 6 THEN 7  -- Friday
        END

    DROP TABLE #T
    ";

        var parameters = new
        {
            DateStart = reportParameters.DateStart,
            DateEnd = reportParameters.DateEnd,
            NumberOfAxle = reportParameters.NumberOfAxle,
            Wheelbase = reportParameters.Wheelbase,
            ClassStatus = reportParameters.ClassStatus,
            CheckWeightCalculation = reportParameters.CheckWeightCalculation
        };

        try
        {
            IEnumerable<AxleLoadReport> reports = await _db.LoadData<AxleLoadReport, dynamic>(query, parameters);
            isSuccess = true;
            return (reports, isSuccess, message);
        }
        catch (Exception ex)
        {
            isSuccess = false;
            message = "Error: " + ex.Message;
        }
        return (null, isSuccess, message);
    }
    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetHourlyOverloadedReport(ReportParameters reportParameters)
    {
        string stationIds = "(" + string.Join("),(", reportParameters.Stations) + ")";
        bool isSuccess = false;
        string message = "";
        string query = @"
        DECLARE @Stations TABLE(AutoId INT IDENTITY(1,1), StationId INT)

        INSERT INTO @Stations(StationId) VALUES " + stationIds + @"

        DECLARE @DateRange TABLE([Date] DATE)
        DECLARE @CurrentDate DATE = @DateStart

        WHILE @CurrentDate <= @DateEnd
        BEGIN
            INSERT INTO @DateRange VALUES(@CurrentDate)
            SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate)
        END

        CREATE TABLE #T(
            TotalVehicle INT DEFAULT 0,
            OverloadVehicle INT DEFAULT 0,
            [DateUnit] INT,
            Axle1 INT DEFAULT 0,
            Axle2 INT DEFAULT 0,
            Axle3 INT DEFAULT 0,
            Axle4 INT DEFAULT 0,
            Axle5 INT DEFAULT 0,
            Axle6 INT DEFAULT 0,
            Axle7 INT DEFAULT 0,
            AxleRemaining INT DEFAULT 0,
            GrossVehicleWeight INT DEFAULT 0
        )

        INSERT INTO #T([DateUnit], TotalVehicle, OverloadVehicle, Axle1, Axle2, Axle3, Axle4, Axle5, Axle6, Axle7, AxleRemaining, GrossVehicleWeight)
        SELECT 
            CASE 
                WHEN DATEPART(HOUR, AL.DateTime) BETWEEN 0 AND 5 THEN 0
                WHEN DATEPART(HOUR, AL.DateTime) BETWEEN 6 AND 11 THEN 6
                WHEN DATEPART(HOUR, AL.DateTime) BETWEEN 12 AND 17 THEN 12
                ELSE 18
            END AS DateUnit,
            COUNT(1) AS TotalVehicle,
            SUM(CAST(IsOverloaded AS INT)) AS OverloadVehicle,
            SUM(Axle1) AS Axle1,
            SUM(Axle2) AS Axle2,
            SUM(Axle3) AS Axle3,
            SUM(Axle4) AS Axle4,
            SUM(Axle5) AS Axle5,
            SUM(Axle6) AS Axle6,
            SUM(Axle7) AS Axle7,
            SUM(AxleRemaining) AS AxleRemaining,
            SUM(GrossVehicleWeight) AS GrossVehicleWeight
        FROM AxleLoad AL
        INNER JOIN @Stations S ON AL.StationId = S.StationId
        WHERE AL.DateTime BETWEEN @DateStart AND @DateEnd
            AND NumberOfAxle = (CASE WHEN @NumberOfAxle = 0 THEN NumberOfAxle ELSE @NumberOfAxle END)
            AND Wheelbase = (CASE WHEN @Wheelbase = 0 THEN Wheelbase ELSE @Wheelbase END)
            AND ClassStatus = (CASE WHEN @ClassStatus = 0 THEN ClassStatus ELSE @ClassStatus END)
        GROUP BY 
            CASE 
                WHEN DATEPART(HOUR, AL.DateTime) BETWEEN 0 AND 5 THEN 0
                WHEN DATEPART(HOUR, AL.DateTime) BETWEEN 6 AND 11 THEN 6
                WHEN DATEPART(HOUR, AL.DateTime) BETWEEN 12 AND 17 THEN 12
                ELSE 18
            END

        SELECT *,
            CASE DateUnit
                WHEN 0 THEN '0:00'
                WHEN 6 THEN '6:00'
                WHEN 12 THEN '12:00'
                ELSE '18:00'
            END AS DateUnitName
        FROM #T
        ORDER BY DateUnit

        DROP TABLE #T
        ";

        var parameters = new
        {
            DateStart = reportParameters.DateStart,
            DateEnd = reportParameters.DateEnd,
            NumberOfAxle = reportParameters.NumberOfAxle,
            Wheelbase = reportParameters.Wheelbase,
            ClassStatus = reportParameters.ClassStatus,
            CheckWeightCalculation = reportParameters.CheckWeightCalculation
        };

        try
        {
            IEnumerable<AxleLoadReport> reports = await _db.LoadData<AxleLoadReport, dynamic>(query, parameters);
            isSuccess = true;
            return (reports, isSuccess, message);
        }
        catch (Exception ex)
        {
            isSuccess = false;
            message = "Error: " + ex.Message;
        }
        return (null, isSuccess, message);
    }

    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetYearlyVehicleReport(ReportParameters reportParameters)
    {
        throw new NotImplementedException();
    }

    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetMonthlyVehicleReport(ReportParameters reportParameters)
    {
        throw new NotImplementedException();
    }

    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetWeeklyVehicleReport(ReportParameters reportParameters)
    {
        throw new NotImplementedException();
    }

    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetHourlyVehicleReport(ReportParameters reportParameters)
    {
        throw new NotImplementedException();
    }

    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetYearlyOverweightReport(ReportParameters reportParameters)
    {
        string stationIds = "(" + string.Join("),(", reportParameters.Stations) + ")";
        bool isSuccess = false;
        string message = "";
        string query = @"
            DECLARE @Stations TABLE(AutoId INT IDENTITY(1,1), StationId INT)

            INSERT INTO @Stations(StationId) VALUES " + stationIds + @"

            DECLARE @Years TABLE([Year] INT)
            DECLARE @CurrentYear INT = YEAR(@DateStart)

            WHILE @CurrentYear <= YEAR(@DateEnd)
            BEGIN
                INSERT INTO @Years VALUES(@CurrentYear)
                SET @CurrentYear = @CurrentYear + 1
            END

            CREATE TABLE #T(
                TotalVehicle INT DEFAULT 0,
                OverloadVehicle INT DEFAULT 0,
                [Year] INT,
                Axle1 INT DEFAULT 0,
                Axle2 INT DEFAULT 0,
                Axle3 INT DEFAULT 0,
                Axle4 INT DEFAULT 0,
                Axle5 INT DEFAULT 0,
                Axle6 INT DEFAULT 0,
                Axle7 INT DEFAULT 0,
                AxleRemaining INT DEFAULT 0,
                GrossVehicleWeight INT DEFAULT 0,
                NumberOfAxle INT DEFAULT 0
            )

            INSERT INTO #T([Year], TotalVehicle, OverloadVehicle, Axle1, Axle2, Axle3, Axle4, Axle5, Axle6, Axle7, AxleRemaining, GrossVehicleWeight, NumberOfAxle)
            SELECT 
                Y.[Year],
                COUNT(AL.StationId) AS TotalVehicle,
                SUM(CAST(AL.IsOverloaded AS INT)) AS OverloadVehicle,
                SUM(AL.Axle1) AS Axle1,
                SUM(AL.Axle2) AS Axle2,
                SUM(AL.Axle3) AS Axle3,
                SUM(AL.Axle4) AS Axle4,
                SUM(AL.Axle5) AS Axle5,
                SUM(AL.Axle6) AS Axle6,
                SUM(AL.Axle7) AS Axle7,
                SUM(AL.AxleRemaining) AS AxleRemaining,
                SUM(AL.GrossVehicleWeight) AS GrossVehicleWeight,
                NumberOfAxle
            FROM @Years Y
            LEFT JOIN AxleLoad AL ON YEAR(AL.DateTime) = Y.[Year]
                AND AL.StationId IN (SELECT StationId FROM @Stations)
                AND AL.DateTime BETWEEN @DateStart AND @DateEnd
                AND NumberOfAxle = (CASE WHEN @NumberOfAxle = 0 THEN NumberOfAxle ELSE @NumberOfAxle END)
                AND Wheelbase = (CASE WHEN @Wheelbase = 0 THEN Wheelbase ELSE @Wheelbase END)
                AND ClassStatus = (CASE WHEN @ClassStatus = 0 THEN ClassStatus ELSE @ClassStatus END)
            GROUP BY Y.[Year], NumberOfAxle

            SELECT *,
                CAST([Year] AS VARCHAR) AS DateUnitName
            FROM #T
            ORDER BY [Year]

            DROP TABLE #T
            ";

        var parameters = new
        {
            DateStart = reportParameters.DateStart,
            DateEnd = reportParameters.DateEnd,
            NumberOfAxle = reportParameters.NumberOfAxle,
            Wheelbase = reportParameters.Wheelbase,
            ClassStatus = reportParameters.ClassStatus,
            CheckWeightCalculation = reportParameters.CheckWeightCalculation
        };
        try
        {
            IEnumerable<AxleLoadReport> reports = await _db.LoadData<AxleLoadReport, dynamic>(query, parameters);
            isSuccess = true;
            return (reports, isSuccess, message);
        }
        catch (Exception ex)
        {
            isSuccess = false;
            message = "Error: " + ex.Message;
        }
        return (null, isSuccess, message);
    }

    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetMonthlyOverweightReport(ReportParameters reportParameters)
    {
        string stationIds = "(" + string.Join("),(", reportParameters.Stations) + ")";
        bool isSuccess = false;
        string message = "";
        string query = @"
            DECLARE @Stations TABLE(AutoId INT IDENTITY(1,1),StationId INT)

            INSERT INTO @Stations(StationId) VALUES " + stationIds +
                @" CREATE TABLE #T(TotalVehicle INT DEFAULT 0,OverloadVehicle INT DEFAULT 0,[DateUnit] INT,Axle1 INT DEFAULT 0,Axle2 INT DEFAULT 0,Axle3 INT DEFAULT 0,Axle4 INT DEFAULT 0,Axle5 INT DEFAULT 0,Axle6 INT DEFAULT 0,Axle7 INT DEFAULT 0,AxleRemaining INT DEFAULT 0,GrossVehicleWeight INT DEFAULT 0, NumberOfAxle INT DEFAULT 0)

            INSERT INTO #T([DateUnit],TotalVehicle,OverloadVehicle,Axle1,Axle2,Axle3,Axle4,Axle5,Axle6,Axle7,AxleRemaining,GrossVehicleWeight,NumberOfAxle)
            SELECT 
            DATEPART(MONTH,DateTime) AS DateUnit
            ,COUNT(1) AS TotalVehicle
            ,SUM(CAST(IsOverloaded AS INT)) AS OverloadVehicle
            ,SUM(Axle1) AS Axle1,SUM(Axle2) AS Axle2,SUM(Axle3) AS Axle3,SUM(Axle4) AS Axle4,SUM(Axle5) AS Axle5,SUM(Axle6) AS Axle6,SUM(Axle7) AS Axle7
            ,SUM(AxleRemaining) AS AxleRemaining,SUM(GrossVehicleWeight) AS GrossVehicleWeight
            ,NumberOfAxle
            FROM AxleLoad AL INNER JOIN @Stations S ON AL.StationId=S.StationId
            WHERE DATEDIFF(DAY,DateTime,@DateStart) <= 0
            AND DATEDIFF(DAY,DateTime,@DateEnd) >= 0
            AND NumberOfAxle = (CASE WHEN @NumberOfAxle = 0 THEN NumberOfAxle ELSE @NumberOfAxle END)
            AND Wheelbase = (CASE WHEN @Wheelbase = 0 THEN Wheelbase ELSE @Wheelbase END)
            AND ClassStatus = (CASE WHEN @ClassStatus = 0 THEN ClassStatus ELSE @ClassStatus END)
            GROUP BY 
            DATEPART(MONTH,DateTime), NumberOfAxle

    
            DECLARE @DateParts TABLE(MonthNumber INT)

            DECLARE @Min INT,@Max INT
            SELECT @Min=DATEPART(MONTH,@DateStart),@Max=DATEPART(MONTH,@DateEnd)

            INSERT INTO @DateParts
            SELECT N.number
            FROM master..spt_values as N
            WHERE N.number between @Min AND @Max
            AND N.type ='P'
            AND N.number>0

            INSERT INTO #T(DateUnit)
            SELECT MonthNumber
            FROM @DateParts
            WHERE MonthNumber NOT IN (SELECT DateUnit FROM #T)



            SELECT OverloadVehicle,TotalVehicle - OverloadVehicle TotalVehicle,DateUnit,Axle1,Axle2,Axle3,Axle4,Axle5,Axle6,Axle7,AxleRemaining,GrossVehicleWeight
            ,DATENAME(month, DATEFROMPARTS(1900, DateUnit, 1)) AS DateUnitName
            FROM #T
            ORDER BY DateUnit


            DROP TABLE #T
            ";

        var parameters = new
        {
            DateStart = reportParameters.DateStart,
            DateEnd = reportParameters.DateEnd,
            NumberOfAxle = reportParameters.NumberOfAxle,
            Wheelbase = reportParameters.Wheelbase,
            ClassStatus = reportParameters.ClassStatus,
            CheckWeightCalculation = reportParameters.CheckWeightCalculation
        };

        try
        {
            IEnumerable<AxleLoadReport> reports = await _db.LoadData<AxleLoadReport, dynamic>(query, parameters);
            isSuccess = true;
            return (reports, isSuccess, message);
        }
        catch (Exception ex)
        {
            isSuccess = false;
            message = "Error: " + ex.Message;
        }
        return (null, isSuccess, message);
    }

    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetWeeklyOverweightReport(ReportParameters reportParameters)
    {
        string stationIds = "(" + string.Join("),(", reportParameters.Stations) + ")";
        bool isSuccess = false;
        string message = "";
        string query = @"
        DECLARE @Stations TABLE(AutoId INT IDENTITY(1,1), StationId INT)

        INSERT INTO @Stations(StationId) VALUES " + stationIds + @"

        DECLARE @DateRange TABLE([Date] DATE)
        DECLARE @CurrentDate DATE = @DateStart

        WHILE @CurrentDate <= @DateEnd
        BEGIN
            INSERT INTO @DateRange VALUES(@CurrentDate)
            SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate)
        END

        SELECT 
            DATEPART(WEEKDAY, AL.DateTime) AS DateUnit,
            DATENAME(WEEKDAY, AL.DateTime) AS DateUnitName,
            AL.NumberOfAxle,
            SUM(CAST(AL.IsOverloaded AS INT)) AS OverloadVehicle
        FROM AxleLoad AL
        INNER JOIN @Stations S ON AL.StationId = S.StationId
        WHERE AL.DateTime BETWEEN @DateStart AND @DateEnd
            AND AL.NumberOfAxle = (CASE WHEN @NumberOfAxle = 0 THEN AL.NumberOfAxle ELSE @NumberOfAxle END)
            AND AL.Wheelbase = (CASE WHEN @Wheelbase = 0 THEN AL.Wheelbase ELSE @Wheelbase END)
            AND AL.ClassStatus = (CASE WHEN @ClassStatus = 0 THEN AL.ClassStatus ELSE @ClassStatus END)
        GROUP BY DATEPART(WEEKDAY, AL.DateTime), DATENAME(WEEKDAY, AL.DateTime), AL.NumberOfAxle
        ORDER BY 
            CASE 
                WHEN DATEPART(WEEKDAY, AL.DateTime) = 7 THEN 1  -- Saturday
                WHEN DATEPART(WEEKDAY, AL.DateTime) = 1 THEN 2  -- Sunday
                WHEN DATEPART(WEEKDAY, AL.DateTime) = 2 THEN 3  -- Monday
                WHEN DATEPART(WEEKDAY, AL.DateTime) = 3 THEN 4  -- Tuesday
                WHEN DATEPART(WEEKDAY, AL.DateTime) = 4 THEN 5  -- Wednesday
                WHEN DATEPART(WEEKDAY, AL.DateTime) = 5 THEN 6  -- Thursday
                WHEN DATEPART(WEEKDAY, AL.DateTime) = 6 THEN 7  -- Friday
            END
        ";

        var parameters = new
        {
            DateStart = reportParameters.DateStart,
            DateEnd = reportParameters.DateEnd,
            NumberOfAxle = reportParameters.NumberOfAxle,
            Wheelbase = reportParameters.Wheelbase,
            ClassStatus = reportParameters.ClassStatus,
            CheckWeightCalculation = reportParameters.CheckWeightCalculation
        };

        try
        {
            IEnumerable<AxleLoadReport> reports = await _db.LoadData<AxleLoadReport, dynamic>(query, parameters);
            isSuccess = true;
            return (reports, isSuccess, message);
        }
        catch (Exception ex)
        {
            isSuccess = false;
            message = "Error: " + ex.Message;
        }
        return (null, isSuccess, message);
    }

    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetHourlyOverweightReport(ReportParameters reportParameters)
    {
        string stationIds = "(" + string.Join("),(", reportParameters.Stations) + ")";
        bool isSuccess = false;
        string message = "";
        string query = @"
        DECLARE @Stations TABLE(AutoId INT IDENTITY(1,1), StationId INT)

        INSERT INTO @Stations(StationId) VALUES " + stationIds + @"

        DECLARE @DateRange TABLE([Date] DATE)
        DECLARE @CurrentDate DATE = @DateStart

        WHILE @CurrentDate <= @DateEnd
        BEGIN
            INSERT INTO @DateRange VALUES(@CurrentDate)
            SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate)
        END

        CREATE TABLE #T(
            TotalVehicle INT DEFAULT 0,
            OverloadVehicle INT DEFAULT 0,
            [DateUnit] INT,
            Axle1 INT DEFAULT 0,
            Axle2 INT DEFAULT 0,
            Axle3 INT DEFAULT 0,
            Axle4 INT DEFAULT 0,
            Axle5 INT DEFAULT 0,
            Axle6 INT DEFAULT 0,
            Axle7 INT DEFAULT 0,
            AxleRemaining INT DEFAULT 0,
            GrossVehicleWeight INT DEFAULT 0,
            NumberOfAxle INT DEFAULT 0  
        )

        INSERT INTO #T([DateUnit], TotalVehicle, OverloadVehicle, Axle1, Axle2, Axle3, Axle4, Axle5, Axle6, Axle7, AxleRemaining, GrossVehicleWeight, NumberOfAxle)
        SELECT
            CASE 
                WHEN DATEPART(HOUR, AL.DateTime) BETWEEN 0 AND 5 THEN 0
                WHEN DATEPART(HOUR, AL.DateTime) BETWEEN 6 AND 11 THEN 6
                WHEN DATEPART(HOUR, AL.DateTime) BETWEEN 12 AND 17 THEN 12
                ELSE 18
            END AS DateUnit,
            COUNT(1) AS TotalVehicle,
            SUM(CAST(IsOverloaded AS INT)) AS OverloadVehicle,
            SUM(Axle1) AS Axle1,
            SUM(Axle2) AS Axle2,
            SUM(Axle3) AS Axle3,
            SUM(Axle4) AS Axle4,
            SUM(Axle5) AS Axle5,
            SUM(Axle6) AS Axle6,
            SUM(Axle7) AS Axle7,
            SUM(AxleRemaining) AS AxleRemaining,
            SUM(GrossVehicleWeight) AS GrossVehicleWeight,
            NumberOfAxle
        FROM AxleLoad AL
        INNER JOIN @Stations S ON AL.StationId = S.StationId
        WHERE AL.DateTime BETWEEN @DateStart AND @DateEnd
            AND NumberOfAxle = (CASE WHEN @NumberOfAxle = 0 THEN NumberOfAxle ELSE @NumberOfAxle END)
            AND Wheelbase = (CASE WHEN @Wheelbase = 0 THEN Wheelbase ELSE @Wheelbase END)
            AND ClassStatus = (CASE WHEN @ClassStatus = 0 THEN ClassStatus ELSE @ClassStatus END)
        GROUP BY 
            CASE 
                WHEN DATEPART(HOUR, AL.DateTime) BETWEEN 0 AND 5 THEN 0
                WHEN DATEPART(HOUR, AL.DateTime) BETWEEN 6 AND 11 THEN 6
                WHEN DATEPART(HOUR, AL.DateTime) BETWEEN 12 AND 17 THEN 12
                ELSE 18
            END, NumberOfAxle

        SELECT *,
            CASE DateUnit
                WHEN 0 THEN '0:00'
                WHEN 6 THEN '6:00'
                WHEN 12 THEN '12:00'
                ELSE '18:00'
            END AS DateUnitName
        FROM #T
        ORDER BY DateUnit

        DROP TABLE #T
        ";

        var parameters = new
        {
            DateStart = reportParameters.DateStart,
            DateEnd = reportParameters.DateEnd,
            NumberOfAxle = reportParameters.NumberOfAxle,
            Wheelbase = reportParameters.Wheelbase,
            ClassStatus = reportParameters.ClassStatus,
            CheckWeightCalculation = reportParameters.CheckWeightCalculation
        };

        try
        {
            IEnumerable<AxleLoadReport> reports = await _db.LoadData<AxleLoadReport, dynamic>(query, parameters);
            isSuccess = true;
            return (reports, isSuccess, message);
        }
        catch (Exception ex)
        {
            isSuccess = false;
            message = "Error: " + ex.Message;
        }
        return (null, isSuccess, message);
    }
}
