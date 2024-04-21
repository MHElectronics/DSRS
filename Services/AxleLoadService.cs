using BOL;
using Services.Helpers;

namespace Services;

public interface IAxleLoadService
{
    Task<bool> Add(LoadData obj);
    Task<bool> Add(List<LoadData> obj);
    Task<bool> Delete(DateTime date);
}

public class AxleLoadService(ISqlDataAccess _db) : IAxleLoadService
{
    public async Task<bool> Add(LoadData obj)
    {
        string query = @"INSERT INTO AxleLoad(StationId,TransactionNumber,LaneNumber,DateTime,PlateZone,PlateSeries,PlateNumber,NumberOfAxle,VehicleSpeed
            ,Axle1,Axle2,Axle3,Axle4,Axle5,Axle6,Axle7
            ,AxleRemaining,GrossVehicleWeight,IsUnloaded,IsOverloaded,OverSizedModified,Wheelbase,ReceiptNumber,BillNumber
            ,Axle1Time,Axle2Time,Axle3Time,Axle4Time,Axle5Time,Axle6Time,Axle7Time)

            VALUES(@StationId,@TransactionNumber,@LaneNumber,@DateTime,@PlateZone,@PlateSeries,@PlateNumber,@NumberOfAxle,@VehicleSpeed
            ,@Axle1,@Axle2,@Axle3,@Axle4,@Axle5,@Axle6,@Axle7
            ,@AxleRemaining,@GrossVehicleWeight,@IsUnloaded,@IsOverloaded,@OverSizedModified,@Wheelbase,@ReceiptNumber,@BillNumber
            ,@Axle1Time,@Axle2Time,@Axle3Time,@Axle4Time,@Axle5Time,@Axle6Time,@Axle7Time)";

        return await _db.SaveData(query, obj);
    }
    public async Task<bool> Add(List<LoadData> obj)
    {
        string query = @"INSERT INTO AxleLoad(StationId,TransactionNumber,LaneNumber,DateTime,PlateZone,PlateSeries,PlateNumber,NumberOfAxle,VehicleSpeed
            ,Axle1,Axle2,Axle3,Axle4,Axle5,Axle6,Axle7
            ,AxleRemaining,GrossVehicleWeight,IsUnloaded,IsOverloaded,OverSizedModified,Wheelbase,ReceiptNumber,BillNumber
            ,Axle1Time,Axle2Time,Axle3Time,Axle4Time,Axle5Time,Axle6Time,Axle7Time)

            VALUES(@StationId,@TransactionNumber,@LaneNumber,@DateTime,@PlateNumber,@PlateZone,@PlateSeries,@NumberOfAxle,@VehicleSpeed
            ,@Axle1,@Axle2,@Axle3,@Axle4,@Axle5,@Axle6,@Axle7
            ,@AxleRemaining,@GrossVehicleWeight,@IsUnloaded,@IsOverloaded,@OverSizedModified,@Wheelbase,@ReceiptNumber,@BillNumber
            ,@Axle1Time,@Axle2Time,@Axle3Time,@Axle4Time,@Axle5Time,@Axle6Time,@Axle7Time)";

        return await _db.SaveData(query, obj);
    }

    public async Task<bool> Delete(DateTime date)
    {
        string query = "DELETE FROM AxleLoad WHERE DATEDIFF(DAY,DateTime,@Date)=0";

        return await _db.SaveData(query, new { Date = date });
    }
}
