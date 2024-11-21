using BOL;
using BOL.CustomModels;
using Services.Helpers;

namespace Services.Reports;

public interface IOverloadReportService
{
    Task<(IEnumerable<LoadData>, bool, string)> Get(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetDailyOverloadedTimeSeriesReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetDailyOverloadedNumberOfAxlesReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetDailyOverloadedHistogramReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetHourlyOverloadedTimeSeriesReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetHourlyOverloadedNumberOfAxlesReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetHourlyOverloadedHistogramReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetMonthlyOverloadedTimeSeriesReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetMonthlyOverloadedNumberOfAxlesReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetMonthlyOverloadedHistogramReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetWeeklyOverloadedTimeSeriesReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetWeeklyOverloadedNumberOfAxlesReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetWeeklyOverloadedHistogramReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetYearlyOverloadedTimeSeriesReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetYearlyOverloadedNumberOfAxlesReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetYearlyOverloadedHistogramReport(ReportParameters reportParameters);
    Task<(IEnumerable<AxleLoadReport>, bool, string)> GetAxleWiseHistogramReport(ReportParameters reportParameters, decimal equivalentAxleLoad);
}

public class OverloadReportService(ISqlDataAccess _db) : IOverloadReportService
{
    public async Task<(IEnumerable<LoadData>, bool, string)> Get(ReportParameters reportParameters)
    {
        bool isSuccess = false;
        string message = "";
        string query = this.GetStationTableQuery(reportParameters) +
        @" 
        SELECT SN.StationName,TransactionNumber,LaneNumber,DateTime 
        ,PlateZone,PlateSeries,PlateNumber,NumberOfAxle,VehicleSpeed
        ,Axle1,Axle2,Axle3,Axle4,Axle5,Axle6,Axle7 
        ,AxleRemaining,GrossVehicleWeight,IsUnloaded,IsOverloaded 
        ,OverSizedModified,Wheelbase,ClassStatus,RecognizedBy,IsBRTAInclude,LadenWeight,UnladenWeight,ReceiptNumber,BillNumber
        ,Axle1Time,Axle2Time,Axle3Time,Axle4Time,Axle5Time,Axle6Time,Axle7Time
        FROM AxleLoad AS AL INNER JOIN Stations SN ON AL.StationId=SN.StationId ";

        query += this.GetFilterClause(reportParameters);
        
        var parameters = new
        {
            StationIds = reportParameters.Stations,
            DateStart = reportParameters.DateStart,
            DateEnd = reportParameters.DateEnd,
            NumberOfAxle = reportParameters.NumberOfAxle,
            Wheelbase = reportParameters.Wheelbase,
            ClassStatus = reportParameters.ClassStatus,
            TimeStart = reportParameters.TimeStart.ToTimeSpan(),
            TimeEnd = reportParameters.TimeEnd.ToTimeSpan(),
        };

        try
        {
            IEnumerable<LoadData> reports = await _db.LoadData<LoadData, object>(query, parameters);
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
    
    #region Overloaded Histogram report query
    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetYearlyOverloadedHistogramReport(ReportParameters reportParameters)
    {
        bool isSuccess = false;
        string message = "";
        string query = this.GetStationTableQuery(reportParameters) + @"
        DECLARE @Multiplier DECIMAL(18,2) = 1000
        DECLARE @TotalIteration INT = 50

        DECLARE @Range TABLE(GroupId INT, Minimum INT, Maximum INT)

        INSERT INTO @Range(GroupId, Minimum, Maximum)
        SELECT DISTINCT number, (number - 1) * @Multiplier, number * @Multiplier
        FROM master..[spt_values] 
        WHERE number >= 1 AND number <= @TotalIteration

        SELECT 
            DATEPART(YEAR, AL.DateTime) AS DateUnit,
            DATENAME(YEAR, AL.DateTime) AS DateUnitName,
            R.GroupId,
            COUNT(1) AS TotalVehicle,
            SUM(CAST(IsOverloaded AS INT)) AS OverloadVehicle,
            CAST((R.Minimum / 1000) AS VARCHAR(100)) + '-' + CAST((R.Maximum / 1000) AS VARCHAR(100)) AS GrossVehicleWeightRange
        FROM AxleLoad AL
        INNER JOIN @Range R ON (AL.GrossVehicleWeight > R.Minimum AND AL.GrossVehicleWeight <= R.Maximum)";

        query += this.GetFilterClause(reportParameters);

        query += @" GROUP BY 
            DATEPART(YEAR, AL.DateTime),
            DATENAME(YEAR, AL.DateTime),
            R.GroupId,
            CAST((R.Minimum / 1000) AS VARCHAR(100)) + '-' + CAST((R.Maximum / 1000) AS VARCHAR(100))
        ORDER BY DATEPART(YEAR, AL.DateTime), R.GroupId";

        var parameters = new
        {
            DateStart = reportParameters.DateStart,
            DateEnd = reportParameters.DateEnd,
            NumberOfAxle = reportParameters.NumberOfAxle,
            Wheelbase = reportParameters.Wheelbase,
            ClassStatus = reportParameters.ClassStatus,
            TimeStart = reportParameters.TimeStart.ToTimeSpan(),
            TimeEnd = reportParameters.TimeEnd.ToTimeSpan()
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
    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetMonthlyOverloadedHistogramReport(ReportParameters reportParameters)
    {
        bool isSuccess = false;
        string message = "";
        string query = this.GetStationTableQuery(reportParameters) + @"
        DECLARE @Multiplier DECIMAL(18,2) = 1000
        DECLARE @TotalIteration INT = 50

        DECLARE @Range TABLE(GroupId INT, Minimum INT, Maximum INT)

        INSERT INTO @Range(GroupId, Minimum, Maximum)
        SELECT DISTINCT number, (number - 1) * @Multiplier, number * @Multiplier
        FROM master..[spt_values] 
        WHERE number >= 1 AND number <= @TotalIteration

        SELECT 
            DATEPART(MONTH, AL.DateTime) AS DateUnit,
            DATENAME(MONTH, AL.DateTime) AS DateUnitName,
            R.GroupId,
            COUNT(1) AS TotalVehicle,
            SUM(CAST(IsOverloaded AS INT)) AS OverloadVehicle,
            CAST((R.Minimum / 1000) AS VARCHAR(100)) + '-' + CAST((R.Maximum / 1000) AS VARCHAR(100)) AS GrossVehicleWeightRange
        FROM AxleLoad AL
        INNER JOIN @Range R ON (AL.GrossVehicleWeight > R.Minimum AND AL.GrossVehicleWeight <= R.Maximum)";

        query += this.GetFilterClause(reportParameters);

        query += @" GROUP BY 
            DATEPART(MONTH, AL.DateTime),
            DATENAME(MONTH, AL.DateTime),
            R.GroupId,
            CAST((R.Minimum / 1000) AS VARCHAR(100)) + '-' + CAST((R.Maximum / 1000) AS VARCHAR(100))
        ORDER BY DATEPART(MONTH, AL.DateTime), R.GroupId";

        var parameters = new
        {
            DateStart = reportParameters.DateStart,
            DateEnd = reportParameters.DateEnd,
            NumberOfAxle = reportParameters.NumberOfAxle,
            Wheelbase = reportParameters.Wheelbase,
            ClassStatus = reportParameters.ClassStatus,
            TimeStart = reportParameters.TimeStart.ToTimeSpan(),
            TimeEnd = reportParameters.TimeEnd.ToTimeSpan()
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
    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetWeeklyOverloadedHistogramReport(ReportParameters reportParameters)
    {
        bool isSuccess = false;
        string message = "";
        string query = this.GetStationTableQuery(reportParameters) + @"
        DECLARE @Multiplier DECIMAL(18,2) = 1000
        DECLARE @TotalIteration INT = 50

        DECLARE @Range TABLE(GroupId INT, Minimum INT, Maximum INT)

        INSERT INTO @Range(GroupId, Minimum, Maximum)
        SELECT DISTINCT number, (number - 1) * @Multiplier, number * @Multiplier
        FROM master..[spt_values] 
        WHERE number >= 1 AND number <= @TotalIteration

        SELECT 
            DATEPART(WEEKDAY, AL.DateTime) AS DateUnit,
            DATENAME(WEEKDAY, DATEADD(DAY, DATEPART(WEEKDAY, AL.DateTime) - 1, 0)) AS DateUnitName,
            R.GroupId,
            COUNT(1) AS TotalVehicle,
            SUM(CAST(IsOverloaded AS INT)) AS OverloadVehicle,
            CAST((R.Minimum / 1000) AS VARCHAR(100)) + '-' + CAST((R.Maximum / 1000) AS VARCHAR(100)) AS GrossVehicleWeightRange
        FROM AxleLoad AL
        INNER JOIN @Range R ON (AL.GrossVehicleWeight > R.Minimum AND AL.GrossVehicleWeight <= R.Maximum)";

        query += this.GetFilterClause(reportParameters);

        query += @" GROUP BY 
            DATEPART(WEEKDAY, AL.DateTime),
            DATENAME(WEEKDAY, DATEADD(DAY, DATEPART(WEEKDAY, AL.DateTime) - 1, 0)),
            R.GroupId,
            CAST((R.Minimum / 1000) AS VARCHAR(100)) + '-' + CAST((R.Maximum / 1000) AS VARCHAR(100))
        ORDER BY DATEPART(WEEKDAY, AL.DateTime), R.GroupId";

        var parameters = new
        {
            DateStart = reportParameters.DateStart,
            DateEnd = reportParameters.DateEnd,
            NumberOfAxle = reportParameters.NumberOfAxle,
            Wheelbase = reportParameters.Wheelbase,
            ClassStatus = reportParameters.ClassStatus,
            TimeStart = reportParameters.TimeStart.ToTimeSpan(),
            TimeEnd = reportParameters.TimeEnd.ToTimeSpan()
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
    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetDailyOverloadedHistogramReport(ReportParameters reportParameters)
    {
        bool isSuccess = false;
        string message = "";
        string query = this.GetStationTableQuery(reportParameters) + @"
        DECLARE @Multiplier DECIMAL(18,2) = 1000
        DECLARE @TotalIteration INT = 50

        DECLARE @Range TABLE(GroupId INT, Minimum INT, Maximum INT)

        INSERT INTO @Range(GroupId, Minimum, Maximum)
        SELECT DISTINCT number, (number - 1) * @Multiplier, number * @Multiplier
        FROM master..[spt_values] 
        WHERE number >= 1 AND number <= @TotalIteration

        SELECT 
            DAY(AL.DateTime) AS DateUnit,  -- Extract only the day number as INT
            CONVERT(NVARCHAR, CAST(AL.DateTime AS DATE), 23) AS DateUnitName,  -- Format full date as YYYY-MM-DD string
            R.GroupId,
            COUNT(1) AS TotalVehicle,
            SUM(CAST(IsOverloaded AS INT)) AS OverloadVehicle,
            CAST((R.Minimum / 1000) AS VARCHAR(100)) + '-' + CAST((R.Maximum / 1000) AS VARCHAR(100)) AS GrossVehicleWeightRange
        FROM AxleLoad AL
        INNER JOIN @Range R ON (AL.GrossVehicleWeight > R.Minimum AND AL.GrossVehicleWeight <= R.Maximum)";

        query += this.GetFilterClause(reportParameters);

        query += @" GROUP BY 
            DAY(AL.DateTime),  -- Group by day number
            CAST(AL.DateTime AS DATE),  -- Group by full date
            R.GroupId,
            CAST((R.Minimum / 1000) AS VARCHAR(100)) + '-' + CAST((R.Maximum / 1000) AS VARCHAR(100))
        ORDER BY DATEPART(DAY, AL.DateTime), R.GroupId";

        var parameters = new
        {
            DateStart = reportParameters.DateStart,
            DateEnd = reportParameters.DateEnd,
            NumberOfAxle = reportParameters.NumberOfAxle,
            Wheelbase = reportParameters.Wheelbase,
            ClassStatus = reportParameters.ClassStatus,
            TimeStart = reportParameters.TimeStart.ToTimeSpan(),
            TimeEnd = reportParameters.TimeEnd.ToTimeSpan()
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
    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetHourlyOverloadedHistogramReport(ReportParameters reportParameters)
    {
        bool isSuccess = false;
        string message = "";
        string query = this.GetStationTableQuery(reportParameters) + @"
        DECLARE @Multiplier DECIMAL(18,2) = 1000
        DECLARE @TotalIteration INT = 50

        DECLARE @Range TABLE(GroupId INT, Minimum INT, Maximum INT)

        INSERT INTO @Range(GroupId, Minimum, Maximum)
        SELECT DISTINCT number, (number - 1) * @Multiplier, number * @Multiplier
        FROM master..[spt_values] 
        WHERE number >= 1 AND number <= @TotalIteration

        SELECT 
            DATEPART(HOUR, AL.DateTime) AS DateUnit,
            CAST(DATEPART(HOUR, AL.DateTime) AS VARCHAR) + ':00' AS DateUnitName,
            R.GroupId,
            COUNT(1) AS TotalVehicle,
            SUM(CAST(IsOverloaded AS INT)) AS OverloadVehicle,
            CAST((R.Minimum / 1000) AS VARCHAR(100)) + '-' + CAST((R.Maximum / 1000) AS VARCHAR(100)) AS GrossVehicleWeightRange
        FROM AxleLoad AL
        INNER JOIN @Range R ON (AL.GrossVehicleWeight > R.Minimum AND AL.GrossVehicleWeight <= R.Maximum)";

        query += this.GetFilterClause(reportParameters);

        query += @" GROUP BY 
            DATEPART(HOUR, AL.DateTime),
            CAST(DATEPART(HOUR, AL.DateTime) AS VARCHAR),
            R.GroupId,
            CAST((R.Minimum / 1000) AS VARCHAR(100)) + '-' + CAST((R.Maximum / 1000) AS VARCHAR(100))
        ORDER BY DATEPART(HOUR, AL.DateTime), R.GroupId";

        var parameters = new
        {
            DateStart = reportParameters.DateStart,
            DateEnd = reportParameters.DateEnd,
            NumberOfAxle = reportParameters.NumberOfAxle,
            Wheelbase = reportParameters.Wheelbase,
            ClassStatus = reportParameters.ClassStatus,
            TimeStart = reportParameters.TimeStart.ToTimeSpan(),
            TimeEnd = reportParameters.TimeEnd.ToTimeSpan()
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
    #endregion


    #region Overloaded Histogram Part 2 report query
    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetAxleWiseHistogramReport(ReportParameters reportParameters, decimal equivalentAxleLoad)
    {
        //Disable number of axle filter
        reportParameters.NumberOfAxle = new();

        bool isSuccess = false;
        string message = "";
        string whereClause  = this.GetFilterClause(reportParameters);
        string query = @"DECLARE @Multiplier DECIMAL(18,2) = 1000
DECLARE @TotalIteration INT = 200 ";

        query += this.GetStationTableQuery(reportParameters);

        query += @" DECLARE @Range TABLE(GroupId INT, Minimum DECIMAL(18,0), Maximum DECIMAL(18,0),MediumWeight DECIMAL(18,2))

INSERT INTO @Range(GroupId, Minimum, Maximum,MediumWeight)
SELECT DISTINCT number, (number - 1) * @Multiplier, number * @Multiplier,((number-1)*@Multiplier + number*@Multiplier)/2
FROM master..[spt_values]
WHERE number >= 1 AND number <= @TotalIteration


DECLARE @GoupCount TABLE(GroupId INT,TotalNumberOfAxles INT)

INSERT INTO @GoupCount(GroupId,TotalNumberOfAxles)
SELECT GroupId,SUM(TotalNumberOfAxles) TotalNumberOfAxles
FROM (";

        for(int i=1; i<=7; i++)
        {
            query += $@"
--Axle {i}
SELECT R.GroupId,COUNT(1) TotalNumberOfAxles
FROM AxleLoad AL
INNER JOIN @Range R ON (AL.Axle{i} >= R.Minimum AND AL.Axle{i} < R.Maximum)
{whereClause}
AND Al.NumberOfAxle>={i}
GROUP BY R.GroupId
";
            if (i != 7)
            {
                query += " UNION ALL ";
            }
        }

        query += @") AS Sub
GROUP BY GroupId 

--Ton coversion
DECLARE @TonConversion INT=1000
UPDATE @Range
SET Minimum=Minimum/@TonConversion
,Maximum=Maximum/@TonConversion
,MediumWeight=MediumWeight/@TonConversion

SELECT R.GroupId,R.Minimum,R.Maximum,R.MediumWeight,CAST(R.Minimum AS VARCHAR(10))+'-'+CAST(R.Maximum AS VARCHAR(10)) GrossVehicleWeightRange
,C.TotalNumberOfAxles
--,POWER(R.MediumWeight,4) MediumWeight4,C.TotalNumberOfAxles*POWER(R.MediumWeight,4) Influence
--,POWER(R.MediumWeight/@EquivalentAxleLoad,4) MediumWeight4_2,C.TotalNumberOfAxles*POWER(R.MediumWeight/@EquivalentAxleLoad,4) Influence_2
--,POWER(R.MediumWeight/@EquivalentAxleLoad2,4) MediumWeight4_3,C.TotalNumberOfAxles*POWER(R.MediumWeight/@EquivalentAxleLoad2,4) Influence_3
,CAST(POWER(R.MediumWeight,4) AS DECIMAL(18,2)) MediumWeight4,CAST(C.TotalNumberOfAxles*POWER(R.MediumWeight,4) AS DECIMAL(18,2)) Influence
,CAST(POWER(R.MediumWeight/@EquivalentAxleLoad,4) AS DECIMAL(18,2)) MediumWeight4_2,CAST(C.TotalNumberOfAxles*POWER(R.MediumWeight/@EquivalentAxleLoad,4) AS DECIMAL(18,2)) Influence_2
--,CAST(POWER(R.MediumWeight/@EquivalentAxleLoad2,4) AS DECIMAL(18,2)) MediumWeight4_3,CAST(C.TotalNumberOfAxles*POWER(R.MediumWeight/@EquivalentAxleLoad2,4) AS DECIMAL(18,2)) Influence_3
FROM @Range R INNER JOIN @GoupCount C ON R.GroupId=C.GroupId";

        var parameters = new
        {
            equivalentAxleLoad,
            reportParameters.DateStart,
            reportParameters.DateEnd,
            reportParameters.Wheelbase,
            reportParameters.ClassStatus,
            TimeStart = reportParameters.TimeStart.ToTimeSpan(),
            TimeEnd = reportParameters.TimeEnd.ToTimeSpan()
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
    #endregion

    #region Overloaded Time Series report query
    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetYearlyOverloadedTimeSeriesReport(ReportParameters reportParameters)
    {
        bool isSuccess = false;
        string message = "";
        string query = this.GetStationTableQuery(reportParameters) + @"
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
        FROM @Years Y LEFT JOIN AxleLoad AL ON YEAR(AL.DateTime) = Y.[Year]
        ";

        query += this.GetFilterClause(reportParameters);

        query += @" GROUP BY Y.[Year]

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
            TimeStart = reportParameters.TimeStart.ToTimeSpan(),
            TimeEnd = reportParameters.TimeEnd.ToTimeSpan()
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
    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetMonthlyOverloadedTimeSeriesReport(ReportParameters reportParameters)
    {
        bool isSuccess = false;
        string message = "";
        string query = this.GetStationTableQuery(reportParameters) +
            @" CREATE TABLE #T(TotalVehicle INT DEFAULT 0,OverloadVehicle INT DEFAULT 0,[DateUnit] INT,Axle1 INT DEFAULT 0,Axle2 INT DEFAULT 0,Axle3 INT DEFAULT 0,Axle4 INT DEFAULT 0,Axle5 INT DEFAULT 0,Axle6 INT DEFAULT 0,Axle7 INT DEFAULT 0,AxleRemaining INT DEFAULT 0,GrossVehicleWeight INT DEFAULT 0)
            
            INSERT INTO #T([DateUnit],TotalVehicle,OverloadVehicle,Axle1,Axle2,Axle3,Axle4,Axle5,Axle6,Axle7,AxleRemaining,GrossVehicleWeight)
            SELECT 
            DATEPART(MONTH,DateTime) AS DateUnit
            ,COUNT(1) AS TotalVehicle
            ,SUM(CAST(IsOverloaded AS INT)) AS OverloadVehicle
            ,SUM(Axle1) AS Axle1,SUM(Axle2) AS Axle2,SUM(Axle3) AS Axle3,SUM(Axle4) AS Axle4,SUM(Axle5) AS Axle5,SUM(Axle6) AS Axle6,SUM(Axle7) AS Axle7
            ,SUM(AxleRemaining) AS AxleRemaining,SUM(GrossVehicleWeight) AS GrossVehicleWeight
            FROM AxleLoad AL ";

        query += this.GetFilterClause(reportParameters);

        query += @" GROUP BY 
                DATEPART(MONTH,DateTime)

    
                DECLARE @DateParts TABLE(MonthNumber INT)

                DECLARE @Min INT,@Max INT
                SELECT @Min=DATEPART(MONTH,@DateStart),@Max=DATEPART(MONTH,@DateEnd)

                INSERT INTO @DateParts
                SELECT N.number
                FROM master..spt_values as N
                WHERE N.number >= @Min AND N.number <= @Max
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
            TimeStart = reportParameters.TimeStart.ToTimeSpan(),
            TimeEnd = reportParameters.TimeEnd.ToTimeSpan()
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
    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetWeeklyOverloadedTimeSeriesReport(ReportParameters reportParameters)
    {
        bool isSuccess = false;
        string message = "";
        string query = this.GetStationTableQuery(reportParameters) + @"
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
        ";

        query += this.GetFilterClause(reportParameters);

        query += @" GROUP BY DATEPART(WEEKDAY, AL.DateTime)

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
            TimeStart = reportParameters.TimeStart.ToTimeSpan(),
            TimeEnd = reportParameters.TimeEnd.ToTimeSpan()
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
    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetDailyOverloadedTimeSeriesReport(ReportParameters reportParameters)
    {
        bool isSuccess = false;
        string message = "";
        string query = this.GetStationTableQuery(reportParameters) + @"
        DECLARE @Days TABLE([Date] DATE)
        DECLARE @CurrentDate DATE = @DateStart

        WHILE @CurrentDate <= @DateEnd
        BEGIN
            INSERT INTO @Days VALUES(@CurrentDate)
            SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate)
        END

        CREATE TABLE #T(
            TotalVehicle INT DEFAULT 0,
            OverloadVehicle INT DEFAULT 0,
            [Date] DATE,
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

        INSERT INTO #T([Date], TotalVehicle, OverloadVehicle, Axle1, Axle2, Axle3, Axle4, Axle5, Axle6, Axle7, AxleRemaining, GrossVehicleWeight)
        SELECT 
            D.[Date],
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
        FROM @Days D LEFT JOIN AxleLoad AL ON CAST(AL.DateTime AS DATE) = D.[Date]
        ";

        query += this.GetFilterClause(reportParameters);

        query += @" GROUP BY D.[Date]

        SELECT *,
            CAST([Date] AS VARCHAR) AS DateUnitName
        FROM #T
        ORDER BY [Date]

        DROP TABLE #T
        ";

        var parameters = new
        {
            DateStart = reportParameters.DateStart,
            DateEnd = reportParameters.DateEnd,
            NumberOfAxle = reportParameters.NumberOfAxle,
            Wheelbase = reportParameters.Wheelbase,
            ClassStatus = reportParameters.ClassStatus,
            TimeStart = reportParameters.TimeStart.ToTimeSpan(),
            TimeEnd = reportParameters.TimeEnd.ToTimeSpan()
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
    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetHourlyOverloadedTimeSeriesReport(ReportParameters reportParameters)
    {
        bool isSuccess = false;
        string message = "";
        string query = this.GetStationTableQuery(reportParameters) + @"
        DECLARE @DateRange TABLE([Date] DATE)
        DECLARE @CurrentDate DATE = @DateStart

        WHILE @CurrentDate <= @DateEnd
        BEGIN
            INSERT INTO @DateRange VALUES(@CurrentDate)
            SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate)
        END

        DECLARE @AllHours TABLE([Hour] INT)
        INSERT INTO @AllHours([Hour])
        VALUES (0), (1), (2), (3), (4), (5), (6), (7), (8), (9), (10), (11), (12), (13), (14), (15), (16), (17), (18), (19), (20), (21), (22), (23)

        CREATE TABLE #T(
            TotalVehicle INT DEFAULT 0,
            OverloadVehicle INT DEFAULT 0,
            [DateUnit] INT 
        )

        INSERT INTO #T([DateUnit], TotalVehicle, OverloadVehicle)
        SELECT 
            DATEPART(HOUR, AL.DateTime) AS DateUnit,
            COUNT(1) AS TotalVehicle,
            SUM(CAST(IsOverloaded AS INT)) AS OverloadVehicle
        FROM AxleLoad AL
        ";

        query += this.GetFilterClause(reportParameters);

        query += @" GROUP BY DATEPART(HOUR, AL.DateTime)

        SELECT 
            AH.Hour AS DateUnit,
            ISNULL(T.TotalVehicle, 0) AS TotalVehicle,
            ISNULL(T.OverloadVehicle, 0) AS OverloadVehicle,
            FORMAT(AH.Hour, '00') AS DateUnitName
        FROM @AllHours AH
        LEFT JOIN #T T ON AH.Hour = T.DateUnit
        ORDER BY AH.Hour

        DROP TABLE #T
        ";

        var parameters = new
        {
            DateStart = reportParameters.DateStart,
            DateEnd = reportParameters.DateEnd,
            NumberOfAxle = reportParameters.NumberOfAxle,
            Wheelbase = reportParameters.Wheelbase,
            ClassStatus = reportParameters.ClassStatus,
            TimeStart = reportParameters.TimeStart.ToTimeSpan(),
            TimeEnd = reportParameters.TimeEnd.ToTimeSpan()
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
    #endregion
    
    #region Overloaded Number of Axles report query
    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetYearlyOverloadedNumberOfAxlesReport(ReportParameters reportParameters)
    {
        bool isSuccess = false;
        string message = "";
        string query = this.GetStationTableQuery(reportParameters) + @"
        DECLARE @Years TABLE([Year] INT)
        DECLARE @CurrentYear INT = YEAR(@DateStart)

        WHILE @CurrentYear <= YEAR(@DateEnd)
        BEGIN
            INSERT INTO @Years VALUES(@CurrentYear)
            SET @CurrentYear = @CurrentYear + 1
        END

        SELECT 
            Y.[Year] AS DateUnit,  -- Year as DateUnit
            CAST(Y.[Year] AS VARCHAR) AS DateUnitName,  -- Year name
            AL.NumberOfAxle,
            ISNULL(COUNT(1), 0) AS TotalVehicle,  -- Total number of vehicles (overloaded + non-overloaded)
            ISNULL(SUM(CAST(AL.IsOverloaded AS INT)), 0) AS OverloadVehicle  -- Overloaded vehicle count
        FROM @Years Y LEFT JOIN AxleLoad AL ON YEAR(AL.DateTime) = Y.[Year]
            ";

        query += this.GetFilterClause(reportParameters);
        query += " AND AL.NumberOfAxle >=2 AND AL.NumberOfAxle <= 7";
        query += @" GROUP BY Y.[Year], AL.NumberOfAxle
        ORDER BY Y.[Year]
        ";

        var parameters = new
        {
            DateStart = reportParameters.DateStart,
            DateEnd = reportParameters.DateEnd,
            NumberOfAxle = reportParameters.NumberOfAxle,
            Wheelbase = reportParameters.Wheelbase,
            ClassStatus = reportParameters.ClassStatus,
            TimeStart = reportParameters.TimeStart.ToTimeSpan(),
            TimeEnd = reportParameters.TimeEnd.ToTimeSpan()
        };

        try
        {
            // Fetch the report data from the database
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
    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetMonthlyOverloadedNumberOfAxlesReport(ReportParameters reportParameters)
    {
        bool isSuccess = false;
        string message = "";
        string query = this.GetStationTableQuery(reportParameters) + @"
        DECLARE @Months TABLE([Month] INT, [MonthName] NVARCHAR(50))
        DECLARE @CurrentMonth INT = MONTH(@DateStart)
        DECLARE @CurrentYear INT = YEAR(@DateStart)

        WHILE @CurrentYear < YEAR(@DateEnd) OR (@CurrentYear = YEAR(@DateEnd) AND @CurrentMonth <= MONTH(@DateEnd))
        BEGIN
            INSERT INTO @Months VALUES(@CurrentMonth, DATENAME(MONTH, DATEFROMPARTS(@CurrentYear, @CurrentMonth, 1)))
            SET @CurrentMonth = @CurrentMonth + 1
            IF @CurrentMonth > 12
            BEGIN
                SET @CurrentMonth = 1
                SET @CurrentYear = @CurrentYear + 1
            END
        END

        SELECT 
            M.[Month] AS DateUnit,  
            M.[MonthName] AS DateUnitName, 
            AL.NumberOfAxle,
            COUNT(1) AS TotalVehicle, 
            SUM(CAST(AL.IsOverloaded AS INT)) AS OverloadVehicle 
        FROM @Months M LEFT JOIN AxleLoad AL ON MONTH(AL.DateTime) = M.[Month]
        ";

        query += this.GetFilterClause(reportParameters);
        query += " AND AL.NumberOfAxle >=2 AND AL.NumberOfAxle <= 7";
        query += @" GROUP BY M.[Month], M.[MonthName], AL.NumberOfAxle
        ORDER BY M.[Month]
        ";

        var parameters = new
        {
            DateStart = reportParameters.DateStart,
            DateEnd = reportParameters.DateEnd,
            NumberOfAxle = reportParameters.NumberOfAxle,
            Wheelbase = reportParameters.Wheelbase,
            ClassStatus = reportParameters.ClassStatus,
            TimeStart = reportParameters.TimeStart.ToTimeSpan(),
            TimeEnd = reportParameters.TimeEnd.ToTimeSpan()
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
    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetWeeklyOverloadedNumberOfAxlesReport(ReportParameters reportParameters)
    {
        bool isSuccess = false;
        string message = "";
        string query = this.GetStationTableQuery(reportParameters) + @"
        DECLARE @DateRange TABLE([Date] DATE)
        DECLARE @CurrentDate DATE = @DateStart

        WHILE @CurrentDate <= @DateEnd
        BEGIN
            INSERT INTO @DateRange VALUES(@CurrentDate)
            SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate)
        END

        SELECT 
            DATEPART(WEEKDAY, AL.DateTime) AS DateUnit,  -- Numeric representation of the day (1-7)
            DATENAME(WEEKDAY, AL.DateTime) AS DateUnitName,  -- Name of the day (e.g., Sunday)
            COUNT(1) AS TotalVehicle,  -- Total number of vehicles (overloaded + non-overloaded)
            AL.NumberOfAxle,
            SUM(CAST(AL.IsOverloaded AS INT)) AS OverloadVehicle  -- Overloaded vehicle count
        FROM AxleLoad AL
        ";

        query += this.GetFilterClause(reportParameters);
        query += " AND AL.NumberOfAxle >=2 AND AL.NumberOfAxle <= 7";
        query += @" GROUP BY DATEPART(WEEKDAY, AL.DateTime), DATENAME(WEEKDAY, AL.DateTime), AL.NumberOfAxle
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
            TimeStart = reportParameters.TimeStart.ToTimeSpan(),
            TimeEnd = reportParameters.TimeEnd.ToTimeSpan()
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
    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetDailyOverloadedNumberOfAxlesReport(ReportParameters reportParameters)
    {
        bool isSuccess = false;
        string message = "";
        string query = this.GetStationTableQuery(reportParameters) + @"
        DECLARE @Days TABLE([Date] DATE)
        DECLARE @CurrentDate DATE = @DateStart

        WHILE @CurrentDate <= @DateEnd
        BEGIN
            INSERT INTO @Days VALUES(@CurrentDate)
            SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate)
        END

        SELECT 
            DAY(D.[Date]) AS DateUnit,  
            CONVERT(NVARCHAR, D.[Date], 23) AS DateUnitName, 
            AL.NumberOfAxle,
            COUNT(AL.StationId) AS TotalVehicle,
            SUM(CAST(AL.IsOverloaded AS INT)) AS OverloadVehicle
        FROM @Days D
        LEFT JOIN AxleLoad AL ON CAST(AL.DateTime AS DATE) = D.[Date]
            ";

        query += this.GetFilterClause(reportParameters);
        query += @" AND AL.NumberOfAxle >=2 AND AL.NumberOfAxle <= 7";
        query += @" GROUP BY D.[Date], AL.NumberOfAxle
        ORDER BY AL.NumberOfAxle";

        var parameters = new
        {
            DateStart = reportParameters.DateStart,
            DateEnd = reportParameters.DateEnd,
            NumberOfAxle = reportParameters.NumberOfAxle,
            Wheelbase = reportParameters.Wheelbase,
            ClassStatus = reportParameters.ClassStatus,
            TimeStart = reportParameters.TimeStart.ToTimeSpan(),
            TimeEnd = reportParameters.TimeEnd.ToTimeSpan()
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
    public async Task<(IEnumerable<AxleLoadReport>, bool, string)> GetHourlyOverloadedNumberOfAxlesReport(ReportParameters reportParameters)
    {
        bool isSuccess = false;
        string message = "";
        string query = this.GetStationTableQuery(reportParameters) + @"
        DECLARE @DateRange TABLE([Date] DATE)
        DECLARE @CurrentDate DATE = @DateStart

        WHILE @CurrentDate <= @DateEnd
        BEGIN
            INSERT INTO @DateRange VALUES(@CurrentDate)
            SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate)
        END

        DECLARE @AllHours TABLE([Hour] INT)
        INSERT INTO @AllHours([Hour])
        VALUES (0), (1), (2), (3), (4), (5), (6), (7), (8), (9), (10), (11), (12), (13), (14), (15), (16), (17), (18), (19), (20), (21), (22), (23)

        CREATE TABLE #T(
            DateUnit INT,
            TotalVehicle INT DEFAULT 0,
            OverloadVehicle INT DEFAULT 0,
            NumberOfAxle INT DEFAULT 0
        )

        -- Insert total and overloaded vehicle counts along with the hour (DateUnit) and NumberOfAxle
        INSERT INTO #T([DateUnit], TotalVehicle, OverloadVehicle, NumberOfAxle)
        SELECT
            DATEPART(HOUR, AL.DateTime) AS DateUnit,
            COUNT(1) AS TotalVehicle,  -- Total vehicles count (overloaded and non-overloaded)
            SUM(CAST(AL.IsOverloaded AS INT)) AS OverloadVehicle,  -- Overloaded vehicles count
            AL.NumberOfAxle
        FROM AxleLoad AL
        ";

        query += this.GetFilterClause(reportParameters);
        query += " AND AL.NumberOfAxle >=2 AND AL.NumberOfAxle <= 7";
        query += @" GROUP BY DATEPART(HOUR, AL.DateTime), AL.NumberOfAxle

        SELECT 
            AH.Hour AS DateUnit,
            ISNULL(T.TotalVehicle, 0) AS TotalVehicle,  -- Total vehicle count
            ISNULL(T.OverloadVehicle, 0) AS OverloadVehicle,  -- Overloaded vehicle count
            ISNULL(T.NumberOfAxle, 0) AS NumberOfAxle,
            FORMAT(AH.Hour, '00') AS DateUnitName
        FROM @AllHours AH
        LEFT JOIN #T T ON AH.Hour = T.DateUnit
        ORDER BY AH.Hour

        DROP TABLE #T
        ";

        var parameters = new
        {
            DateStart = reportParameters.DateStart,
            DateEnd = reportParameters.DateEnd,
            NumberOfAxle = reportParameters.NumberOfAxle,
            Wheelbase = reportParameters.Wheelbase,
            ClassStatus = reportParameters.ClassStatus,
            TimeStart = reportParameters.TimeStart.ToTimeSpan(),
            TimeEnd = reportParameters.TimeEnd.ToTimeSpan()
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
    #endregion

    private string GetStationTableQuery(ReportParameters reportParameters)
    {
        if(reportParameters.Stations.Count == 1)
        {
            return "";
        }

        string query;

        if (reportParameters.WIMType > 0 || reportParameters.UpboundDirection || reportParameters.DownboundDirection)
        {
            string stationIds = string.Join(",", reportParameters.Stations.Select(s => s));
            query = @" DECLARE @Stations TABLE(AutoId INT IDENTITY(1,1), StationId INT,LaneNo INT)
            INSERT INTO @Stations(StationId,LaneNo)
            SELECT StationId,LaneNumber
            FROM WIMScale
            WHERE StationId IN (" + stationIds + ")";
            if(reportParameters.WIMType > 0)
            {
                query += " AND Type=" + reportParameters.WIMType;
            }
            else if(reportParameters.UpboundDirection)
            {
                query += " AND IsUpbound=1";
            }
            else if (reportParameters.DownboundDirection)
            {
                query += " AND IsUpbound=0";
            }

            return query;
        }
        else
        {
            string stationIds = string.Join(",", reportParameters.Stations.Select(s => "(" + s + ")"));
            query = @" DECLARE @Stations TABLE(AutoId INT IDENTITY(1,1), StationId INT)
            INSERT INTO @Stations(StationId) VALUES " + stationIds + " ";
        }
        
        return query;
    }
    private string GetFilterClause(ReportParameters reportParameters)
    {
        string joining = String.Empty;
        string whereClause = @" WHERE DATEDIFF(Day, AL.DateTime, @DateStart) <= 0
            AND DATEDIFF(Day, AL.DateTime, @DateEnd) >= 0";
        
        if(reportParameters.Stations.Count() == 1)
        {
            whereClause += " AND AL.StationId=" + reportParameters.Stations.FirstOrDefault();
        }
        else
        {
            if (reportParameters.WIMType > 0 || reportParameters.UpboundDirection || reportParameters.DownboundDirection)
            {
                joining = " INNER JOIN @Stations S ON AL.StationId = S.StationId AND AL.LaneNumber=S.LaneNo";
            }
            else
            {
                joining = " INNER JOIN @Stations S ON AL.StationId = S.StationId";
            }
        }
        if (reportParameters.TimeStart != reportParameters.TimeEnd)
        {
            whereClause += " AND CAST(AL.DateTime AS TIME) >= @TimeStart AND CAST(AL.DateTime AS TIME) <=@TimeEnd";
        }
        if (reportParameters.WIMScales is not null && reportParameters.WIMScales.Any())
        {
            if (reportParameters.WIMScales.Count() == 1)
            {
                whereClause += " AND AL.LaneNumber = " + reportParameters.WIMScales.FirstOrDefault().LaneNumber;
            }
            else
            {
                whereClause += " AND AL.LaneNumber IN (" + string.Join(",", reportParameters.WIMScales.Select(ws => "(" + ws.LaneNumber + ")")) + ")";
            }
        }
        if (reportParameters.NumberOfAxle is not null && reportParameters.NumberOfAxle.Any())
        {
            if (reportParameters.NumberOfAxle.Count() == 1)
            {
                whereClause += " AND AL.NumberOfAxle = " + reportParameters.NumberOfAxle.FirstOrDefault();
            }
            else
            {
                whereClause += " AND AL.NumberOfAxle IN (" + string.Join(",", reportParameters.NumberOfAxle.Select(na => "(" + na + ")")) + ")";
            }
        }
        if (!string.IsNullOrEmpty(reportParameters.WeightFilterColumn))
        {
            whereClause += " AND (AL." + reportParameters.WeightFilterColumn + ">=" + reportParameters.WeightMin + " AND AL." + reportParameters.WeightFilterColumn + "<=" + reportParameters.WeightMax + ")";
        }
        if (reportParameters.Wheelbase > 0)
        {
            whereClause += " AND Wheelbase = @Wheelbase";
        }
        if (reportParameters.ClassStatus > 0)
        {
            whereClause += " AND ClassStatus = @ClassStatus";
        }

        if (!string.IsNullOrEmpty(joining))
        {
            return joining + whereClause;
        }
        
        return whereClause;
    }

    private string _overloadSelectQuery { get; set; } = " (CASE WHEN AL.GrossVehicleWeight>OL.AllowedWeight THEN 1 ELSE 0 END) ";
    private string _overloadCountQuery { get; set; } = " SUM(CASE WHEN AL.GrossVehicleWeight>OL.AllowedWeight THEN 1 ELSE 0 END) ";
    private string _overloadJoiningQuery { get; set; } = " LEFT JOIN ConfigurationOverloadWeight OL ON AL.NumberOfAxle=OL.AxleNumber ";
    private (string, string) GetOverloadingQuery(bool isCount = false)
    {
        string selectQuery = " (CASE WHEN AL.GrossVehicleWeight>OL.AllowedWeight THEN 1 ELSE 0 END) ";
        if(isCount)
        {
            selectQuery = " SUM(CASE WHEN AL.GrossVehicleWeight>OL.AllowedWeight THEN 1 ELSE 0 END) ";
        }
        string joiningQuery = " LEFT JOIN ConfigurationOverloadWeight OL ON AL.NumberOfAxle=OL.AxleNumber ";
        return (selectQuery, joiningQuery);
    }
}
